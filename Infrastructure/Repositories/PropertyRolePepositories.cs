// RealEstate.Infrastructure/Repositories/PropertyRoleRepository.cs
using Google.Cloud.Firestore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;

namespace RealEstate.Infrastructure.Repositories;

public class PropertyRoleRepository : FirestoreRepository<PropertyRole>, IPropertyRoleRepository
{
    public PropertyRoleRepository(FirestoreDb db) : base(db, "property_roles") { }

    public async Task<IEnumerable<PropertyRole>> GetUserRolesForPropertyAsync(string userId, string propertyId)
    {
        var query = _db.Collection(_collectionName)
            .WhereEqualTo("UserId", userId)
            .WhereEqualTo("PropertyId", propertyId);
        
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<PropertyRole>()).ToList();
    }

    public async Task<IEnumerable<PropertyRole>> GetPropertyRolesAsync(string propertyId)
    {
        var query = _db.Collection(_collectionName).WhereEqualTo("PropertyId", propertyId);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<PropertyRole>()).ToList();
    }

    public async Task<bool> HasRoleAsync(string userId, string propertyId, UserRole role)
    {
        var query = _db.Collection(_collectionName)
            .WhereEqualTo("UserId", userId)
            .WhereEqualTo("PropertyId", propertyId)
            .WhereEqualTo("Role", role);
        
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Any();
    }
}