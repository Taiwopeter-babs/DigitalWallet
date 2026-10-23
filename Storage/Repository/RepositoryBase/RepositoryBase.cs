using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DigitalWallet.Data;

namespace DigitalWallet.Repository;

public abstract class RepositoryBase<T>(DatabaseContext context) : IRepositoryBase<T> where T : class
{
    protected DatabaseContext _context = context;


    /// <summary>
    /// Add an entity to the database
    /// </summary>
    /// <param name="entity"></param>
    public void Create(T entity)
    {
        _context.Set<T>().Add(entity);
    }

    /// <summary>
    /// Remove an entity from the database
    /// </summary>
    /// <param name="entity"></param>
    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public IQueryable<T> FindMany(bool trackChanges = false)
    {
        if (!trackChanges)
            return _context.Set<T>().AsNoTracking();

        return _context.Set<T>();
    }

    /// <summary>
    /// Finds an entity by a condition given by the expression. Also includes condition
    /// to load the entities related.
    /// </summary>
    /// <param name="expression">The extension expression</param>
    /// <param name="trackChanges">A boolean to check tracking entity in the storage</param>
    /// <returns></returns>
    public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression,
        bool trackChanges = false)
    {
        return trackChanges
            ? _context.Set<T>().Where(expression)
            : _context.Set<T>()
                .Where(expression)
                .AsNoTracking();
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);

        var entityEntry = _context.Set<T>().Entry(entity);

        entityEntry.State = EntityState.Modified;

        _context.Set<T>().Entry(entity)
            .Property<DateTime>("UpdatedAt").CurrentValue = DateTime.UtcNow;
    }
}