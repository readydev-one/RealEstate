// RealEstate.Application/DTOs/UserDTOs.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Application.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Agency { get; set; }
    public UserStatus Status { get; set; }
    public bool IsOAuthUser { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCloserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Agency { get; set; } = string.Empty;
}

public class ApproveCloserRequest
{
    public string UserId { get; set; } = string.Empty;
}
