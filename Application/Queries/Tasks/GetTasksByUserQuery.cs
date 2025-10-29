// RealEstate.Application/Queries/Tasks/GetTasksByUserQuery.cs
using MediatR;
using RealEstate.Application.DTOs;

namespace RealEstate.Application.Queries.Tasks;

public record GetTasksByUserQuery(string UserId) : IRequest<List<TaskDto>>;