// RealEstate.Domain/Exceptions/InvitationExpiredException.cs
namespace RealEstate.Domain.Exceptions;

public class InvitationExpiredException : DomainException
{
    public InvitationExpiredException(string invitationId)
        : base($"Invitation '{invitationId}' has expired.") { }
}
