using Newtonsoft.Json;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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



        public async Task<User> AddUser(User user)
        {
            try
            {
                if (user == null)
                    throw new Exception($"Error adding user, user is null");

                if (_repository.GetQuery<User>(e => e.Name == user.Name && e.Surname == user.Surname && e.Birthday == user.Birthday).Result.Any())
                    throw new Exception($"Error adding user, such user already exists");
                //додати перевірку захешованого паролю
                //Todo(hash only password)
                var allusers = _repository.GetAll<User>().ToList();
                bool isDuplicate = allusers.Any(e =>
                    HashHelper.ComputeHash(user.Login, e.Salt) == e.Login &&
                    HashHelper.ComputeHash(user.Password, e.Salt) == e.Password);

                if (isDuplicate)
                    throw new Exception("Error adding user, please choose another password or login.");

                //хешированія
                var salt = HashHelper.GenerateSalt();
                user.Salt = salt;
                user.Password = HashHelper.ComputeHash(user.Password, salt);
                user.Login = HashHelper.ComputeHash(user.Login, salt);

                // Додавання аватарки у вигляді JSON-рядка
                if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                {
                    user.ProfileAvatar = JsonConvert.SerializeObject(user.ProfileAvatar);
                }

                return await _repository.Add(user);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new user: ", ex);
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

        public Task<IEnumerable<User>> GetAllUsersEnterprise(int enterpriseId)
        {
            try
            {
                //додати перевірку на вірність полів
                return _repository.GetQuery<User>(u => u.Enterprise.Id == enterpriseId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get all users by enterpriceId : {enterpriseId} error: ", ex);
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
                return _repository.GetQuery<User>(r => r.JobTitle.Id == jobTitleId);
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
                var user = (await _repository.GetQuery<User>(u => true))
                           .FirstOrDefault(u => u.Login == HashHelper.ComputeHash(login, u.Salt));

                if (user == null)
                    throw new Exception("Invalid login or password");

                var hashedPassword = HashHelper.ComputeHash(password, user.Salt);
                if (user.Password != hashedPassword)
                    throw new Exception("Invalid login or password");

                return user;
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
                if (user == null)
                    throw new ArgumentNullException(nameof(user), "User object is null");


                //if (_repository.GetQuery<User>(u => HashHelper.ComputeHash(u.Login, u.Salt) == user.Login).Result.Any())
                //    throw new Exception("A user with this login already exists");
                //додати перевірку захешованого паролю
                //Todo(hash only password)
                var allusers = _repository.GetAll<User>().ToList();
                bool isDuplicate = allusers.Any(e =>
                    HashHelper.ComputeHash(user.Login, e.Salt) == e.Login &&
                    HashHelper.ComputeHash(user.Password, e.Salt) == e.Password);

                if (isDuplicate)
                    throw new Exception("Error adding user, please choose another password or login.");


                var salt = HashHelper.GenerateSalt();
                user.Salt = salt;
                user.Password = HashHelper.ComputeHash(user.Password, salt);
                user.Login = HashHelper.ComputeHash(user.Login, salt);

                // Додавання аватарки у вигляді JSON-рядка
                if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                {
                    user.ProfileAvatar = JsonConvert.SerializeObject(user.ProfileAvatar);
                }
                return await _repository.Add(user);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error registering user: {ex.Message}", ex);
            }
        }



        public async Task<User> SearchUser(string name, string surname, DateTime birthday)
        {
            try
            {
                return _repository.GetQuery<User>(u => u.Surname == surname && u.Name == name && u.Birthday == birthday).Result.FirstOrDefault();
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

                User olduser =await  _repository.GetById<User>(user.Id);
                if (user.Surname != user.Surname)
                {
                    olduser.Surname = user.Surname;
                }
                if (user.Name != user.Name)
                {
                    olduser.Name = user.Name;
                }
                if (user.Role != user.Role)
                {
                    olduser.Role = user.Role;
                }
                if (user.Email != user.Email)
                {
                    olduser.Email = user.Email;
                }
                if (user.Birthday != user.Birthday)
                {
                    olduser.Birthday = user.Birthday;
                }


                    var salt = HashHelper.GenerateSalt();
                olduser.Salt = salt;
                olduser.Login = user.Login;
                olduser.Password = user.Password;
                     olduser.Login = HashHelper.ComputeHash(user.Login, olduser.Salt);
               
                    olduser.Password = HashHelper.ComputeHash(user.Password, olduser.Salt);
                


                // Додавання аватарки у вигляді JSON-рядка
                if (user.ProfileAvatar != null)
                {
                    olduser.ProfileAvatar = JsonConvert.SerializeObject(user.ProfileAvatar);

                }

                return await _repository.Update(olduser);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error update user , error:{ex}");
                throw;
            }
        }



        public async Task<IEnumerable<User>> GetNewUsers(int enterpriseId)
        {
            try
            {
                var users = await _repository.GetQueryAsync<User>(u => u.Enterprise.Id == enterpriseId);
                return users.OrderByDescending(user => user.DateCreate).Take(5);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving new users: {ex.Message}", ex);
            }
        }

    }
}


