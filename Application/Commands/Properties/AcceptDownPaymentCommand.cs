// RealEstate.Application/Commands/Properties/AcceptDownPaymentCommand.cs
using MediatR;
using RealEstate.Application.DTOs;

namespace RealEstate.Application.Commands.Properties;

public record AcceptDownPaymentCommand(
    string PropertyId,
    decimal Amount,
    string CloserId) : IRequest<PropertyDto>;