// RealEstate.Application/Commands/Tasks/UpdateTaskStatusCommand.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Domain.Enums;

namespace RealEstate.Application.Commands.Tasks;

public record UpdateTaskStatusCommand(
    string TaskId,
    TaskStatus Status,
    string UserId) : IRequest<TaskDto>;