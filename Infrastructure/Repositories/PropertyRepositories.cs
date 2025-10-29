// RealEstate.Infrastructure/Repositories/PropertyRepository.cs
using Google.Cloud.Firestore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Repositories;

public class PropertyRepository : FirestoreRepository<Property>, IPropertyRepository
{
    public PropertyRepository(FirestoreDb db) : base(db, "properties") { }

    public async Task<IEnumerable<Property>> GetByCloserIdAsync(string closerId)
    {
        var query = _db.Collection(_collectionName).WhereEqualTo("CloserId", closerId);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Property>()).ToList();
    }

    public async Task<IEnumerable<Property>> GetByBuyerIdAsync(string buyerId)
    {
        var query = _db.Collection(_collectionName).WhereArrayContains("BuyerIds", buyerId);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Property>()).ToList();
    }

    public async Task<IEnumerable<Property>> GetBySellerIdAsync(string sellerId)
    {
        var query = _db.Collection(_collectionName).WhereEqualTo("SellerId", sellerId);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Property>()).ToList();
    }

    public async Task<IEnumerable<Property>> GetPropertiesNeedingClosingCostsRecalculation()
    {
        var allProperties = await GetAllAsync();
        var yesterday = DateTime.UtcNow.AddDays(-1);
        
        return allProperties.Where(p => 
            p.ClosingCostsCalculatedAt == null || 
            p.ClosingCostsCalculatedAt < yesterday).ToList();
    }
}