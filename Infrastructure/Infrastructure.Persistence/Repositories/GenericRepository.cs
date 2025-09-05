using System.Linq.Expressions;
using Application.DTOs.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Base;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class GenericRepository<T>(OrderManagerDbContext context, IMapper mapper) : IGenericRepository<T>
    where T : BaseEntity
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null) _dbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity != null;
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    // Методы с маппингом для избежания загрузки всех данных
    public async Task<TResult?> GetByIdMappedAsync<TResult>(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync([id], cancellationToken);
        return entity == null ? default : mapper.Map<TResult>(entity);
    }

    public async Task<IEnumerable<TResult>> GetAllMappedAsync<TResult>(CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .ProjectTo<TResult>(mapper.ConfigurationProvider).ToListAsync(cancellationToken: cancellationToken);
        return  entities;
    }

    public async Task<IEnumerable<TResult>> FindMappedAsync<TResult>(Expression<Func<T, bool>> predicate,CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(predicate)
            .ProjectTo<TResult>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken: cancellationToken);
        
        return entities;
    }

    public async Task<IEnumerable<TResult>> GetPagedMappedAsync<TResult>(int skip, int take, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Skip(skip)
            .Take(take)
            .ProjectTo<TResult>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken: cancellationToken);
        return entities;
    }

    public async Task<IEnumerable<TResult>> GetPagedMappedAsync<TResult>(int skip, int take, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .AsNoTracking()
            .Where(predicate)
            .Skip(skip)
            .Take(take)
            .ProjectTo<TResult>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken: cancellationToken);
        return entities;
    }

    public async Task<PaginatedResult<TResult>> GetPaginatedAsync<TResult>(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbSet.CountAsync(cancellationToken);
        var skip = (pageNumber - 1) * pageSize;
        
        var items = await _dbSet
            .AsNoTracking()
            .Skip(skip)
            .Take(pageSize)
            .ProjectTo<TResult>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TResult>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PaginatedResult<TResult>> GetPaginatedAsync<TResult>(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbSet.CountAsync(predicate, cancellationToken);
        var skip = (pageNumber - 1) * pageSize;
        
        var items = await _dbSet
            .AsNoTracking()   
            .Where(predicate)
            .Skip(skip)
            .Take(pageSize)
            .ProjectTo<TResult>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TResult>(items, totalCount, pageNumber, pageSize);
    }
}