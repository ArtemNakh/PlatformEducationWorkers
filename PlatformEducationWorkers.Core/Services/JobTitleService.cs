using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Services
{
    /// <summary>
    /// Service for managing job titles.
    /// </summary>
    public class JobTitleService : IJobTitleService
    {
        private readonly IRepository _repository;
        private readonly IUserService _userService;
        private readonly IUserResultService _userResultService;
        private readonly ICourseService _courseService;

        /// <summary>
        /// Constructor for JobTitleService.
        /// </summary>
        /// <param name="repository">Repository for data access.</param>
        /// <param name="userService">Service for user management.</param>
        /// <param name="userResultService">Service for user results management.</param>
        /// <param name="courseService">Service for course management.</param>
        public JobTitleService(IRepository repository, IUserService userService, IUserResultService userResultService, ICourseService courseService)
        {
            _repository = repository;
            _userService = userService;
            _userResultService = userResultService;
            _courseService = courseService;
        }

        /// <summary>
        /// Adds a new job title.
        /// </summary>
        /// <param name="jobTitle">The JobTitle object to be added.</param>
        /// <returns>The added job title.</returns>
        public Task<JobTitle> AddingRole(JobTitle jobTitle)
        {
            try
            {
                // Check for the existence of the enterprise and the uniqueness of the job title
                if (jobTitle.Enterprise == null
                    || _repository.GetQuery<JobTitle>(r => r.Name == jobTitle.Name
                    && r.Enterprise.Id == jobTitle.Enterprise.Id).Result.Count() > 0)
                {
                    throw new Exception($"Error addingJobTitle, this state already exist ");
                }
                return _repository.Add(jobTitle);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error addingJobTitle, error:{ex}");
            }
        }

        /// <summary>
        /// Deletes a job title by its ID.
        /// </summary>
        /// <param name="idJobTitle">The ID of the job title to be deleted.</param>
        public async Task DeleteJobTitle(int idJobTitle)
        {
            try
            {
                if (idJobTitle == null)
                {
                    throw new Exception($"id jobTitle is null.");
                }

                JobTitle jobTitle = await _repository.GetByIdAsync<JobTitle>(idJobTitle);

                if (jobTitle == null)
                {
                    throw new Exception($"JobTitle with id {idJobTitle} not found.");
                }

                // Delete all users associated with this job title
                List<User> users = (await _userService.GetUsersByJobTitle(idJobTitle)).ToList();
                foreach (User user in users)
                {
                    // Delete all course results for the user
                     List<UserResults> userResults = (await _userResultService.GetAllUserResult(user.Id)).ToList();
                    foreach (var result in userResults)
                    {
                        await _userResultService.DeleteResult(result.Id);
                    }

                    await _userService.DeleteUser(user.Id);
                }

                // Remove the job title from the list of available roles in courses
                IEnumerable<Courses> courses = await _courseService.GetCoursesByJobTitle(idJobTitle);
                if (courses != null && courses.Any())
                {
                    foreach (var course in courses)
                    {
                        List<JobTitle> jobTitles = course.AccessRoles.ToList();
                        jobTitles.Remove(jobTitle);
                        course.AccessRoles = jobTitles;
                        await _courseService.UpdateCourse(course);
                    }
                }
                await _repository.Delete<JobTitle>(idJobTitle);
                return;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleted job Title, error:{ex}");
            }
        }

        /// <summary>
        /// Retrieves all job titles for a given enterprise.
        /// </summary>
        /// <param name="idEnterprise">The ID of the enterprise.</param>
        /// <returns>A list of job titles.</returns>
        public Task<IEnumerable<JobTitle>> GetAllJobTitles(int idEnterprise)
        {
            try
            {
                return _repository.GetQuery<JobTitle>(j => j.Enterprise.Id == idEnterprise);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get all job Title enterprice, error:{ex}");
            }
        }

        /// <summary>
        /// Retrieves a job title by its ID.
        /// </summary>
        /// <param name="idJobTitle">The ID of the job title.</param>
        /// <returns>The job title object.</returns>
        public Task<JobTitle> GetJobTitle(int idJobTitle)
        {
            try
            {
                return _repository.GetById<JobTitle>(idJobTitle);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get job Title by id, error:{ex}");
            }
        }

        /// <summary>
        /// Updates an existing job title.
        /// </summary>
        /// <param name="newJobTitle">The new job title object with updated information.</param>
        /// <returns>The updated job title.</returns>
        public Task<JobTitle> UpdateJobTitle(JobTitle newJobTitle)
        {
            try
            {
                JobTitle oldJobTitle = _repository.GetById<JobTitle>(newJobTitle.Id).Result;

                oldJobTitle.Name = newJobTitle.Name;
                return _repository.Update(oldJobTitle);

            }
            catch (Exception ex)
            {
                throw new Exception($"Error update job Title, error:{ex}");
            }
        }

        /// <summary>
        /// Retrieves available job roles for a given enterprise.
        /// </summary>
        /// <param name="enterpriseId">The ID of the enterprise.</param>
        /// <returns>A list of available job titles.</returns>
        public async Task<IEnumerable<JobTitle>> GetAvaliableRoles(int enterpriseId)
        {
            try
            {
                if (enterpriseId == null || enterpriseId < 0)
                {
                    throw new Exception($"Error get avaliable job Title, error:enterpriseId null or less than 0");
                }



                return (await _repository.GetQueryAsync<JobTitle>(n => n.Enterprise.Id == enterpriseId)).Skip(1);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error get avaliable job Title, error:{ex}");
            }

        }
    }
}
