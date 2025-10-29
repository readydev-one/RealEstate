// RealEstate.Application/Queries/Documents/GetDocumentsByPropertyQueryHandler.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Queries.Documents;

public class GetDocumentsByPropertyQueryHandler : IRequestHandler<GetDocumentsByPropertyQuery, List<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentsByPropertyQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<List<DocumentDto>> Handle(GetDocumentsByPropertyQuery request, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetVisibleDocumentsForUserAsync(
            request.UserId, request.PropertyId);

        return documents.Select(d => new DocumentDto
        {
            Id = d.Id,
            PropertyId = d.PropertyId,
            UploadedBy = d.UploadedBy,
            FileName = d.FileName,
            StorageUrl = d.StorageUrl,
            DocumentType = d.DocumentType,
            Status = d.Status,
            CreatedAt = d.CreatedAt
        }).ToList();
    }
}