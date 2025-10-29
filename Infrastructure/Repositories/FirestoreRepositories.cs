// RealEstate.Infrastructure/Repositories/FirestoreRepository.cs
using System.Linq.Expressions;
using Google.Cloud.Firestore;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Repositories;

public abstract class FirestoreRepository<T> : IRepository<T> where T : class
{
    protected readonly FirestoreDb _db;
    protected readonly string _collectionName;

    protected FirestoreRepository(FirestoreDb db, string collectionName)
    {
        _db = db;
        _collectionName = collectionName;
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        var docRef = _db.Collection(_collectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        
        return snapshot.Exists ? snapshot.ConvertTo<T>() : null;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var snapshot = await _db.Collection(_collectionName).GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<T>()).ToList();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        var allItems = await GetAllAsync();
        return allItems.Where(predicate.Compile()).ToList();
    }

    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate)
    {
        var items = await FindAsync(predicate);
        return items.FirstOrDefault();
    }

    public async Task<string> AddAsync(T entity)
    {
        var docRef = _db.Collection(_collectionName).Document();
        await docRef.SetAsync(entity);
        return docRef.Id;
    }

    public async Task UpdateAsync(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");

        var id = idProperty.GetValue(entity)?.ToString();
        if (string.IsNullOrEmpty(id))
            throw new InvalidOperationException("Entity Id cannot be null or empty");

        var docRef = _db.Collection(_collectionName).Document(id);
        await docRef.SetAsync(entity, SetOptions.MergeAll);
    }

    public async Task DeleteAsync(string id)
    {
        var docRef = _db.Collection(_collectionName).Document(id);
        await docRef.DeleteAsync();
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var docRef = _db.Collection(_collectionName).Document(id);
        var snapshot = await docRef.GetSnapshotAsync();
        return snapshot.Exists;
    }
}