// RealEstate.Application/Interfaces/IJwtService.cs
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, List<string> roles);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    string? GetUserIdFromToken(string token);
}