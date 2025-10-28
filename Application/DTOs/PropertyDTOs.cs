// RealEstate.Application/DTOs/PropertyDTOs.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Application.DTOs;

public class PropertyDto
{
    public string Id { get; set; } = string.Empty;
    public List<string> BuyerIds { get; set; } = new();
    public string SellerId { get; set; } = string.Empty;
    public string CloserId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public DateTime ClosingDate { get; set; }
    public PropertyStatus Status { get; set; }
    public decimal AnnualPropertyTax { get; set; }
    public decimal MonthlyInsurance { get; set; }
    public int EscrowMonths { get; set; }
    public decimal? DownPaymentAmount { get; set; }
    public DownPaymentStatus? DownPaymentStatus { get; set; }
    public bool AreBuyersLocked { get; set; }
    public decimal? CalculatedTotalClosingCosts { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePropertyRequest
{
    public string Address { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public DateTime ClosingDate { get; set; }
    public decimal AnnualPropertyTax { get; set; }
    public decimal MonthlyInsurance { get; set; }
    public int EscrowMonths { get; set; } = 6;
}

public class UpdatePropertyRequest
{
    public string? Address { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? ClosingDate { get; set; }
    public decimal? AnnualPropertyTax { get; set; }
    public decimal? MonthlyInsurance { get; set; }
    public int? EscrowMonths { get; set; }
}

public class AcceptDownPaymentRequest
{
    public string PropertyId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}