// RealEstate.Application/Queries/Tasks/GetTasksByUserQueryHandler.cs
using MediatR;
using RealEstate.Application.DTOs;
using RealEstate.Application.Interfaces;

namespace RealEstate.Application.Queries.Tasks;

public class GetTasksByUserQueryHandler : IRequestHandler<GetTasksByUserQuery, List<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;

    public GetTasksByUserQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<List<TaskDto>> Handle(GetTasksByUserQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetByAssignedUserIdAsync(request.UserId);

        return tasks.Select(t => new TaskDto
        {
            Id = t.Id,
            PropertyId = t.PropertyId,
            Title = t.Title,
            Description = t.Description,
            TaskType = t.TaskType,
            AssignedTo = t.AssignedTo,
            DueDate = t.DueDate,
            Status = t.Status,
            CreatedAt = t.CreatedAt
        }).ToList();
    }
}