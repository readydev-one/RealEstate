// RealEstate.Domain/Entities/ClosingCosts.cs
namespace RealEstate.Domain.Entities;

public class ClosingCosts : BaseEntity
{
    public string PropertyId { get; set; } = string.Empty;
    public decimal ProratedTaxes { get; set; }
    public decimal EscrowAmount { get; set; }
    public decimal TotalClosingCosts { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}