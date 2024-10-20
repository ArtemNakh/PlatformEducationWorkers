using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> Login(string login, string password);
        Task<User> Registration(User user);

        Task<IEnumerable<User>> GetAllUsersEnterprice(int enterpriceId);
        Task<User> GetUser(int userId);
        Task DeleteUser(int userId);
        Task<User> UpdateUser(User user);
        Task<User> AddUser(User user);

        Task<User> SearchUser(string name, string surname,DateTime birthday);
        Task<IEnumerable<User>> GetUsersByJobTitle(int jobTitleId);

    }
}
