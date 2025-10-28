// RealEstate.Application/Interfaces/ITaskRepository.cs
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface ITaskRepository : IRepository<TaskEntity>
{
    Task<IEnumerable<TaskEntity>> GetByPropertyIdAsync(string propertyId);
    Task<IEnumerable<TaskEntity>> GetByAssignedUserIdAsync(string userId);
    Task<IEnumerable<TaskEntity>> GetUpcomingTasksAsync(int daysAhead);
}