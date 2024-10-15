using PlatformEducationWorkers.Core.Interfaces;

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

        public Task<T> GetById<T>(int id) where T : class
        {
            return _context.Set<T>().FindAsync(id).AsTask();
        }

        public async Task<T> Update<T>(T entity) where T : class
        {
            var newEntity = _context.Update(entity);
            await _context.SaveChangesAsync();
            return newEntity.Entity;
        }
    }
}
