// RealEstate.Infrastructure/Repositories/TaskRepository.cs
using Google.Cloud.Firestore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Repositories;

public class TaskRepository : FirestoreRepository<TaskEntity>, ITaskRepository
{
    public TaskRepository(FirestoreDb db) : base(db, "tasks") { }

    public async Task<IEnumerable<TaskEntity>> GetByPropertyIdAsync(string propertyId)
    {
        var query = _db.Collection(_collectionName).WhereEqualTo("PropertyId", propertyId);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<TaskEntity>()).ToList();
    }

    public async Task<IEnumerable<TaskEntity>> GetByAssignedUserIdAsync(string userId)
    {
        var query = _db.Collection(_collectionName).WhereEqualTo("AssignedTo", userId);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<TaskEntity>()).ToList();
    }

    public async Task<IEnumerable<TaskEntity>> GetUpcomingTasksAsync(int daysAhead)
    {
        var targetDate = DateTime.UtcNow.AddDays(daysAhead);
        var allTasks = await GetAllAsync();
        
        return allTasks.Where(t => 
            t.DueDate <= targetDate && 
            t.Status != Domain.Enums.TaskStatus.Completed &&
            t.Status != Domain.Enums.TaskStatus.Cancelled).ToList();
    }
}