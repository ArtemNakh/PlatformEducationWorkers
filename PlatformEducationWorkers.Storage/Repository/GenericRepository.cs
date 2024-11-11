using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Core.Interfaces;
using System.Linq.Expressions;

namespace PlatformEducationWorkers.Storage.Repository
{
    public class GenericRepository:IRepository
    {
        private readonly PlatformEducationContex _context;

        public GenericRepository(PlatformEducationContex context)
        {
            _context = context;
        }

        public async Task<T> Add<T>(T entity) where T : class
        {
            var obj = _context.Add(entity);
            await _context.SaveChangesAsync();
            return obj.Entity;
        }

        public async Task Delete<T>(int id) where T : class
        {
            var entity = await GetById<T>(id);
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<T> GetAll<T>() where T : class
        {
            return _context.Set<T>();
        }

        public async Task<T> GetById<T>(int id) where T : class
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetQuery<T>(Expression<Func<T, bool>> func) where T : class
        {
            return _context.Set<T>().Where(func);
        }

        public async Task<T> Update<T>(T entity) where T : class
        {
            var newEntity = _context.Update(entity);
            await _context.SaveChangesAsync();
            return newEntity.Entity;
        }

        // Асинхронний метод для отримання сутності за id
        public async Task<T> GetByIdAsync<T>(int id) where T : class
        {
            return await _context.Set<T>().FindAsync(id);
        }

        // Асинхронний метод для виконання запиту з фільтром
        public async Task<IEnumerable<T>> GetQueryAsync<T>(Expression<Func<T, bool>> func) where T : class
        {
            var query = _context.Set<T>().Where(func); // Створюємо запит
            return await query.ToListAsync(); // Виконуємо асинхронний запит
        }
    }
}
