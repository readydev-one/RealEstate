// RealEstate.Application/Commands/Auth/LoginCommandHandler.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;
using BCrypt.Net;

namespace RealEstate.Application.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPropertyRoleRepository _propertyRoleRepository;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IPropertyRoleRepository propertyRoleRepository)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _propertyRoleRepository = propertyRoleRepository;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user == null || user.PasswordHash == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.Status != Domain.Enums.UserStatus.Active)
            throw new UnauthorizedAccessException("Your account is not active.");

        var roles = await GetUserRolesAsync(user.Id);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _userRepository.UpdateAsync(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = MapToUserDto(user)
        };
    }

    private async Task<List<string>> GetUserRolesAsync(string userId)
    {
        var propertyRoles = await _propertyRoleRepository.FindAsync(pr => pr.UserId == userId);
        return propertyRoles.Select(pr => pr.Role.ToString()).Distinct().ToList();
    }

    private UserDto MapToUserDto(Domain.Entities.User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Name = user.Name,
        PhoneNumber = user.PhoneNumber,
        Agency = user.Agency,
        Status = user.Status,
        IsOAuthUser = user.IsOAuthUser,
        CreatedAt = user.CreatedAt
    };
}