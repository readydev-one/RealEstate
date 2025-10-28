// RealEstate.IntegrationTests/Controllers/AuthControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RealEstate.Application.DTOs;
using Xunit;

namespace RealEstate.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@realestate.com",
            Password = "Admin@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrEmpty();
        authResponse.RefreshToken.Should().NotBeNullOrEmpty();
        authResponse.User.Email.Should().Be(loginRequest.Email);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "admin@realestate.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

// RealEstate.IntegrationTests/Controllers/PropertiesControllerTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RealEstate.Application.DTOs;
using Xunit;

namespace RealEstate.IntegrationTests.Controllers;

public class PropertiesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public PropertiesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var loginRequest = new LoginRequest
        {
            Email = "admin@realestate.com",
            Password = "Admin@123456"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.AccessToken;
    }

    [Fact]
    public async Task CreateProperty_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreatePropertyRequest
        {
            Address = "456 Test Avenue",
            PurchasePrice = 750000,
            ClosingDate = DateTime.UtcNow.AddMonths(3),
            AnnualPropertyTax = 8000,
            MonthlyInsurance = 250,
            EscrowMonths = 6
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/properties", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var property = await response.Content.ReadFromJsonAsync<PropertyDto>();
        property.Should().NotBeNull();
        property!.Address.Should().Be(request.Address);
        property.PurchasePrice.Should().Be(request.PurchasePrice);
        property.CalculatedTotalClosingCosts.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateProperty_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            Address = "789 Test Street",
            PurchasePrice = 500000,
            ClosingDate = DateTime.UtcNow.AddMonths(2),
            AnnualPropertyTax = 6000,
            MonthlyInsurance = 200,
            EscrowMonths = 6
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/properties", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProperty_ExistingProperty_ReturnsProperty()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a property first
        var createRequest = new CreatePropertyRequest
        {
            Address = "123 Integration Test St",
            PurchasePrice = 600000,
            ClosingDate = DateTime.UtcNow.AddMonths(2),
            AnnualPropertyTax = 7000,
            MonthlyInsurance = 220,
            EscrowMonths = 6
        };

        var createResponse = await _client.PostAsJsonAsync("/api/properties", createRequest);
        var createdProperty = await createResponse.Content.ReadFromJsonAsync<PropertyDto>();

        // Act
        var response = await _client.GetAsync($"/api/properties/{createdProperty!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var property = await response.Content.ReadFromJsonAsync<PropertyDto>();
        property.Should().NotBeNull();
        property!.Id.Should().Be(createdProperty.Id);
    }
}

// RealEstate.IntegrationTests/Controllers/UsersControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RealEstate.Application.DTOs;
using Xunit;

namespace RealEstate.IntegrationTests.Controllers;

public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterCloser_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateCloserRequest
        {
            Name = "John Closer",
            Email = $"closer{Guid.NewGuid()}@example.com",
            Password = "SecurePass123!",
            PhoneNumber = "+1234567890",
            Agency = "Premier Realty"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users/closers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Email.Should().Be(request.Email);
        user.Status.Should().Be(Domain.Enums.UserStatus.PendingApproval);
    }

    [Fact]
    public async Task RegisterCloser_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var email = $"duplicate{Guid.NewGuid()}@example.com";
        var request1 = new CreateCloserRequest
        {
            Name = "Closer One",
            Email = email,
            Password = "SecurePass123!",
            Agency = "Agency One"
        };

        var request2 = new CreateCloserRequest
        {
            Name = "Closer Two",
            Email = email,
            Password = "SecurePass456!",
            Agency = "Agency Two"
        };

        // Act
        await _client.PostAsJsonAsync("/api/users/closers", request1);
        var response = await _client.PostAsJsonAsync("/api/users/closers", request2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

// RealEstate.IntegrationTests/TestFixtures/CustomWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RealEstate.IntegrationTests.TestFixtures;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Use test configuration
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["GoogleCloud:ProjectId"] = "test-project",
                ["GoogleCloud:StorageBucket"] = "test-bucket",
                ["Jwt:Secret"] = "test-secret-key-for-integration-tests-minimum-32-chars",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Encryption:Key"] = "test-encryption-key-32-bytes!!",
                ["BootstrapAdmin:Email"] = "admin@realestate.com",
                ["BootstrapAdmin:Password"] = "Admin@123456",
                ["BootstrapAdmin:Name"] = "Test Admin"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // Configure test services
            // Use Firestore emulator for integration tests
            Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", "localhost:8080");
        });
    }
}

// RealEstate.IntegrationTests/Scenarios/EndToEndPropertyTransactionTests.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RealEstate.Application.DTOs;
using RealEstate.Domain.Enums;
using Xunit;

namespace RealEstate.IntegrationTests.Scenarios;

public class EndToEndPropertyTransactionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EndToEndPropertyTransactionTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompletePropertyTransaction_FromCreationToClosing_Success()
    {
        // Step 1: Login as admin/closer
        var loginRequest = new LoginRequest
        {
            Email = "admin@realestate.com",
            Password = "Admin@123456"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        // Step 2: Create a property
        var createPropertyRequest = new CreatePropertyRequest
        {
            Address = "789 Transaction Test Blvd",
            PurchasePrice = 850000,
            ClosingDate = DateTime.UtcNow.AddMonths(4),
            AnnualPropertyTax = 9000,
            MonthlyInsurance = 300,
            EscrowMonths = 6
        };

        var createPropertyResponse = await _client.PostAsJsonAsync("/api/properties", createPropertyRequest);
        createPropertyResponse.EnsureSuccessStatusCode();
        var property = await createPropertyResponse.Content.ReadFromJsonAsync<PropertyDto>();

        property.Should().NotBeNull();
        property!.CalculatedTotalClosingCosts.Should().BeGreaterThan(0);

        // Step 3: Send invitation to buyer
        var invitationRequest = new CreateInvitationRequest
        {
            Email = $"buyer{Guid.NewGuid()}@example.com",
            Role = UserRole.Buyer,
            PropertyId = property.Id
        };

        var invitationResponse = await _client.PostAsJsonAsync("/api/invitations", invitationRequest);
        invitationResponse.EnsureSuccessStatusCode();
        var invitation = await invitationResponse.Content.ReadFromJsonAsync<InvitationDto>();

        invitation.Should().NotBeNull();
        invitation!.Status.Should().Be(InvitationStatus.Pending);

        // Step 4: Create a task
        var createTaskRequest = new CreateTaskRequest
        {
            PropertyId = property.Id,
            Title = "Complete home inspection",
            Description = "Schedule and complete home inspection",
            TaskType = TaskType.ReviewInspection,
            AssignedTo = authResponse.User.Id,
            DueDate = DateTime.UtcNow.AddDays(30)
        };

        var createTaskResponse = await _client.PostAsJsonAsync("/api/tasks", createTaskRequest);
        createTaskResponse.EnsureSuccessStatusCode();
        var task = await createTaskResponse.Content.ReadFromJsonAsync<TaskDto>();

        task.Should().NotBeNull();
        task!.Status.Should().Be(TaskStatus.NotStarted);

        // Step 5: Verify we can get our tasks
        var myTasksResponse = await _client.GetAsync("/api/tasks/my-tasks");
        myTasksResponse.EnsureSuccessStatusCode();
        var myTasks = await myTasksResponse.Content.ReadFromJsonAsync<List<TaskDto>>();

        myTasks.Should().NotBeNull();
        myTasks!.Should().Contain(t => t.Id == task.Id);
    }
}