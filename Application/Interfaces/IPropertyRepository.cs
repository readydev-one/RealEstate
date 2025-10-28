// RealEstate.Application/Interfaces/IPropertyRepository.cs
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IPropertyRepository : IRepository<Property>
{
    Task<IEnumerable<Property>> GetByCloserIdAsync(string closerId);
    Task<IEnumerable<Property>> GetByBuyerIdAsync(string buyerId);
    Task<IEnumerable<Property>> GetBySellerIdAsync(string sellerId);
    Task<IEnumerable<Property>> GetPropertiesNeedingClosingCostsRecalculation();
}