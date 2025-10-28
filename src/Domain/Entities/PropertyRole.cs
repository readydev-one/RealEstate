// RealEstate.Domain/Entities/PropertyRole.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Domain.Entities;

public class PropertyRole : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string PropertyId { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsLocked { get; set; }
}