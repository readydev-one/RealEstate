// RealEstate.Application/Commands/Tasks/UpdateTaskStatusCommandHandler.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Enums;
using RealEstate.Domain.Exceptions;

namespace RealEstate.Application.Commands.Tasks;

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;

    public UpdateTaskStatusCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        if (task == null)
            throw new EntityNotFoundException(nameof(TaskEntity), request.TaskId);

        if (task.AssignedTo != request.UserId)
            throw new UnauthorizedAccessException("Only the assigned user can update task status.");

        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;

        if (request.Status == TaskStatus.Completed)
            task.CompletedAt = DateTime.UtcNow;

        await _taskRepository.UpdateAsync(task);

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