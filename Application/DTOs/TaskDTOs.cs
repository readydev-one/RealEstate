// RealEstate.Application/DTOs/TaskDTOs.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Application.DTOs;

public class TaskDto
{
    public string Id { get; set; } = string.Empty;
    public string PropertyId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskType TaskType { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTaskRequest
{
    public string PropertyId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskType TaskType { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
}

public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskStatus? Status { get; set; }
}