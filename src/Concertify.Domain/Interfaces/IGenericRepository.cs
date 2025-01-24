using System.Linq.Expressions;

using Concertify.Domain.Models;

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Concertify.Domain.Interfaces;

public interface IGenericRepository<T> where T : EntityBase
{
    public Task<List<T>> GetFilteredAsync(Expression<Func<T, bool>>[] filtered,
        int? skip,
        int? take,
        params Expression<Func<T, object>>[] includes);

    public Task<List<T>> GetAsync(int? skip, int? take, params Expression<Func<T, object>>[] includes);
    public Task<T> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);
    public Task<int> InsertAsync (T entity);
    public void Update (T entity);
    public void Delete(T entity);
    public Task SaveChangesAsync();
}
