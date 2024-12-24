using Amazon.S3.Model;
using Newtonsoft.Json;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;

using PlatformEducationWorkers.Core.Azure;
using Azure.Core;


namespace PlatformEducationWorkers.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository _repository;
        private readonly IUserResultService _userResultService;
        private readonly AzureBlobAvatarOperation AzureAvatarService;
        private readonly EmailService _emailService;
        public UserService(IRepository repository, EmailService emailService, AzureBlobAvatarOperation azureAvatarService, IUserResultService userResultServuce)
        {
            _repository = repository;
            _emailService = emailService;
            AzureAvatarService = azureAvatarService;
            _userResultService = userResultServuce;
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

                    // Конвертуємо аватарку у Base64
                    using (var memoryStream = new MemoryStream())
                    {
                        byte[] fileBytes = Convert.FromBase64String(user.ProfileAvatar);
                        user.ProfileAvatar = await AzureAvatarService.UploadAvatarToBlobAsync(fileBytes);
                    }
                }

                var enterpriseEmail = user.Enterprise.Email;
                var subject = "Вітаємо вас було додано до на платформу навчання працівників";
                var body = $"<p>Шановний {user.Name} {user.Surname},</p>" +
                     $"<p>Вітаємо вас було додано до на платформу навчання працівників від вашої фірми</p>" +
                           $"<p>З найкращими побажаннями,<br>Команда {user.Enterprise.Title}</p>";

                await _emailService.SendEmailAsync(user.Enterprise.Email, user.Enterprise.PasswordEmail, user.Email, subject, body);

                return await _repository.Add(user);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new user: ", ex);
            }
        }


        public async Task DeleteUser(int userId)
        {
            try
            {
                User user = await _repository.GetById<User>(userId);
                if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                {
                    await AzureAvatarService.DeleteAvatarFromBlobAsync(user.ProfileAvatar);

                }

                await _userResultService.DeleteAllResultsUser(userId);

                await _repository.Delete<User>(userId);


                var subject = "Видалення облікового запису";
                var body = $"<p>Шановний {user.Name} {user.Surname},</p>" +
                     $"<p>Ваш обліковий запис був видалений зі пратформи для навчання співробітників</p>" +
                           $"<p>З найкращими побажаннями,<br>Команда {user.Enterprise.Title}</p>";
                string EnterpriseEmail = user.Enterprise.Email;
                string UserEmail = user.Email;
                string EnterprisePassword = user.Enterprise.PasswordEmail;

                await _emailService.SendEmailAsync(EnterpriseEmail, EnterprisePassword, UserEmail, subject, body);


            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleted User by id: {userId} error: ", ex);
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersEnterprise(int enterpriseId)
        {
            try
            {
                if(enterpriseId==null||enterpriseId < 0)
                {
                    throw new Exception("EnterpriseId is null or less than 0");
                }
                IEnumerable<User> usersEnterprise = await _repository.GetQuery<User>(u => u.Enterprise.Id == enterpriseId);

                foreach (var user in usersEnterprise)
                {
                    if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                    {
                        byte[] fileBytes = await AzureAvatarService.UnloadAvatarFromBlobAsync(user.ProfileAvatar);
                        user.ProfileAvatar = Convert.ToBase64String(fileBytes);

                    }
                }


                return usersEnterprise;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get all users by enterpriceId : {enterpriseId} error: ", ex);
            }
        }

        public async Task<User> GetUser(int userId)
        {
            try
            {
                if (userId == null || userId < 0)
                {
                    throw new Exception("userId is null or less than 0");
                }

                User user = await _repository.GetById<User>(userId);
                if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                {
                    byte[] fileBytes = await AzureAvatarService.UnloadAvatarFromBlobAsync(user.ProfileAvatar);
                    user.ProfileAvatar = Convert.ToBase64String(fileBytes);

                }
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get user by id , error:{ex}");
            }
        }

        public async Task<IEnumerable<User>> GetUsersByJobTitle(int jobTitleId)
        {
            try
            {
                if (jobTitleId == null || jobTitleId < 0)
                {
                    throw new Exception("jobTitleId is null or less than 0");
                }
                IEnumerable<User> users = await _repository.GetQuery<User>(r => r.JobTitle.Id == jobTitleId);
                foreach (var user in users)
                {
                    if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                    {
                        byte[] fileBytes = await AzureAvatarService.UnloadAvatarFromBlobAsync(user.ProfileAvatar);
                        user.ProfileAvatar = Convert.ToBase64String(fileBytes);

                    }
                }

                return users;
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
                if (login == null || password ==null|| login==""||password=="")
                {
                    throw new Exception("loginor password is null or empty");
                }

                var user = (await _repository.GetQuery<User>(u => true))
                           .FirstOrDefault(u => u.Login == HashHelper.ComputeHash(login, u.Salt));

                if (user == null)
                    throw new Exception("Invalid login or password");

                var hashedPassword = HashHelper.ComputeHash(password, user.Salt);
                if (user.Password != hashedPassword)
                    throw new Exception("Invalid login or password");

                if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                {
                    byte[] fileBytes = await AzureAvatarService.UnloadAvatarFromBlobAsync(user.ProfileAvatar);
                    user.ProfileAvatar = Convert.ToBase64String(fileBytes);

                }
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
                    byte[] fileBytes = Convert.FromBase64String(user.ProfileAvatar);
                    user.ProfileAvatar = await AzureAvatarService.UploadAvatarToBlobAsync(fileBytes);

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
               User user=(await _repository.GetQuery<User>(u => u.Surname == surname && u.Name == name && u.Birthday == birthday)).FirstOrDefault();
                if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                {
                    byte[] fileBytes = await AzureAvatarService.UnloadAvatarFromBlobAsync(user.ProfileAvatar);
                    user.ProfileAvatar = Convert.ToBase64String(fileBytes);

                }

                return user;
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
                if(user==null)
                {
                    throw new Exception("User is null");
                }
                User olduser = await _repository.GetById<User>(user.Id);
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

                bool isDuplicate = _repository.GetAll<User>().ToList().Any(e =>
                   HashHelper.ComputeHash(user.Login, e.Salt) == e.Login &&
                   HashHelper.ComputeHash(user.Password, e.Salt) == e.Password);

                if (isDuplicate)
                    throw new Exception("Error adding user, please choose another password or login.");


                var salt = HashHelper.GenerateSalt();
                olduser.Salt = salt;
                olduser.Login = user.Login;
                olduser.Password = user.Password;
                olduser.Login = HashHelper.ComputeHash(user.Login, olduser.Salt);
                olduser.Password = HashHelper.ComputeHash(user.Password, olduser.Salt);

                //видалення старої фотографії
                if (olduser.ProfileAvatar != null)
                {
                    await AzureAvatarService.DeleteAvatarFromBlobAsync(olduser.ProfileAvatar);
                }

                //додавання нової фотографії
                if (user.ProfileAvatar != null)
                {
                    byte[] fileBytes = Convert.FromBase64String(user.ProfileAvatar);
                    user.ProfileAvatar = await AzureAvatarService.UploadAvatarToBlobAsync(fileBytes);
                }

                user = await _repository.Update(olduser);

                var enterpriseEmail = user.Enterprise.Email;
                var subject = "Оновлення облікового запису";
                var body = $"<p>Шановний {user.Name} {user.Surname},</p>" +
                     $"<p>Ваш обліковий запис був успішно оновлений</p>" +
                           $"<p>З найкращими побажаннями,<br>Команда {user.Enterprise.Title}</p>";

                await _emailService.SendEmailAsync(user.Enterprise.Email, user.Enterprise.PasswordEmail, user.Email, subject, body);

                return user;
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
                if (enterpriseId == null|| enterpriseId<0)
                {
                    throw new Exception("enterpriseId is null or less than 0");
                }
                var users = await _repository.GetQueryAsync<User>(u => u.Enterprise.Id == enterpriseId);
                foreach (var user in users)
                {
                    if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                    {
                        byte[] fileBytes = await AzureAvatarService.UnloadAvatarFromBlobAsync(user.ProfileAvatar);
                        user.ProfileAvatar = Convert.ToBase64String(fileBytes);

                    }
                }
               
                return users.OrderByDescending(user => user.DateCreate).Take(5);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving new users: {ex.Message}", ex);
            }
        }

    }
}


