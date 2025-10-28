// RealEstate.Application/Commands/Invitations/CreateInvitationCommand.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Domain.Enums;

namespace RealEstate.Application.Commands.Invitations;

public record CreateInvitationCommand(
    string Email,
    UserRole Role,
    string PropertyId,
    string InvitedBy) : IRequest<InvitationDto>;