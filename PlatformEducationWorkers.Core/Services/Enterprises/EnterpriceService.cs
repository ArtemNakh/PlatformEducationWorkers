using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Enterprises;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;


namespace PlatformEducationWorkers.Core.Services.Enterprises
{
    /// <summary>
    /// Service class responsible for managing enterprise-related operations,
    /// including retrieving, updating, and deleting enterprises and their associated entities.
    /// </summary>
    public class EnterpriceService : IEnterpriseService
    {
        private readonly IRepository _repository;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly IJobTitleService _jobTitleService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnterpriceService"/> class.
        /// </summary>
        /// <param name="repository">The repository interface for data operations.</param>
        /// <param name="courseService">Service for managing courses.</param>
        /// <param name="userService">Service for managing users.</param>
        /// <param name="jobTitleService">Service for managing job titles.</param>
        public EnterpriceService(IRepository repository, ICourseService courseService, IUserService userService, IJobTitleService jobTitleService)
        {
            _repository = repository;
            _courseService = courseService;
            _userService = userService;
            _jobTitleService = jobTitleService;
        }


        /// <summary>
        /// Deletes an enterprise and all its associated entities such as courses, users, and job titles.
        /// </summary>
        /// <param name="enterpriceId">The ID of the enterprise to delete.</param>
        /// <exception cref="Exception">Throws if the enterprise or related entities cannot be deleted.</exception>
        public async Task DeleteingEnterprise(int enterpriceId)
        {
            try
            {
                // Retrieve the enterprise to delete
                var enterprice = await _repository.GetById<Enterprise>(enterpriceId);
                if (enterprice == null)
                    throw new Exception($"Enterprice with id {enterpriceId} not found");

                // Delete associated courses
                var courses = (await _courseService.GetAllCoursesEnterprise(enterpriceId)).ToList();
                foreach (var course in courses)
                {
                    await _courseService.DeleteCourse(course.Id);
                }

                // Delete associated users
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

                // Delete associated job titles
                IEnumerable<JobTitle> jobTitles = await _jobTitleService.GetAllJobTitles(enterpriceId);
                foreach (var jobTitle in jobTitles)
                {
                    _jobTitleService.DeleteJobTitle(jobTitle.Id);
                }


                // Delete the enterprise
                await _repository.Delete<Enterprise>(enterpriceId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting Enterprice and its related entities: {ex.Message}");
            }
        }


        /// <summary>
        /// Retrieves an enterprise by its ID.
        /// </summary>
        /// <param name="enterpriceId">The ID of the enterprise.</param>
        /// <returns>The enterprise entity.</returns>
        /// <exception cref="Exception">Throws if the enterprise ID is invalid or not found.</exception>
        public Task<Enterprise> GetEnterprise(int enterpriceId)
        {
            try
            {
                if (enterpriceId == null || enterpriceId == 0)
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

        /// <summary>
        /// Retrieves an enterprise by its title.
        /// </summary>
        /// <param name="title">The title of the enterprise.</param>
        /// <returns>The enterprise entity.</returns>
        /// <exception cref="Exception">Throws if the title is null or empty.</exception>
        public async Task<Enterprise> GetEnterprise(string title)
        {
            try
            {
                if (title == null || title == "")
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

        /// <summary>
        /// Retrieves an enterprise by a user's ID.
        /// </summary>
        /// <param name="userId">The ID of the user associated with the enterprise.</param>
        /// <returns>The enterprise entity.</returns>
        /// <exception cref="Exception">Throws if the user ID is invalid or the user has no associated enterprise.</exception>
        public async Task<Enterprise> GetEnterpriseByUser(int userId)
        {
            try
            {
                if (userId == null || userId == 0)
                {
                    throw new Exception("user Id is null or less than 0");
                }

                User user = await _userService.GetUser(userId);

                return (await _repository.GetQuery<Enterprise>(u => u.Id == user.Enterprise.Id)).FirstOrDefault();
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get Enterprice by user, error:{ex}");
            }
        }

        /// <summary>
        /// Updates an enterprise entity.
        /// </summary>
        /// <param name="enterprice">The enterprise entity with updated values.</param>
        /// <returns>The updated enterprise entity.</returns>
        /// <exception cref="Exception">Throws if the enterprise is null or update operation fails.</exception>
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

        

    }
}
