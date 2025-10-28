// RealEstate.Domain/Entities/Property.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Domain.Entities;

public class Property : BaseEntity
{
    public List<string> BuyerIds { get; set; } = new();
    public string SellerId { get; set; } = string.Empty;
    public string CloserId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public DateTime ClosingDate { get; set; }
    public PropertyStatus Status { get; set; } = PropertyStatus.Draft;
    public decimal AnnualPropertyTax { get; set; }
    public decimal MonthlyInsurance { get; set; }
    public int EscrowMonths { get; set; } = 6;
    public decimal? DownPaymentAmount { get; set; }
    public DownPaymentStatus? DownPaymentStatus { get; set; }
    public DateTime? DownPaymentAcceptedDate { get; set; }
    public bool AreBuyersLocked { get; set; }
    public decimal? CalculatedProratedTaxes { get; set; }
    public decimal? CalculatedEscrowAmount { get; set; }
    public decimal? CalculatedTotalClosingCosts { get; set; }
    public DateTime? ClosingCostsCalculatedAt { get; set; }
    
    // Encrypted PII
    public string? EncryptedAddress { get; set; }
}