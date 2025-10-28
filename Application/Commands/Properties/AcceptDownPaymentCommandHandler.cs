// RealEstate.Application/Commands/Properties/AcceptDownPaymentCommandHandler.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Enums;
using RealEstate.Domain.Events;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Commands.Properties;

public class AcceptDownPaymentCommandHandler : IRequestHandler<AcceptDownPaymentCommand, PropertyDto>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyRoleRepository _propertyRoleRepository;
    private readonly IEventBus _eventBus;

    public AcceptDownPaymentCommandHandler(
        IPropertyRepository propertyRepository,
        IPropertyRoleRepository propertyRoleRepository,
        IEventBus eventBus)
    {
        _propertyRepository = propertyRepository;
        _propertyRoleRepository = propertyRoleRepository;
        _eventBus = eventBus;
    }

    public async Task<PropertyDto> Handle(AcceptDownPaymentCommand request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
        if (property == null)
            throw new EntityNotFoundException(nameof(Property), request.PropertyId);

        if (property.CloserId != request.CloserId)
            throw new UnauthorizedAccessException("Only the assigned closer can accept down payment.");

        property.DownPaymentAmount = request.Amount;
        property.DownPaymentStatus = DownPaymentStatus.Accepted;
        property.DownPaymentAcceptedDate = DateTime.UtcNow;
        property.AreBuyersLocked = true;

        // Lock all buyer roles
        var buyerRoles = await _propertyRoleRepository.FindAsync(pr => 
            pr.PropertyId == request.PropertyId && pr.Role == UserRole.Buyer);
        
        foreach (var role in buyerRoles)
        {
            role.IsLocked = true;
            await _propertyRoleRepository.UpdateAsync(role);
        }

        await _propertyRepository.UpdateAsync(property);

        await _eventBus.PublishAsync(new DownPaymentAcceptedEvent
        {
            PropertyId = property.Id,
            LockedBuyerIds = property.BuyerIds,
            Amount = request.Amount
        });

        return MapToDto(property);
    }

    private PropertyDto MapToDto(Domain.Entities.Property property) => new()
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