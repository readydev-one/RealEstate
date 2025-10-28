// RealEstate.Application/Commands/Tasks/CreateTaskCommandHandler.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Commands.Tasks;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IPropertyRepository _propertyRepository;

    public CreateTaskCommandHandler(
        ITaskRepository taskRepository,
        IPropertyRepository propertyRepository)
    {
        _taskRepository = taskRepository;
        _propertyRepository = propertyRepository;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId);
        if (property == null)
            throw new EntityNotFoundException(nameof(Property), request.PropertyId);

        var task = new TaskEntity
        {
            PropertyId = request.PropertyId,
            Title = request.Title,
            Description = request.Description,
            TaskType = request.TaskType,
            AssignedTo = request.AssignedTo,
            CreatedBy = request.CreatedBy,
            DueDate = request.DueDate,
            Status = TaskStatus.NotStarted
        };

        var taskId = await _taskRepository.AddAsync(task);
        task.Id = taskId;

        return new TaskDto
        {
            Id = task.Id,
            PropertyId = task.PropertyId,
            Title = task.Title,
            Description = task.Description,
            TaskType = task.TaskType,
            AssignedTo = task.AssignedTo,
            DueDate = task.DueDate,
            Status = task.Status,
            CreatedAt = task.CreatedAt
        };
    }
}