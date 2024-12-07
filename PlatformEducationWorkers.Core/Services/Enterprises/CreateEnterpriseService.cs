﻿using PlatformEducationWorkers.Core.Interfaces.Repositories;
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
        private readonly IRepository _repositoryGeneral;

        public CreateEnterpriseService(IEnterpriseRepository repository, IRepository repositoryGeneral)
        {
            _repository = repository;
            _repositoryGeneral = repositoryGeneral;
        }

        public async Task AddEnterpriseWithOwnerAsync(Enterprise enterprise, string jobTitleName, User owner)
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

                // Створюємо користувачаtry
                try
                {
                    if (owner == null)
                        throw new Exception($"Error adding user, user is null");

                    if (_repositoryGeneral.GetQuery<User>(e => e.Name == owner.Name && e.Surname == owner.Surname && e.Birthday == owner.Birthday).Result.Any())
                        throw new Exception($"Error adding user, such user already exists");
                    // if (_repositoryGeneral.GetQuery<User>(e => HashHelper.ComputeHash(e.Login, e.Salt) == owner.Login && HashHelper.ComputeHash(e.Password, e.Salt) == owner.Password).Result.Count() > 0)
                    //    throw new Exception($"Error adding user,Please choose other password or login");
                    ////додати перевірку захешованого паролю
                    var salt = HashHelper.GenerateSalt();
                    owner.Salt = salt;
                    owner.Password = HashHelper.ComputeHash(owner.Password, salt);
                    owner.Login = HashHelper.ComputeHash(owner.Login, salt);

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
