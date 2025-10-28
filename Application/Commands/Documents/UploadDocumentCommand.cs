// RealEstate.Application/Commands/Documents/UploadDocumentCommand.cs
using MediatR;
using RealEstate.Application.DTOs;

namespace RealEstate.Application.Commands.Documents;

public record UploadDocumentCommand(
    string PropertyId,
    string UploadedBy,
    string FileName,
    byte[] FileData,
    string ContentType,
    string DocumentType) : IRequest<DocumentDto>;