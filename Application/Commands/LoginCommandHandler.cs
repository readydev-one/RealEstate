// RealEstate.UnitTests/Commands/LoginCommandHandlerTests.cs
using FluentAssertions;
using Moq;
using RealEstate.Application.Commands.Auth;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Domain.Exceptions;
using Xunit;

namespace RealEstate.UnitTests.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IPropertyRoleRepository> _propertyRoleRepositoryMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _propertyRoleRepositoryMock = new Mock<IPropertyRoleRepository>();
        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _jwtServiceMock.Object,
            _propertyRoleRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        
        var user = new User
        {
            Id = "user123",
            Email = email,
            Name = "Test User",
            PasswordHash = hashedPassword,
            Status = UserStatus.Active
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        _propertyRoleRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PropertyRole, bool>>>()))
            .ReturnsAsync(new List<PropertyRole>());

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<List<string>>()))
            .Returns("access_token");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        var command = new LoginCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand("invalid@example.com", "password");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123"),
            Status = UserStatus.Active
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        var command = new LoginCommand(user.Email, "WrongPassword");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            Status = UserStatus.Suspended
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        var command = new LoginCommand(user.Email, "Password123");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}

// RealEstate.UnitTests/Commands/CreatePropertyCommandHandlerTests.cs
using FluentAssertions;
using Moq;
using RealEstate.Application.Commands.Properties;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using Xunit;

namespace RealEstate.UnitTests.Commands;

public class CreatePropertyCommandHandlerTests
{
    private readonly Mock<IPropertyRepository> _propertyRepositoryMock;
    private readonly Mock<IEncryptionService> _encryptionServiceMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly CreatePropertyCommandHandler _handler;

    public CreatePropertyCommandHandlerTests()
    {
        _propertyRepositoryMock = new Mock<IPropertyRepository>();
        _encryptionServiceMock = new Mock<IEncryptionService>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new CreatePropertyCommandHandler(
            _propertyRepositoryMock.Object,
            _encryptionServiceMock.Object,
            _eventBusMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesPropertyWithClosingCosts()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            Address = "123 Main St",
            PurchasePrice = 500000,
            ClosingDate = DateTime.UtcNow.AddMonths(2),
            AnnualPropertyTax = 6000,
            MonthlyInsurance = 200,
            EscrowMonths = 6
        };

        var closerId = "closer123";
        var propertyId = "property123";

        _encryptionServiceMock
            .Setup(x => x.Encrypt(request.Address))
            .Returns("encrypted_address");

        _propertyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Property>()))
            .ReturnsAsync(propertyId);

        var command = new CreatePropertyCommand(request, closerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Address.Should().Be(request.Address);
        result.PurchasePrice.Should().Be(request.PurchasePrice);
        result.CloserId.Should().Be(closerId);
        result.CalculatedTotalClosingCosts.Should().BeGreaterThan(0);

        _propertyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Property>()), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<Domain.Events.PropertyUpdatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CalculatesCorrectClosingCosts()
    {
        // Arrange
        var closingDate = new DateTime(2025, 6, 1);
        var request = new CreatePropertyRequest
        {
            Address = "123 Main St",
            PurchasePrice = 500000,
            ClosingDate = closingDate,
            AnnualPropertyTax = 3650, // $10/day
            MonthlyInsurance = 100,
            EscrowMonths = 6
        };

        var propertyId = "property123";
        _encryptionServiceMock.Setup(x => x.Encrypt(It.IsAny<string>())).Returns("encrypted");
        _propertyRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Property>())).ReturnsAsync(propertyId);

        var command = new CreatePropertyCommand(request, "closer123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Prorated taxes: days from June 1 to Dec 31 = 214 days
        // 3650 * (214/365) = ~2140
        // Escrow: ((3650/12) + 100) * 6 = (304.17 + 100) * 6 = 2425
        // Total: ~4565
        result.CalculatedTotalClosingCosts.Should().BeGreaterThan(4000);
        result.CalculatedTotalClosingCosts.Should().BeLessThan(5000);
    }
}

// RealEstate.UnitTests/Commands/ApproveDocumentCommandHandlerTests.cs
using FluentAssertions;
using Moq;
using RealEstate.Application.Commands.Documents;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Domain.Exceptions;
using Xunit;

namespace RealEstate.UnitTests.Commands;

public class ApproveDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IPropertyRepository> _propertyRepositoryMock;
    private readonly Mock<IPropertyRoleRepository> _propertyRoleRepositoryMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly ApproveDocumentCommandHandler _handler;

    public ApproveDocumentCommandHandlerTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _propertyRepositoryMock = new Mock<IPropertyRepository>();
        _propertyRoleRepositoryMock = new Mock<IPropertyRoleRepository>();
        _eventBusMock = new Mock<IEventBus>();
        _handler = new ApproveDocumentCommandHandler(
            _documentRepositoryMock.Object,
            _propertyRepositoryMock.Object,
            _propertyRoleRepositoryMock.Object,
            _eventBusMock.Object);
    }

    [Fact]
    public async Task Handle_CloserApprovesBuyerDocument_MakesVisibleToSeller()
    {
        // Arrange
        var documentId = "doc123";
        var propertyId = "prop123";
        var closerId = "closer123";
        var buyerId = "buyer123";
        var sellerId = "seller123";

        var document = new Document
        {
            Id = documentId,
            PropertyId = propertyId,
            UploadedBy = buyerId,
            Status = DocumentStatus.PendingReview
        };

        var property = new Property
        {
            Id = propertyId,
            CloserId = closerId,
            SellerId = sellerId,
            BuyerIds = new List<string> { buyerId }
        };

        var buyerRole = new PropertyRole
        {
            UserId = buyerId,
            PropertyId = propertyId,
            Role = UserRole.Buyer
        };

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId)).ReturnsAsync(document);
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId)).ReturnsAsync(property);
        _propertyRoleRepositoryMock
            .Setup(x => x.GetUserRolesForPropertyAsync(buyerId, propertyId))
            .ReturnsAsync(new List<PropertyRole> { buyerRole });

        var command = new ApproveDocumentCommand(documentId, closerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(DocumentStatus.Approved);
        _documentRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Document>(d => 
            d.VisibleTo.Contains(sellerId) && 
            d.VisibleTo.Contains(closerId) && 
            d.VisibleTo.Contains(buyerId))), Times.Once);
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<Domain.Events.DocumentApprovedEvent>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonCloserTriesToApprove_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var documentId = "doc123";
        var propertyId = "prop123";
        var closerId = "closer123";
        var unauthorizedUserId = "other123";

        var document = new Document
        {
            Id = documentId,
            PropertyId = propertyId
        };

        var property = new Property
        {
            Id = propertyId,
            CloserId = closerId
        };

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId)).ReturnsAsync(document);
        _propertyRepositoryMock.Setup(x => x.GetByIdAsync(propertyId)).ReturnsAsync(property);

        var command = new ApproveDocumentCommand(documentId, unauthorizedUserId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}

// RealEstate.UnitTests/Services/JwtServiceTests.cs
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Services;
using Xunit;

namespace RealEstate.UnitTests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Jwt:Secret"] = "this-is-a-very-secure-secret-key-for-testing-purposes-only",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            }!)
            .Build();

        _jwtService = new JwtService(configuration);
    }

    [Fact]
    public void GenerateAccessToken_ValidUser_ReturnsToken()
    {
        // Arrange
        var user = new User
        {
            Id = "user123",
            Email = "test@example.com",
            Name = "Test User"
        };
        var roles = new List<string> { "Buyer", "Seller" };

        // Act
        var token = _jwtService.GenerateAccessToken(user, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueToken()
    {
        // Act
        var token1 = _jwtService.GenerateRefreshToken();
        var token2 = _jwtService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = "user123",
            Email = "test@example.com",
            Name = "Test User"
        };
        var token = _jwtService.GenerateAccessToken(user, new List<string>());

        // Act
        var isValid = _jwtService.ValidateToken(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = _jwtService.ValidateToken(invalidToken);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void GetUserIdFromToken_ValidToken_ReturnsUserId()
    {
        // Arrange
        var userId = "user123";
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Name = "Test User"
        };
        var token = _jwtService.GenerateAccessToken(user, new List<string>());

        // Act
        var extractedUserId = _jwtService.GetUserIdFromToken(token);

        // Assert
        extractedUserId.Should().Be(userId);
    }
}

// RealEstate.UnitTests/Services/EncryptionServiceTests.cs
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using RealEstate.Infrastructure.Services;
using Xunit;

namespace RealEstate.UnitTests.Services;

public class EncryptionServiceTests
{
    private readonly EncryptionService _encryptionService;

    public EncryptionServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Encryption:Key"] = "this-is-a-32-byte-encryption-key!"
            }!)
            .Build();

        _encryptionService = new EncryptionService(configuration);
    }

    [Fact]
    public void Encrypt_ValidText_ReturnsEncryptedString()
    {
        // Arrange
        var plainText = "sensitive information";

        // Act
        var encrypted = _encryptionService.Encrypt(plainText);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe(plainText);
    }

    [Fact]
    public void Decrypt_EncryptedText_ReturnsOriginalText()
    {
        // Arrange
        var plainText = "123 Main Street";
        var encrypted = _encryptionService.Encrypt(plainText);

        // Act
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void EncryptDecrypt_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        var plainText = string.Empty;

        // Act
        var encrypted = _encryptionService.Encrypt(plainText);
        var decrypted = _encryptionService.Decrypt(encrypted);

        // Assert
        encrypted.Should().Be(plainText);
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void Encrypt_SameTextMultipleTimes_ReturnsDifferentCipherText()
    {
        // Arrange
        var plainText = "test data";

        // Act
        var encrypted1 = _encryptionService.Encrypt(plainText);
        var encrypted2 = _encryptionService.Encrypt(plainText);

        // Assert
        encrypted1.Should().NotBe(encrypted2); // Different IV each time
        _encryptionService.Decrypt(encrypted1).Should().Be(plainText);
        _encryptionService.Decrypt(encrypted2).Should().Be(plainText);
    }
}

// RealEstate.UnitTests/Validators/CreatePropertyCommandValidatorTests.cs
using FluentAssertions;
using FluentValidation.TestHelper;
using RealEstate.Application.Commands.Properties;
using RealEstate.Application.DTOs;
using RealEstate.Application.Validators;
using Xunit;

namespace RealEstate.UnitTests.Validators;

public class CreatePropertyCommandValidatorTests
{
    private readonly CreatePropertyCommandValidator _validator;

    public CreatePropertyCommandValidatorTests()
    {
        _validator = new CreatePropertyCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new CreatePropertyCommand(
            new CreatePropertyRequest
            {
                Address = "123 Main St",
                PurchasePrice = 500000,
                ClosingDate = DateTime.UtcNow.AddMonths(2),
                AnnualPropertyTax = 6000,
                MonthlyInsurance = 200,
                EscrowMonths = 6
            },
            "closer123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyAddress_FailsValidation()
    {
        // Arrange
        var command = new CreatePropertyCommand(
            new CreatePropertyRequest
            {
                Address = "",
                PurchasePrice = 500000,
                ClosingDate = DateTime.UtcNow.AddMonths(2),
                AnnualPropertyTax = 6000,
                MonthlyInsurance = 200,
                EscrowMonths = 6
            },
            "closer123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Address);
    }

    [Fact]
    public void Validate_NegativePurchasePrice_FailsValidation()
    {
        // Arrange
        var command = new CreatePropertyCommand(
            new CreatePropertyRequest
            {
                Address = "123 Main St",
                PurchasePrice = -100,
                ClosingDate = DateTime.UtcNow.AddMonths(2),
                AnnualPropertyTax = 6000,
                MonthlyInsurance = 200,
                EscrowMonths = 6
            },
            "closer123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.PurchasePrice);
    }

    [Fact]
    public void Validate_PastClosingDate_FailsValidation()
    {
        // Arrange
        var command = new CreatePropertyCommand(
            new CreatePropertyRequest
            {
                Address = "123 Main St",
                PurchasePrice = 500000,
                ClosingDate = DateTime.UtcNow.AddDays(-1),
                AnnualPropertyTax = 6000,
                MonthlyInsurance = 200,
                EscrowMonths = 6
            },
            "closer123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.ClosingDate);
    }

    [Fact]
    public void Validate_InvalidEscrowMonths_FailsValidation()
    {
        // Arrange
        var command = new CreatePropertyCommand(
            new CreatePropertyRequest
            {
                Address = "123 Main St",
                PurchasePrice = 500000,
                ClosingDate = DateTime.UtcNow.AddMonths(2),
                AnnualPropertyTax = 6000,
                MonthlyInsurance = 200,
                EscrowMonths = 25 // Outside valid range
            },
            "closer123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.EscrowMonths);
    }
}