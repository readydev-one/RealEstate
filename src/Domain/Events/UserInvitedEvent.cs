// RealEstate.Domain/Events/UserInvitedEvent.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Domain.Events;

public class UserInvitedEvent : IDomainEvent
{
    public string InvitationId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string PropertyId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}