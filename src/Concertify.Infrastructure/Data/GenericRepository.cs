using System.Linq.Expressions;

using Concertify.Domain.Interfaces;
using Concertify.Domain.Models;

using Microsoft.EntityFrameworkCore;

namespace Concertify.Infrastructure.Data;

public class GenericRepository<T> : IGenericRepository<T> where T : EntityBase
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<List<T>> GetFilteredAsync(Expression<Func<T, bool>>[] filters, int? skip, int? take, params Expression<Func<T, bool>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsQueryable<T>();
        
        if (filters.Length != 0)
        {
            foreach (var filter in filters)
            {
                query = query.Where(filter);
            }
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync();

    }

    public async Task<List<T>> GetAsync(int? skip, int? take, params Expression<Func<T, bool>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsQueryable<T>();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id, params Expression<Func<T, bool>>[] includes)
    {
        IQueryable<T> query = _dbSet.AsQueryable<T>();

        query = query.Where(e => e.Id == id);

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.SingleAsync();
    }

    public async Task<int> InsertAsync(T entity)
    {
        await _dbSet.AddAsync(entity);

        return entity.Id; 
    }

    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (_context.Entry(entity).State == EntityState.Detached)
            _dbSet.Attach(entity);

        _context.Entry(entity).State = EntityState.Modified;
        
    }
    public void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (_context.Entry(entity).State == EntityState.Detached)
            _dbSet.Attach(entity);
        
        _dbSet.Remove(entity);
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

}

