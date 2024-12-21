﻿using Amazon.S3.Model;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Core.Services.Enterprises
{
    public class EnterpriceService : IEnterpriseService
    {
        private readonly IRepository _repository;
        public EnterpriceService(IRepository repository)
        {
            _repository = repository;
        }

        public Task<Enterprise> AddingEnterprise(Enterprise enterprice)
        {
            try
            {

                if (enterprice == null)
                    throw new Exception($"Error adding Enterprice,enterprice is null");

                if (_repository.GetQuery<Enterprise>(e => e.Title == enterprice.Title).Result.Count() > 0)
                    throw new Exception($"Error adding Enterprice, Choose other name");
                if (_repository.GetQuery<Enterprise>(e => e.Email == enterprice.Email).Result.Count() > 0)
                    throw new Exception($"Error adding Enterprice, Choose other Email");

                return _repository.Add(enterprice);

            }
            catch (Exception ex)
            {

                throw new Exception($"Error adding Enterprice, error:{ex}");
            }
        }

        public async Task DeleteingEnterprise(int enterpriceId)
        {
            try
            {
                // Отримуємо фірму, яку потрібно видалити
                var enterprice = await _repository.GetById<Enterprise>(enterpriceId);
                if (enterprice == null)
                    throw new Exception($"Enterprice with id {enterpriceId} not found");


                var courses = (await _repository.GetQuery<Courses>(c => c.Enterprise.Id == enterpriceId)).ToList();
                foreach (var course in courses)
                {
                    await _repository.Delete<Courses>(course.Id);
                }

                var users = (await _repository.GetQuery<User>(u => u.Enterprise.Id == enterpriceId)).ToList();
                foreach (var user in users)
                {
                    if (enterprice.Owner != null && user.Id == enterprice.Owner.Id)
                    {
                        enterprice.Owner = null;
                        await _repository.Update(enterprice);
                    }
                    await _repository.Delete<User>(user.Id);
                }


                var jobTitles = (await _repository.GetQuery<JobTitle>(j => j.Enterprise.Id == enterpriceId)).ToList();
                foreach (var job in jobTitles)
                {
                    await _repository.Delete<JobTitle>(job.Id);
                }


                // Видалення фірми
                await _repository.Delete<Enterprise>(enterpriceId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting Enterprice and its related entities: {ex.Message}");
            }
        }

        public Task<Enterprise> GetEnterprise(int enterpriceId)
        {
            try
            {
                //додати валідацію
                return _repository.GetById<Enterprise>(enterpriceId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error getting Enterprice by id, error:{ex}");
            }
        }

        public Task<Enterprise> GetEnterprise(string title)
        {
            try
            {
                //додати валідацію
                return Task.FromResult(_repository.GetQuery<Enterprise>(e => e.Title == title).Result.FirstOrDefault());

            }
            catch (Exception ex)
            {

                throw new Exception($"Error get Enterprice by title, error:{ex}");
            }
        }

        public Task<Enterprise> GetEnterpriseByUser(int userId)
        {
            try
            {
                User user = _repository.GetById<User>(userId).Result;
                //додати валідацію
                return Task.FromResult(_repository.GetQuery<Enterprise>(u => u.Id == user.Enterprise.Id).Result.FirstOrDefault());
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get Enterprice by user, error:{ex}");
            }
        }

        public Task<Enterprise> UpdateEnterprise(Enterprise enterprice)
        {
            try
            {
                if (enterprice == null) throw new Exception($"Error update Enterprice, entrprice is null");
                Enterprise oldEnterprice = _repository.GetById<Enterprise>(enterprice.Id).Result;

                oldEnterprice.Title = enterprice.Title;
                oldEnterprice.Email = enterprice.Email;

                return _repository.Update(oldEnterprice);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error update Enterprice, error:{ex}");
            }
        }

        public async Task<bool> HasEnterprise(int enterpriseId)
        {
            try
            {
                if (enterpriseId == null || enterpriseId == 0) throw new Exception("enterpriseId is null or less than 0");


                return _repository.GetById<Enterprise>(enterpriseId).IsCompleted;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

    }
}
