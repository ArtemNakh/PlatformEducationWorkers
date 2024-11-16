using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
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
                if (user == null)
                    throw new Exception($"Error adding user,user is null");

                if (_repository.GetQuery<User>(e => e.Name == user.Name && e.Surname==user.Surname && e.Birthday==user.Birthday).Result.Count() > 0)
                    throw new Exception($"Error adding user, those man already exist");
              
                if(_repository.GetQuery<User>(e=>e.Login == user.Login && e.Password==user.Password).Result.Count() > 0)
                    throw new Exception($"Error adding user,Please choose correct password or login");

                return _repository.Add(user);
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

    
        public async Task<User> Login(string login, string password)
        {
            try
            {
                //додати валідацію
                var user = await _repository.GetQuery<User>(u => u.Login == login && u.Password == password);
                return user.FirstOrDefault();
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


