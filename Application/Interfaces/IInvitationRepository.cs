// RealEstate.Application/Interfaces/IInvitationRepository.cs
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IInvitationRepository : IRepository<Invitation>
{
    Task<Invitation?> GetByTokenAsync(string token);
    Task<IEnumerable<Invitation>> GetByPropertyIdAsync(string propertyId);
    Task<IEnumerable<Invitation>> GetPendingInvitationsAsync();
}
