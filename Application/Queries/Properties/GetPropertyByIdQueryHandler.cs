// RealEstate.Application/Queries/Properties/GetPropertyByIdQueryHandler.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Queries.Properties;

public class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyDto>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyRoleRepository _propertyRoleRepository;

    public GetPropertyByIdQueryHandler(
        IPropertyRepository propertyRepository,
        IPropertyRoleRepository propertyRoleRepository)
    {
        _propertyRepository = propertyRepository;
        _propertyRoleRepository = propertyRoleRepository;
    }

    public async Task<PropertyDto> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
        if (property == null)
            throw new EntityNotFoundException(nameof(Property), request.PropertyId);

        // Check if user has access to this property
        var userRoles = await _propertyRoleRepository.GetUserRolesForPropertyAsync(
            request.UserId, request.PropertyId);

        if (!userRoles.Any() && property.CloserId != request.UserId)
            throw new UnauthorizedAccessException("You do not have access to this property.");

        return new PropertyDto
        {
            Id = property.Id,
            BuyerIds = property.BuyerIds,
            SellerId = property.SellerId,
            CloserId = property.CloserId,
            Address = property.Address,
            PurchasePrice = property.PurchasePrice,
            ClosingDate = property.ClosingDate,
            Status = property.Status,
            AnnualPropertyTax = property.AnnualPropertyTax,
            MonthlyInsurance = property.MonthlyInsurance,
            EscrowMonths = property.EscrowMonths,
            DownPaymentAmount = property.DownPaymentAmount,
            DownPaymentStatus = property.DownPaymentStatus,
            AreBuyersLocked = property.AreBuyersLocked,
            CalculatedTotalClosingCosts = property.CalculatedTotalClosingCosts,
            CreatedAt = property.CreatedAt
        };
    }
}