// RealEstate.Infrastructure/Repositories/DocumentRepository.cs
using Google.Cloud.Firestore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;

namespace RealEstate.Infrastructure.Repositories;

public class DocumentRepository : FirestoreRepository<Document>, IDocumentRepository
{
    public DocumentRepository(FirestoreDb db) : base(db, "documents") { }

    public async Task<IEnumerable<Document>> GetByPropertyIdAsync(string propertyId)
    {
        var query = _db.Collection(_collectionName).WhereEqualTo("PropertyId", propertyId);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Document>()).ToList();
    }

    public async Task<IEnumerable<Document>> GetVisibleDocumentsForUserAsync(string userId, string propertyId)
    {
        var query = _db.Collection(_collectionName)
            .WhereEqualTo("PropertyId", propertyId)
            .WhereArrayContains("VisibleTo", userId);
        
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Document>()).ToList();
    }

    public async Task<IEnumerable<Document>> GetPendingReviewDocumentsAsync()
    {
        var query = _db.Collection(_collectionName)
            .WhereEqualTo("Status", DocumentStatus.PendingReview);
        
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Document>()).ToList();
    }
}