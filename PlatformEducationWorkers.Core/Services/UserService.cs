using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Azure;

namespace PlatformEducationWorkers.Core.Services
{
    /// <summary>
    /// Service for managing user-related operations.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IRepository _repository;
        private readonly IUserResultService _userResultService;
        private readonly AzureBlobAvatarOperation AzureAvatarService;
        private readonly EmailService _emailService;

        /// <summary>
        /// Constructor for UserService.
        /// </summary>
        /// <param name="repository">Repository for data access.</param>
        /// <param name="emailService">Service for sending emails.</param>
        /// <param name="azureAvatarService">Service for Azure Blob avatar operations.</param>
        /// <param name="userResultService">Service for managing user results.</param>
        public UserService(IRepository repository, EmailService emailService, AzureBlobAvatarOperation azureAvatarService, IUserResultService userResultService)
        {
            _repository = repository;
            _emailService = emailService;
            AzureAvatarService = azureAvatarService;
            _userResultService = userResultService;
        }


        /// <summary>
        /// Adds a new user to the system.
        /// </summary>
        /// <param name="user">The user object to be added.</param>
        /// <returns>The added user object.</returns>
        public async Task<User> AddUser(User user)
        {
            try
            {
                if (user == null)
                    throw new Exception($"Error adding user, user is null");

                // Check for existing user with the same name, surname, and birthday
                if (_repository.GetQuery<User>(e => e.Name == user.Name && e.Surname == user.Surname && e.Birthday == user.Birthday).Result.Any())
                    throw new Exception($"Error adding user, such user already exists");


                // Check for duplicate login and password
                var allusers = _repository.GetAll<User>().ToList();
                bool isDuplicate = allusers.Any(e =>
                    HashHelper.ComputeHash(user.Login, e.Salt) == e.Login &&
                    HashHelper.ComputeHash(user.Password, e.Salt) == e.Password);

                if (isDuplicate)
                    throw new Exception("Error adding user, please choose another password or login.");

                // Hashing the password
                var salt = HashHelper.GenerateSalt();
                user.Salt = salt;
                user.Password = HashHelper.ComputeHash(user.Password, salt);
                user.Login = HashHelper.ComputeHash(user.Login, salt);

                // Upload avatar if provided
                if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                {

                    // Конвертуємо аватарку у Base64
                    using (var memoryStream = new MemoryStream())
                    {
                        byte[] fileBytes = Convert.FromBase64String(user.ProfileAvatar);
                        user.ProfileAvatar = await AzureAvatarService.UploadAvatarToBlobAsync(fileBytes);
                    }
                }

                // Send welcome email to the enterprise
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

        /// <summary>
        /// Deletes a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to be deleted.</param>
        public async Task DeleteUser(int userId)
        {
            try
            {
                User user = await _repository.GetById<User>(userId);

                // Delete avatar from Azure Blob if it exists
                if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                {
                    await AzureAvatarService.DeleteAvatarFromBlobAsync(user.ProfileAvatar);

                }

                // Delete all results associated with the user
                await _userResultService.DeleteAllResultsUser(userId);

                // Delete the user from the repository
                await _repository.Delete<User>(userId);

                // Send account deletion email
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

        /// <summary>
        /// Retrieves all users associated with a specific enterprise.
        /// </summary>
        /// <param name="enterpriseId">The ID of the enterprise.</param>
        /// <returns>A list of users belonging to the enterprise.</returns>
        public async Task<IEnumerable<User>> GetAllUsersEnterprise(int enterpriseId)
        {
            try
            {
                if (enterpriseId == null || enterpriseId < 0)
                {
                    throw new Exception("EnterpriseId is null or less than 0");
                }
                IEnumerable<User> usersEnterprise = await _repository.GetQuery<User>(u => u.Enterprise.Id == enterpriseId);

                // Load avatars for each user
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

        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user object.</returns>
        public async Task<User> GetUser(int userId)
        {
            try
            {
                if (userId == null || userId < 0)
                {
                    throw new Exception("userId is null or less than 0");
                }

                User user = await _repository.GetById<User>(userId);

                // Load avatar if it exists
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

        /// <summary>
        /// Retrieves users associated with a specific job title.
        /// </summary>
        /// <param name="jobTitleId">The ID of the job title.</param>
        /// <returns>A list of users with the specified job title.</returns>
        public async Task<IEnumerable<User>> GetUsersByJobTitle(int jobTitleId)
        {
            try
            {
                if (jobTitleId == null || jobTitleId < 0)
                {
                    throw new Exception("jobTitleId is null or less than 0");
                }
                IEnumerable<User> users = await _repository.GetQuery<User>(r => r.JobTitle.Id == jobTitleId);

                // Load avatars for each user
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

        /// <summary>
        /// Authenticates a user based on login credentials.
        /// </summary>
        /// <param name="login">The user's login.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>The authenticated user object.</returns>
        public async Task<User> Login(string login, string password)
        {
            try
            {
                if (login == null || password == null || login == "" || password == "")
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

                // Load avatar if it exists
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

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="user">The user object to be registered.</param>
        /// <returns>The registered user object.</returns>
        public async Task<User> Registration(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user), "User object is null");

                // Check for duplicate login and password
                var allusers = _repository.GetAll<User>().ToList();
                bool isDuplicate = allusers.Any(e =>
                    HashHelper.ComputeHash(user.Login, e.Salt) == e.Login &&
                    HashHelper.ComputeHash(user.Password, e.Salt) == e.Password);

                if (isDuplicate)
                    throw new Exception("Error adding user, please choose another password or login.");

                // Hashing the password
                var salt = HashHelper.GenerateSalt();
                user.Salt = salt;
                user.Password = HashHelper.ComputeHash(user.Password, salt);
                user.Login = HashHelper.ComputeHash(user.Login, salt);

                // Upload avatar if provided
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


        /// <summary>
        /// Searches for a user by name, surname, and birthday.
        /// </summary>
        /// <param name="name">The user's name.</param>
        /// <param name="surname">The user's surname.</param>
        /// <param name="birthday">The user's birthday.</param>
        /// <returns>The found user object.</returns>
        public async Task<User> SearchUser(string name, string surname, DateTime birthday)
        {
            try
            {
                User user = (await _repository.GetQuery<User>(u => u.Surname == surname && u.Name == name && u.Birthday == birthday)).FirstOrDefault();

                // Load avatar if it exists
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

        /// <summary>
        /// Updates an existing user's information.
        /// </summary>
        /// <param name="user">The user object with updated information.</param>
        /// <returns>The updated user object.</returns>
        public async Task<User> UpdateUser(User user)
        {
            try
            {
                if (user == null)
                {
                    throw new Exception("User is null");
                }
                User olduser = await _repository.GetById<User>(user.Id);
                if (olduser.Surname != user.Surname)
                {
                    olduser.Surname = user.Surname;
                }
                if (olduser.Name != user.Name)
                {
                    olduser.Name = user.Name;
                }
                if (olduser.Role != user.Role)
                {
                    olduser.Role = user.Role;
                }
                if (olduser.Email != user.Email)
                {
                    olduser.Email = user.Email;
                }
                if (olduser.Birthday != user.Birthday)
                {
                    olduser.Birthday = user.Birthday;
                }

                // Check for duplicate login and password
                bool isDuplicate = _repository.GetAll<User>().ToList().Any(e =>
                   HashHelper.ComputeHash(user.Login, e.Salt) == e.Login &&
                   HashHelper.ComputeHash(user.Password, e.Salt) == e.Password);

                if (isDuplicate)
                    throw new Exception("Error adding user, please choose another password or login.");
                
                if(HashHelper.ComputeHash(user.Login, olduser.Salt)== olduser.Login 
                    && HashHelper.ComputeHash(user.Password, olduser.Salt) == olduser.Password)
                {
                    // Hashing the new password
                    var salt = HashHelper.GenerateSalt();
                    olduser.Salt = salt;
                    olduser.Login = user.Login;
                    olduser.Password = user.Password;
                    olduser.Login = HashHelper.ComputeHash(user.Login, olduser.Salt);
                    olduser.Password = HashHelper.ComputeHash(user.Password, olduser.Salt);
                }
                // Remove old avatar if it exists
                if (olduser.ProfileAvatar != null && user.ProfileAvatar != null)
                {
                    await AzureAvatarService.DeleteAvatarFromBlobAsync(olduser.ProfileAvatar);
                }

                // Upload new avatar if provided
                if (user.ProfileAvatar != null)
                {
                    byte[] fileBytes = Convert.FromBase64String(user.ProfileAvatar);
                    user.ProfileAvatar = await AzureAvatarService.UploadAvatarToBlobAsync(fileBytes);
                }

                user = await _repository.Update(olduser);

                // Send email notification about account update
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


        /// <summary>
        /// Retrieves the most recent users for a specific enterprise.
        /// </summary>
        /// <param name="enterpriseId">The ID of the enterprise.</param>
        /// <returns>A list of the most recent users.</returns>
        public async Task<IEnumerable<User>> GetNewUsers(int enterpriseId, int numbersUser)
        {
            try
            {
                if (enterpriseId == null || enterpriseId < 0 || numbersUser==null || numbersUser<0)
                {
                    throw new Exception("enterpriseId or numbers user is null or less than 0 ");
                }
                var users = (await _repository.GetQueryAsync<User>(u => u.Enterprise.Id == enterpriseId)).Take(numbersUser);

                // Load avatars for each user
                foreach (var user in users)
                {
                    if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                    {
                        byte[] fileBytes = await AzureAvatarService.UnloadAvatarFromBlobAsync(user.ProfileAvatar);
                        user.ProfileAvatar = Convert.ToBase64String(fileBytes);

                    }
                }

                return users.OrderByDescending(user => user.DateCreate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving new users: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Retrieves available users for a given enterprise, excluding those with the "Owner" job title.
        /// Additionally, loads profile avatars from Azure Blob Storage.
        /// </summary>
        /// <param name="enterpriseId">The ID of the enterprise to filter users.</param>
        /// <returns>A list of users matching the criteria with loaded avatars.</returns>
        /// <exception cref="Exception">Thrown if enterpriseId is invalid or if an error occurs during retrieval.</exception>
        public async Task<IEnumerable<User>> GetAvaliableUsers(int enterpriseId)
        {
            try
            {
                // Validate enterpriseId: must not be null and must be non-negative
                if (enterpriseId == null || enterpriseId < 0)
                {
                    throw new Exception("enterpriseId is null or less than 0");
                }

                // Query users from the repository based on the enterprise ID
                // Exclude users with the "Owner" job title
                var users = await _repository.GetQueryAsync<User>(u => u.Enterprise.Id == enterpriseId && u.JobTitle.Name != "Owner");

                // Iterate through the retrieved users to process their avatars
                foreach (var user in users)
                {
                    // Check if the user has a profile avatar
                    if (user.ProfileAvatar != null && !string.IsNullOrEmpty(user.ProfileAvatar))
                    {
                        // Load avatar from Azure Blob Storage and convert it to a Base64 string
                        byte[] fileBytes = await AzureAvatarService.UnloadAvatarFromBlobAsync(user.ProfileAvatar);
                        user.ProfileAvatar = Convert.ToBase64String(fileBytes);

                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                // Handle and rethrow exception with additional context
                throw new Exception($"Error retrieving new users: {ex.Message}", ex);
            }
        }
    }
}


