// RealEstate.Application/Interfaces/IRepository.cs
using System.Linq.Expressions;

namespace RealEstate.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate);
    Task<string> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}