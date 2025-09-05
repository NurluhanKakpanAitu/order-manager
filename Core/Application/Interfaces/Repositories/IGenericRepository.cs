using System.Linq.Expressions;
using Application.DTOs.Common;
using Application.Interfaces;
using Domain.Entities.Base;

namespace Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    // Методы с маппингом для избежания загрузки всех данных
    Task<TResult?> GetByIdMappedAsync<TResult>(Guid id,  CancellationToken cancellationToken = default);
    Task<IEnumerable<TResult>> GetAllMappedAsync<TResult>(CancellationToken cancellationToken = default);
    Task<IEnumerable<TResult>> FindMappedAsync<TResult>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<PaginatedResult<TResult>> GetPaginatedAsync<TResult>(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResult<TResult>> GetPaginatedAsync<TResult>(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}