namespace PlatformEducationWorkers.Core.Interfaces.Repositories
{
    public interface ILogRepository
    {
        Task<T> AddLog<T>(T entity) where T : class;
    }
}