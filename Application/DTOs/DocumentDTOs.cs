// RealEstate.Application/DTOs/DocumentDTOs.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Application.DTOs;

public class DocumentDto
{
    public string Id { get; set; } = string.Empty;
    public string PropertyId { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string StorageUrl { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UploadDocumentRequest
{
    public string PropertyId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
}

public class ApproveDocumentRequest
{
    public string DocumentId { get; set; } = string.Empty;
}

public class RejectDocumentRequest
{
    public string DocumentId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}