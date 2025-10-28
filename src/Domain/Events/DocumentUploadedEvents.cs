// RealEstate.Domain/Events/DocumentUploadedEvent.cs
namespace RealEstate.Domain.Events;

public class DocumentUploadedEvent : IDomainEvent
{
    public string DocumentId { get; set; } = string.Empty;
    public string PropertyId { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}