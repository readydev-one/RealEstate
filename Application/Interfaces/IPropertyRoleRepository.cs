// RealEstate.Application/Interfaces/IPropertyRoleRepository.cs
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;

namespace RealEstate.Application.Interfaces;

public interface IPropertyRoleRepository : IRepository<PropertyRole>
{
    Task<IEnumerable<PropertyRole>> GetUserRolesForPropertyAsync(string userId, string propertyId);
    Task<IEnumerable<PropertyRole>> GetPropertyRolesAsync(string propertyId);
    Task<bool> HasRoleAsync(string userId, string propertyId, UserRole role);
}