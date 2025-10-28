// RealEstate.Application/Interfaces/IDocumentRepository.cs
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;

namespace RealEstate.Application.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetByPropertyIdAsync(string propertyId);
    Task<IEnumerable<Document>> GetVisibleDocumentsForUserAsync(string userId, string propertyId);
    Task<IEnumerable<Document>> GetPendingReviewDocumentsAsync();
}