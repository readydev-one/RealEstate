// RealEstate.Infrastructure/Repositories/InvitationRepository.cs
using Google.Cloud.Firestore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;

namespace RealEstate.Infrastructure.Repositories;

public class InvitationRepository : FirestoreRepository<Invitation>, IInvitationRepository
{
    public InvitationRepository(FirestoreDb db) : base(db, "invitations") { }

    public async Task<Invitation?> GetByTokenAsync(string token)
    {
        var query = _db.Collection(_collectionName).WhereEqualTo("Token", token);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.FirstOrDefault()?.ConvertTo<Invitation>();
    }

    public async Task<IEnumerable<Invitation>> GetByPropertyIdAsync(string propertyId)
    {
        var query = _db.Collection(_collectionName).WhereEqualTo("PropertyId", propertyId);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Invitation>()).ToList();
    }

    public async Task<IEnumerable<Invitation>> GetPendingInvitationsAsync()
    {
        var query = _db.Collection(_collectionName)
            .WhereEqualTo("Status", InvitationStatus.Pending);
        
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Invitation>()).ToList();
    }
}