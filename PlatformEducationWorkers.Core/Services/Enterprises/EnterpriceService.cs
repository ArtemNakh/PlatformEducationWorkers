using Amazon.S3.Model;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Core.Services.Enterprises
{
    public class EnterpriceService : IEnterpriseService
    {
        private readonly IRepository _repository;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        public EnterpriceService(IRepository repository, ICourseService courseService, IUserService userService)
        {
            _repository = repository;
            _courseService = courseService;
            _userService = userService;
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

                var courses=(await _courseService.GetAllCoursesEnterprise(enterpriceId)).ToList();

                foreach (var course in courses)
                {
                    await _courseService.DeleteCourse(course.Id);
                }
                
                var users = (await _userService.GetAllUsersEnterprise(enterpriceId)).ToList();
                foreach (var user in users)
                {
                    if (enterprice.Owner != null && user.Id == enterprice.Owner.Id)
                    {
                        enterprice.Owner = null;
                        await _repository.Update(enterprice);
                    }
                    await _userService.DeleteUser(user.Id);
                }

               
                var jobTitles = (await _repository.GetQueryAsync<JobTitle>(e=>e.Enterprise.Id== enterpriceId)).ToList();
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
                if(enterpriceId == null||enterpriceId == 0 )
                {
                    throw new Exception("enterprise is null or less than 0");
                }

                return _repository.GetById<Enterprise>(enterpriceId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error getting Enterprice by id, error:{ex}");
            }
        }

        public async Task<Enterprise> GetEnterprise(string title)
        {
            try
            {
                if (title == null|| title=="")
                {
                    throw new Exception("title is null or empty");
                }
                //додати валідацію
                return (await _repository.GetQueryAsync<Enterprise>(e => e.Title == title)).FirstOrDefault();

            }
            catch (Exception ex)
            {

                throw new Exception($"Error get Enterprice by title, error:{ex}");
            }
        }

        public async  Task<Enterprise> GetEnterpriseByUser(int userId)
        {
            try
            {
                if(userId==null|| userId==0)
                {
                    throw new Exception("user Id is null or less than 0");
                }
                User user = await _userService.GetUser(userId);
                //додати валідацію
                return (await _repository.GetQuery<Enterprise>(u => u.Id == user.Enterprise.Id)).FirstOrDefault();
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


                var enterprise = await _repository.GetByIdAsync<Enterprise>(enterpriseId);
                return enterprise != null;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

    }
}
