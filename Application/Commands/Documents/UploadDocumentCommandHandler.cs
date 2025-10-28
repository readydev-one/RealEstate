// RealEstate.Application/Commands/Documents/UploadDocumentCommandHandler.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Domain.Events;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Commands.Documents;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IStorageService _storageService;
    private readonly IEventBus _eventBus;

    public UploadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IPropertyRepository propertyRepository,
        IStorageService storageService,
        IEventBus eventBus)
    {
        _documentRepository = documentRepository;
        _propertyRepository = propertyRepository;
        _storageService = storageService;
        _eventBus = eventBus;
    }

    public async Task<DocumentDto> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
        if (property == null)
            throw new EntityNotFoundException(nameof(Property), request.PropertyId);

        // Upload to Google Cloud Storage
        var storageUrl = await _storageService.UploadFileAsync(
            request.FileName,
            request.FileData,
            request.ContentType);

        var document = new Document
        {
            PropertyId = request.PropertyId,
            UploadedBy = request.UploadedBy,
            FileName = request.FileName,
            StorageUrl = storageUrl,
            DocumentType = request.DocumentType,
            Status = DocumentStatus.PendingReview,
            FileSizeBytes = request.FileData.Length,
            ContentType = request.ContentType,
            VisibleTo = new List<string> { request.UploadedBy } // Only uploader can see initially
        };

        var documentId = await _documentRepository.AddAsync(document);
        document.Id = documentId;

        await _eventBus.PublishAsync(new DocumentUploadedEvent
        {
            DocumentId = documentId,
            PropertyId = request.PropertyId,
            UploadedBy = request.UploadedBy,
            FileName = request.FileName
        });

        return new DocumentDto
        {
            Id = document.Id,
            PropertyId = document.PropertyId,
            UploadedBy = document.UploadedBy,
            FileName = document.FileName,
            StorageUrl = document.StorageUrl,
            DocumentType = document.DocumentType,
            Status = document.Status,
            CreatedAt = document.CreatedAt
        };
    }
}