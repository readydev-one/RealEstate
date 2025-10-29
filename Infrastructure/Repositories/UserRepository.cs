// RealEstate.Infrastructure/Repositories/UserRepository.cs
using Google.Cloud.Firestore;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;

namespace RealEstate.Infrastructure.Repositories;

public class UserRepository : FirestoreRepository<User>, IUserRepository
{
    public UserRepository(FirestoreDb db) : base(db, "users") { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var query = _db.Collection(_collectionName).WhereEqualTo("Email", email);
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.FirstOrDefault()?.ConvertTo<User>();
    }

    public async Task<User?> GetByOAuthSubjectAsync(string provider, string subject)
    {
        var query = _db.Collection(_collectionName)
            .WhereEqualTo("OAuthProvider", provider)
            .WhereEqualTo("OAuthSubject", subject);
        
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.FirstOrDefault()?.ConvertTo<User>();
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        // Note: This requires querying PropertyRoles, implemented in application layer
        var allUsers = await GetAllAsync();
        return allUsers.ToList();
    }

    public async Task<IEnumerable<User>> GetPendingClosersAsync()
    {
        var query = _db.Collection(_collectionName)
            .WhereEqualTo("Status", UserStatus.PendingApproval);
        
        var snapshot = await query.GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<User>()).ToList();
    }
}