using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core;
using PlatformEducationWorkers.Core.Azure;

namespace PlatformEducationWorkers.Core.Services.Enterprises
{
    public class CreateEnterpriseService : ICreateEnterpriseService
    {
        private readonly IEnterpriseRepository _repository;
        private readonly IRepository _repositoryGeneral;
        private readonly EmailService _emailService;
        private readonly AzureBlobAvatarOperation _AzureAvatarService;

        public CreateEnterpriseService(IEnterpriseRepository repository, IRepository repositoryGeneral, EmailService emailService, AzureBlobAvatarOperation azureAvatarService)
        {
            _repository = repository;
            _repositoryGeneral = repositoryGeneral;
            _emailService = emailService;
            _AzureAvatarService = azureAvatarService;
        }

        public async Task AddEnterpriseWithOwnerAsync(Enterprise enterprise, string jobTitleName, User owner)
        {
            try
            {
                //подивитися у Enterpriceservice(AddingEnterprice)
                // Починаємо транзакцію
                await _repository.BeginTransactionAsync();


                await _repository.AddEnterpriseAsync(enterprise);
                await _repository.SaveChangesAsync();

                // Створюємо JobTitle
                var jobTitle = new JobTitle
                {
                    Name = jobTitleName,
                    Enterprise = enterprise
                };
                await _repository.AddJobTitleAsync(jobTitle);
                await _repository.SaveChangesAsync();

                // Створюємо користувачаtry
                try
                {
                    if (owner == null)
                        throw new Exception($"Error adding user, user is null");

                    if (_repositoryGeneral.GetQuery<User>(e => e.Name == owner.Name && e.Surname == owner.Surname && e.Birthday == owner.Birthday).Result.Any())
                        throw new Exception($"Error adding user, such user already exists");
                    
                    ////додати перевірку захешованого паролю
                    var salt = HashHelper.GenerateSalt();
                    owner.Salt = salt;
                    owner.Password = HashHelper.ComputeHash(owner.Password, salt);
                    owner.Login = HashHelper.ComputeHash(owner.Login, salt);

                    // Додавання аватарки у вигляді JSON-рядка
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
                owner.JobTitle = jobTitle;
                owner.Enterprise = enterprise;
                enterprise.Owner = owner;
                await _repository.AddUserAsync(owner);
                await _repository.SaveChangesAsync();
                var enterpriseEmail = owner.Enterprise.Email;
                var subject = "Вітаємо з реєстрацією!";
                var body = $"<p>Шановний {owner.Name} {owner.Surname},</p>" +
                           $"<p>Ви успішно зареєструвалися в системі.</p>" +
                           $"<p>З найкращими побажаннями,<br>Команда {owner.Enterprise.Title}</p>";

                await _emailService.SendEmailAsync(enterprise.Email, enterprise.PasswordEmail, owner.Email, subject, body);

                // Завершуємо транзакцію
                await _repository.CommitTransactionAsync();
            }
            catch
            {
                // У разі помилки скасовуємо всі зміни
                await _repository.RollbackTransactionAsync();
                throw;
            }
        }
    }


}
