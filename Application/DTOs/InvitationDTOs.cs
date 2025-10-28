// RealEstate.Application/DTOs/InvitationDTOs.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Application.DTOs;

public class InvitationDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string PropertyId { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInvitationRequest
{
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string PropertyId { get; set; } = string.Empty;
}