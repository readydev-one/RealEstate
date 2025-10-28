// RealEstate.Application/DTOs/ClosingCostsDTOs.cs
namespace RealEstate.Application.DTOs;

public class ClosingCostsDto
{
    public string PropertyId { get; set; } = string.Empty;
    public decimal ProratedTaxes { get; set; }
    public decimal EscrowAmount { get; set; }
    public decimal TotalClosingCosts { get; set; }
    public DateTime CalculatedAt { get; set; }
}