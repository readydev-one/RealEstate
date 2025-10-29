// RealEstate.Application/Queries/Documents/GetDocumentsByPropertyQuery.cs
using MediatR;
using RealEstate.Application.DTOs;

namespace RealEstate.Application.Queries.Documents;

public record GetDocumentsByPropertyQuery(string PropertyId, string UserId) : IRequest<List<DocumentDto>>;