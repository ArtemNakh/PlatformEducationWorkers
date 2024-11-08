using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Interfaces
{
    public interface IRepository
    {
        Task<T> Add<T>(T entity) where T : class;
        Task<T> Update<T>(T entity) where T : class;
        Task Delete<T>(int id) where T : class;
        Task<T> GetById<T>(int id) where T : class;
        IQueryable<T> GetAll<T>() where T : class;
        Task<IEnumerable<T>> GetQuery<T>(Expression<Func<T, bool>> func) where T : class;
        Task<T> GetByIdAsync<T>(int id) where T : class;
        Task<IEnumerable<T>> GetQueryAsync<T>(Expression<Func<T, bool>> func) where T : class;
    }
}
