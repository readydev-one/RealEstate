// RealEstate.Domain/Entities/Document.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Domain.Entities;

public class Document : BaseEntity
{
    public string PropertyId { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string StorageUrl { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public List<string> VisibleTo { get; set; } = new();
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
}
