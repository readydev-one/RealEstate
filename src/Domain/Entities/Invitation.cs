// RealEstate.Domain/Entities/Invitation.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Domain.Entities;

public class Invitation : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string PropertyId { get; set; } = string.Empty;
    public string InvitedBy { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
}