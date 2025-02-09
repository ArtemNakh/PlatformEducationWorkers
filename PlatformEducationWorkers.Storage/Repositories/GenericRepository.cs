using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using System.Linq.Expressions;

namespace PlatformEducationWorkers.Storage.Repositories
{
    public class GenericRepository : IRepository
    {
        private readonly PlatformEducationContex _context;
        public GenericRepository(PlatformEducationContex context)
        {
            _context = context;
        }

        public async Task<T> Add<T>(T entity) where T : class
        {

            // Від'єднати всі відстежувані об'єкти
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Detached;
                }
            }

            // Додати новий об'єкт
            var obj = _context.Add(entity);
            await _context.SaveChangesAsync();
            return obj.Entity;
        }
        public async Task<T> AddAll<T>(T entity) where T : class
        {
            // Додати новий об'єкт
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

        public async Task<T> GetById<T>(int id, bool trackChanges = true) where T : class
        {
            if (trackChanges)
            {
                return await _context.Set<T>().FindAsync(id);
            }
            else
            {
                // Завантаження без відстеження
                return await _context.Set<T>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
            }
        }

        public async Task<IEnumerable<T>> GetQuery<T>(Expression<Func<T, bool>> func) where T : class
        {
            return _context.Set<T>().Where(func);
        }

        public async Task<T> Update<T>(T entity) where T : class
        {
            if (IsTracked(entity))
            {
                // Якщо сутність вже відстежується, просто зберігаємо зміни
                await _context.SaveChangesAsync();
                return entity;
            }
            else
            {
                var newEntity = _context.Update(entity);
                await _context.SaveChangesAsync();
                return newEntity.Entity;
            }

        }

        public async Task<T> UpdateOnlySelected<T>(T entity, params Expression<Func<T, object>>[] updatedProperties) where T : class
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlatformEducationContex>();
            optionsBuilder.UseSqlServer(_context.Database.GetConnectionString()); 

            using (var newContext = new PlatformEducationContex(optionsBuilder.Options))
           {
                var entry = newContext.Entry(entity);

                // Додаємо об'єкт, але робимо його незмінним
                newContext.Set<T>().Attach(entity);
                entry.State = EntityState.Unchanged;

                // Позначаємо лише вибрані поля як змінені
                foreach (var property in updatedProperties)
                {
                    entry.Property(property).IsModified = true;
                }

                await newContext.SaveChangesAsync();
            }
            return entity;
        }

        public bool IsTracked<T>(T entity) where T : class
        {
            var entry = _context.Entry(entity);
            return entry.State != EntityState.Detached;
        }

        // Асинхронний метод для отримання сутності за id
        public async Task<T> GetByIdAsync<T>(int id) where T : class
        {
            return await _context.Set<T>().FindAsync(id);
        }

        // Асинхронний метод для виконання запиту з фільтром
        public async Task<IEnumerable<T>> GetQueryAsync<T>(Expression<Func<T, bool>> func) where T : class
        {
            var query = _context.Set<T>().Where(func); 
            return await query.ToListAsync(); 
        }


    }
}
