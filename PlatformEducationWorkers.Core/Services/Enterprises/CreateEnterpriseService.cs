using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;

namespace PlatformEducationWorkers.Core.Services.Enterprises
{
    public class CreateEnterpriseService : ICreateEnterpriseService
    {
        private readonly IEnterpriseRepository _repository;

        public CreateEnterpriseService(IEnterpriseRepository repository)
        {
            _repository = repository;
        }

        public async Task AddEnterpriseWithOwnerAsync(Enterprice enterprise, string jobTitleName, User owner)
        {
            try
            {
                //подивитися у Enterpriceservice(AddingEnterprice)
                // Починаємо транзакцію
                await _repository.BeginTransactionAsync();

                // Додаємо фірму
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

                // Створюємо користувача
                owner.JobTitle = jobTitle;
                owner.Enterprise = enterprise;
                enterprise.Owner = owner;
                await _repository.AddUserAsync(owner);
                await _repository.SaveChangesAsync();

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
