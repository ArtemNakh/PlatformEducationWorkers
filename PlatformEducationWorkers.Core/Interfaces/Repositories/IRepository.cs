
using System.Linq.Expressions;

namespace PlatformEducationWorkers.Core.Interfaces.Repositories
{
    public interface IRepository
    {
        Task<T> Add<T>(T entity) where T : class;
        Task<T> AddAll<T>(T entity) where T : class;
        Task<T> UpdateOnlySelected<T>(T entity, params Expression<Func<T, object>>[] updatedProperties) where T : class;
        Task<T> Update<T>(T entity) where T : class;
        Task Delete<T>(int id) where T : class;
        Task<T> GetById<T>(int id, bool trackChanges = true) where T : class;
        IQueryable<T> GetAll<T>() where T : class;
        Task<IEnumerable<T>> GetQuery<T>(Expression<Func<T, bool>> func) where T : class;
        Task<T> GetByIdAsync<T>(int id) where T : class;
        Task<IEnumerable<T>> GetQueryAsync<T>(Expression<Func<T, bool>> func) where T : class;
     
    }
}
