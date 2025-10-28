// RealEstate.Application/Commands/Tasks/CreateTaskCommand.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Domain.Enums;

namespace RealEstate.Application.Commands.Tasks;

public record CreateTaskCommand(
    string PropertyId,
    string Title,
    string? Description,
    TaskType TaskType,
    string AssignedTo,
    DateTime DueDate,
    string CreatedBy) : IRequest<TaskDto>;