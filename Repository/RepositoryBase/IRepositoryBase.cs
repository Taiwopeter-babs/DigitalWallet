using System.Linq.Expressions;

namespace DigitalWallet.Repository;

public interface IRepositoryBase<T>
{
    IQueryable<T> FindMany(bool trackChanges = false);
    IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false);
    void Create(T entity);
    void Delete(T entity);
    void Update(T entity);
}