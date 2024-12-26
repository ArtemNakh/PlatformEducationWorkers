using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Azure;

namespace PlatformEducationWorkers.Core.Services.Enterprises
{
    /// <summary>
    /// Service class responsible for creating new enterprises along with their owners.
    /// Handles operations such as adding enterprises, creating job titles, 
    /// setting up user accounts, and sending confirmation emails.
    /// </summary>
    public class CreateEnterpriseService : ICreateEnterpriseService
    {
        private readonly IEnterpriseRepository _repository;
        private readonly IRepository _repositoryGeneral;
        private readonly EmailService _emailService;
        private readonly AzureBlobAvatarOperation _AzureAvatarService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateEnterpriseService"/> class.
        /// </summary>
        /// <param name="repository">Repository for enterprise-specific operations.</param>
        /// <param name="repositoryGeneral">General repository for non-enterprise-specific entities.</param>
        /// <param name="emailService">Service for sending emails.</param>
        /// <param name="azureAvatarService">Service for managing Azure Blob avatar operations.</param>
        public CreateEnterpriseService(IEnterpriseRepository repository, IRepository repositoryGeneral, EmailService emailService, AzureBlobAvatarOperation azureAvatarService)
        {
            _repository = repository;
            _repositoryGeneral = repositoryGeneral;
            _emailService = emailService;
            _AzureAvatarService = azureAvatarService;
        }

        /// <summary>
        /// Adds a new enterprise along with its owner and associated job title.
        /// Includes operations for adding enterprise, setting user details,
        /// uploading profile avatars, and sending confirmation emails.
        /// </summary>
        /// <param name="enterprise">The enterprise entity to be added.</param>
        /// <param name="jobTitleName">The name of the job title for the owner.</param>
        /// <param name="owner">The user entity representing the owner of the enterprise.</param>
        /// <exception cref="Exception">Throws if any operation fails during the transaction.</exception>
        public async Task AddEnterpriseWithOwnerAsync(Enterprise enterprise, string jobTitleName, User owner)
        {
            try
            {

                // Begin a transaction for atomicity
                await _repository.BeginTransactionAsync();

                // Add the enterprise entity to the database
                await _repository.AddEnterpriseAsync(enterprise);
                await _repository.SaveChangesAsync();

                // Create and add a new JobTitle associated with the enterprise
                var jobTitle = new JobTitle
                {
                    Name = jobTitleName,
                    Enterprise = enterprise
                };
                await _repository.AddJobTitleAsync(jobTitle);
                await _repository.SaveChangesAsync();

                // Validate and prepare the user entity
                try
                {
                    if (owner == null)
                        throw new Exception($"Error adding user, user is null");

                    if (_repositoryGeneral.GetQuery<User>(e => e.Name == owner.Name && e.Surname == owner.Surname && e.Birthday == owner.Birthday).Result.Any())
                        throw new Exception($"Error adding user, such user already exists");

                    // Generate and set password hash and salt
                    var salt = HashHelper.GenerateSalt();
                    owner.Salt = salt;
                    owner.Password = HashHelper.ComputeHash(owner.Password, salt);
                    owner.Login = HashHelper.ComputeHash(owner.Login, salt);

                    // Process and upload avatar if provided
                    if (owner.ProfileAvatar != null && !string.IsNullOrEmpty(owner.ProfileAvatar))
                    {
                        byte[] imageBytes = Convert.FromBase64String(owner.ProfileAvatar);
                        owner.ProfileAvatar = await _AzureAvatarService.UploadAvatarToBlobAsync(imageBytes);
                    }


                }
                catch (Exception ex)
                {
                    throw new Exception("Error adding new user: ", ex);
                }

                // Assign job title and enterprise to the user, then link the user as the enterprise owner
                owner.JobTitle = jobTitle;
                owner.Enterprise = enterprise;
                enterprise.Owner = owner;

                await _repository.AddUserAsync(owner);
                await _repository.SaveChangesAsync();

                // Prepare and send a confirmation email to the owner
                var enterpriseEmail = owner.Enterprise.Email;
                var subject = "Вітаємо з реєстрацією!";
                var body = $"<p>Шановний {owner.Name} {owner.Surname},</p>" +
                           $"<p>Ви успішно зареєструвалися в системі.</p>" +
                           $"<p>З найкращими побажаннями,<br>Команда {owner.Enterprise.Title}</p>";

                await _emailService.SendEmailAsync(enterprise.Email, enterprise.PasswordEmail, owner.Email, subject, body);

                // Commit the transaction after successful operations
                await _repository.CommitTransactionAsync();
            }
            catch
            {
                // Roll back changes and delete uploaded avatar if an error occurs
                if (owner.ProfileAvatar != null && !string.IsNullOrEmpty(owner.ProfileAvatar))
                {
                     await _AzureAvatarService.DeleteAvatarFromBlobAsync(owner.ProfileAvatar);
                }
                await _repository.RollbackTransactionAsync();
                throw;
            }
        }
    }


}
