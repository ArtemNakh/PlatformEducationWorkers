using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Services
{
    public class UserService : IUserService
    {
        public Task<User> AddUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUser(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> GetAllUsersEnterprice(int enterpriceId)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUser(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> GetUsersByRole(int roleId)
        {
            throw new NotImplementedException();
        }

        public Task<User> Login(string login, string password)
        {
            throw new NotImplementedException();
        }

        public Task<User> Registration(User user)
        {
            throw new NotImplementedException();
        }

        public Task<User> SearchUser(string name, string surname, DateTime birthday)
        {
            throw new NotImplementedException();
        }

        public Task<User> UpdateUser(User user)
        {
            throw new NotImplementedException();
        }
    }
}
