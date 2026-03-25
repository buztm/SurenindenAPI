using System.Linq.Expressions;

namespace SurenindenAPI.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> SaveAsync();

        IQueryable<T> Where(Expression<Func<T, bool>> expression);
    }
}
