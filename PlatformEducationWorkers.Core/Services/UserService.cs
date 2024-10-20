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
        private readonly IRepository _repository;

        public UserService(IRepository repository)
        {
            _repository = repository;
        }

        public Task<User> AddUser(User user)
        {
            try
            {
                //додати перевірку на існування усіх полів
                _repository.Add(user);
                return Task.FromResult(user);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new User: ", ex);
            }
        }

        public Task DeleteUser(int userId)
        {
            try
            {
                //додати перевірку на існування Id
                return _repository.Delete<User>(userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleted User by id: {userId} error: ", ex);
            }
        }

        public Task<IEnumerable<User>> GetAllUsersEnterprice(int enterpriceId)
        {
            try
            {
                //додати перевірку на вірність полів
                return _repository.GetQuery<User>(u => u.Enterprise.Id == enterpriceId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get all users by enterpriceId : {enterpriceId} error: ", ex);
            }
        }

        public Task<User> GetUser(int userId)
        {
            try
            {
                //додати валіадцію
                return _repository.GetById<User>(userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get user by id , error:{ex}");
            }
        }

        public Task<IEnumerable<User>> GetUsersByJobTitle(int jobTitleId)
        {
            try
            {
                //додати валідацію
                return _repository.GetQuery<User>(r=>r.JobTitle.Id== jobTitleId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get user by role , error:{ex}");
            }
        }

        public Task<IEnumerable<User>> GetUsersByRole(int roleId)
        {
            throw new NotImplementedException();
        }

        public Task<User> Login(string login, string password)
        {
            try
            {
                //додати валідацію
               User user=  _repository.GetQuery<User>(r => r.Login == login && r.Password == password).Result.First();
                return  Task.FromResult(user);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error login user , error:{ex}");
            }
        }

        public async Task<User> Registration(User user)
        {
            try
            {
                //додати валідацію
                if (_repository.GetQuery<User>(r => r.Password == user.Password).Result.Count() > 0)
                {
                    throw new Exception($"Error Choose other password");
                }

                return await _repository.Add(user);
              
            }
            catch (Exception ex)
            {
                throw new Exception($"Error registration user , error:{ex}");
            }
        }

        public async Task<User> SearchUser(string name, string surname, DateTime birthday)
        {
            try
            {
                return  _repository.GetQuery<User>(u => u.Surname == surname && u.Name == name && u.Birthday == birthday).Result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error search user , error:{ex}");
            }
        }

        public async Task<User> UpdateUser(User user)
        {
            try
            {

                User olduser =  _repository.GetById<User>(user.Id).Result;

                olduser.Surname = user.Surname;
                olduser.Name = user.Name;   
                olduser.Role = user.Role;
                olduser.Email = user.Email;
                olduser.Birthday=user.Birthday;
               return  _repository.Update(olduser).Result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error update user , error:{ex}");
                throw;
            }
        }
    }
}


/*
 * using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository _repository;

        public UserService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<User> AddUser(User user)
        {
            // Валідація наявності підприємства
            if (user.Enterprise == null)
            {
                throw new ArgumentException("Enterprise is required.");
            }

            // Валідація ролі користувача
            if (user.Role == null)
            {
                throw new ArgumentException("Role is required.");
            }

            return await _repository.Add(user);
        }

        public async Task DeleteUser(int userId)
        {
            var user = await _repository.GetById<User>(userId);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            await _repository.Delete<User>(userId);
        }

        public async Task<IEnumerable<User>> GetAllUsersEnterprice(int enterpriceId)
        {
            return await _repository.GetQuery<User>(u => u.Enterprise.Id == enterpriceId);
        }

        public async Task<User> GetUser(int userId)
        {
            var user = await _repository.GetById<User>(userId);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            return user;
        }

        public async Task<IEnumerable<User>> GetUsersByRole(int roleId)
        {
            return await _repository.GetQuery<User>(u => u.Role.Id == roleId);
        }

        public async Task<User> Login(string login, string password)
        {
            var user = await _repository.GetQuery<User>(u => u.Login == login && u.Password == password).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new ArgumentException("Invalid login or password.");
            }

            return user;
        }

        public async Task<User> Registration(User user)
        {
            // Перевірка, чи користувач з таким логіном вже існує
            var existingUser = await _repository.GetQuery<User>(u => u.Login == user.Login).FirstOrDefaultAsync();

            if (existingUser != null)
            {
                throw new ArgumentException("User with this login already exists.");
            }

            return await _repository.Add(user);
        }

        public async Task<User> SearchUser(string name, string surname, DateTime birthday)
        {
            var user = await _repository.GetQuery<User>(u => u.Name == name && u.Surname == surname && u.Birthday == birthday).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            return user;
        }

        public async Task<User> UpdateUser(User user)
        {
            var existingUser = await _repository.GetById<User>(user.Id);

            if (existingUser == null)
            {
                throw new ArgumentException("User not found.");
            }

            // Оновлення інформації
            existingUser.Name = user.Name;
            existingUser.Surname = user.Surname;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;
            existingUser.Login = user.Login;
            existingUser.Enterprise = user.Enterprise;
            existingUser.Role = user.Role;

            return await _repository.Update(existingUser);
        }
    }
}

 * */
