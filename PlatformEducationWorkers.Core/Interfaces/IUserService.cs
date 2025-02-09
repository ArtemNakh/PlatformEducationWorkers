using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> Login(string login, string password);
        Task<User> Registration(User user);
        Task<IEnumerable<User>> GetAllUsersEnterprise(int enterpriseId);
        Task<User> GetUser(int userId);
        Task DeleteUser(int userId);
        Task<User> UpdateUser(User user);
        Task<User> AddUser(User user);
        Task<User> SearchUser(string name, string surname,DateTime birthday);
        Task<IEnumerable<User>> GetUsersByJobTitle(int jobTitleId);
        Task<IEnumerable<User>> GetNewUsers(int enterpriseId,int numberUsers);
        Task<IEnumerable<User>> GetAvaliableUsers(int enterpriseId);
    }
}
