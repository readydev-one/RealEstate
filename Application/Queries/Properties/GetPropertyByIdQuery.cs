// RealEstate.Application/Queries/Properties/GetPropertyByIdQuery.cs
using MediatR;
using RealEstate.Application.DTOs;

namespace RealEstate.Application.Queries.Properties;

public record GetPropertyByIdQuery(string PropertyId, string UserId) : IRequest<PropertyDto>;