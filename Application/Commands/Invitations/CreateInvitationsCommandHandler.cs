// RealEstate.Application/Commands/Invitations/CreateInvitationCommandHandler.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Domain.Events;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Commands.Invitations;

public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, InvitationDto>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEventBus _eventBus;

    public CreateInvitationCommandHandler(
        IInvitationRepository invitationRepository,
        IPropertyRepository propertyRepository,
        IUserRepository userRepository,
        IEventBus eventBus)
    {
        _invitationRepository = invitationRepository;
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
        _eventBus = eventBus;
    }

    public async Task<InvitationDto> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
        if (property == null)
            throw new EntityNotFoundException(nameof(Property), request.PropertyId);

        // Check if buyers are locked for buyer invitations
        if (request.Role == UserRole.Buyer && property.AreBuyersLocked)
            throw new BuyersLockedException(request.PropertyId);

        // Check if user already exists with this email
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException($"User with email {request.Email} already exists.");

        var invitation = new Invitation
        {
            Email = request.Email,
            Role = request.Role,
            PropertyId = request.PropertyId,
            InvitedBy = request.InvitedBy,
            Status = InvitationStatus.Pending,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var invitationId = await _invitationRepository.AddAsync(invitation);
        invitation.Id = invitationId;

        await _eventBus.PublishAsync(new UserInvitedEvent
        {
            InvitationId = invitationId,
            Email = request.Email,
            Role = request.Role,
            PropertyId = request.PropertyId,
            Token = invitation.Token,
            ExpiresAt = invitation.ExpiresAt
        });

        return new InvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role,
            PropertyId = invitation.PropertyId,
            Status = invitation.Status,
            ExpiresAt = invitation.ExpiresAt,
            CreatedAt = invitation.CreatedAt
        };
    }
}