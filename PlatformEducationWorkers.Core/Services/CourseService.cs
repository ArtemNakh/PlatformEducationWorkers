using Newtonsoft.Json;
using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Azure;
using PlatformEducationWorkers.Core.AddingModels.Questions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace PlatformEducationWorkers.Core.Services
{
    /// <summary>
    /// Service responsible for managing courses. Provides methods for creating, updating, deleting, 
    /// and retrieving courses as well as handling course-specific business logic.
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly IRepository _repository;
        private readonly IUserService _userService;
        private readonly IUserResultService _userResultService;
        private readonly AzureBlobCourseOperation AzureCourseOperation;

        private bool AreQuestionsEqual(List<QuestionContext> oldQuestions, List<QuestionContext> newQuestions)
        {
            if (oldQuestions.Count != newQuestions.Count)
                return false;

            for (int i = 0; i < oldQuestions.Count; i++)
            {
                var oldQuestion = oldQuestions[i];
                var newQuestion = newQuestions[i];

                if (oldQuestion.Text != newQuestion.Text ||
                    oldQuestion.PhotoQuestionBase64 != newQuestion.PhotoQuestionBase64 ||
                    oldQuestion.Answers.Count != newQuestion.Answers.Count)
                    return false;

                for (int j = 0; j < oldQuestion.Answers.Count; j++)
                {
                    var oldAnswer = oldQuestion.Answers[j];
                    var newAnswer = newQuestion.Answers[j];

                    if (oldAnswer.Text != newAnswer.Text ||
                        oldAnswer.IsCorrect != newAnswer.IsCorrect ||
                        oldAnswer.PhotoAnswerBase64 != newAnswer.PhotoAnswerBase64)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Fetches photos from Azure Blob storage for the list of courses.
        /// </summary>
        /// <param name="coursesEnterprise">List of courses to process.</param>
        private async Task GettingListsPhotosAzure(IEnumerable<Courses> coursesEnterprise)
        {
            foreach (Courses course in coursesEnterprise)
            {
                List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                course.Questions = JsonConvert.SerializeObject(questionContexts);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseService"/> class.
        /// </summary>
        /// <param name="repository">Repository for accessing and managing data.</param>
        /// <param name="azureCourseOperation">Service for Azure Blob operations related to courses.</param>
        /// <param name="userService">Service for managing user-related operations.</param>
        /// <param name="userResultService">Service for managing user course results.</param>
        public CourseService(IRepository repository, AzureBlobCourseOperation azureCourseOperation, IUserService userService, IUserResultService userResultService)
        {
            _repository = repository;
            AzureCourseOperation = azureCourseOperation;
            _userService = userService;
            _userResultService = userResultService;
        }

        /// <summary>
        /// Adds a new course to the system.
        /// </summary>
        /// <param name="courses">The course to be added.</param>
        /// <returns>The added course.</returns>
        /// <exception cref="Exception">Thrown when the course is null or an error occurs.</exception>
        public async Task<Courses> AddCourse(Courses courses)
        {
            try
            {
                //додати валідацію
                if (courses == null)
                    throw new Exception("Cource is null");

                ////getting photos from the cloud
                List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(courses.Questions);
                questionContexts = await AzureCourseOperation.UploadFileToBlobAsync(questionContexts);
                courses.Questions = JsonConvert.SerializeObject(questionContexts);

                return await _repository.Add(courses);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error adding cource,  error:{ex}");
            }
        }

        /// <summary>
        /// Deletes a course by its identifier and removes associated files from Azure Blob.
        /// </summary>
        /// <param name="courseId">The identifier of the course to delete.</param>
        /// <exception cref="Exception">Thrown when the course is not found or an error occurs.</exception>
        public async Task DeleteCourse(int courseId)
        {
            try
            {
                //додати валідацію
                if (courseId == null)
                    throw new Exception("Cource is null");

                Courses course = _repository.GetById<Courses>(courseId).Result;

                //deleting photos from the cloud
                List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                await AzureCourseOperation.DeleteFilesFromBlobAsync(questionContexts);

                await _repository.Delete<Courses>(courseId);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error delete cource,  error:{ex}");
            }
        }

        /// <summary>
        /// Retrieves all courses associated with a specific enterprise.
        /// </summary>
        /// <param name="enterpriseId">The ID of the enterprise.</param>
        /// <returns>List of courses for the enterprise.</returns>
        /// <exception cref="Exception">Thrown if the enterprise ID is invalid or an error occurs.</exception>
        public async Task<IEnumerable<Courses>> GetAllCoursesEnterprise(int enterpriseId)
        {
            try
            {
                //додати валідацію
                if (enterpriseId == null)
                    throw new Exception("enterprice is null");

                //getting photos from the cloud
                List<Courses> coursesEnterprise = (await _repository.GetQueryAsync<Courses>(u => u.Enterprise.Id == enterpriseId)).ToList();
                await GettingListsPhotosAzure(coursesEnterprise);

                return coursesEnterprise;
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get all cource enterprice by enterprice ,  error:{ex}");
            }
        }

        /// <summary>
        /// Retrieves all courses accessible to a specific user based on their job title.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>List of accessible courses for the user.</returns>
        /// <exception cref="Exception">Thrown if the user ID is invalid or an error occurs.</exception>
        public async Task<IEnumerable<Courses>> GetAllCoursesUser(int userId)
        {
            try
            {
                //додати валідацію
                if (userId == null)
                    throw new Exception("user is null");
                User user = await _userService.GetUser(userId);
                if (user == null)
                    throw new Exception("User not found");

                //getting photos from the cloud
                IEnumerable<Courses> courcesUser = (await _repository.GetQueryAsync<Courses>(c => c.AccessRoles.Any(ar => ar.Id == user.JobTitle.Id)));
                await GettingListsPhotosAzure(courcesUser);


                return courcesUser.ToList();
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get all cource enterprice by user ,  error:{ex}");
            }
        }

        /// <summary>
        /// Retrieves a course by its ID, including its associated photo data from Azure Blob storage.
        /// </summary>
        /// <param name="courseId">The ID of the course to retrieve.</param>
        /// <param name="original">Getting original entity or getting value for using.</param>
        /// <returns>The course with the specified ID.</returns>
        /// <exception cref="Exception">Thrown if the course ID is invalid or the course is not found.</exception>
        public async Task<Courses> GetCoursesById(int courseId, bool original = false)
        {
            try
            {
                //додати валідацію
                if (courseId == null)
                    throw new Exception("cource is null");

                Courses course = await _repository.GetByIdAsync<Courses>(courseId);
                if (course != null)
                {
                    if (original == false)
                    {
                        //getting photos from the cloud
                        if (course.Questions != null)
                        {
                            List<QuestionContext> questionContexts = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);
                            questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                            course.Questions = JsonConvert.SerializeObject(questionContexts);
                        }
                    }
                }
                return course;
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get cource by id ,  error:{ex}");
            }
        }

        /// <summary>
        /// Retrieves all courses accessible to a specific job title.
        /// </summary>
        /// <param name="jobTitleId">The ID of the job title.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a collection of courses associated with the specified job title.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if <paramref name="jobTitleId"/> is null or invalid, 
        /// or if an error occurs during the retrieval process.
        /// </exception>
        public async Task<IEnumerable<Courses>> GetCoursesByJobTitle(int jobTitleId)
        {
            try
            {
                if (jobTitleId == null)
                    throw new Exception("jobTitleId is null");

                JobTitle jobTitle = await _repository.GetByIdAsync<JobTitle>(jobTitleId);

                //getting photos from the cloud
                IEnumerable<Courses> courcesJobTitle = await _repository.GetQueryAsync<Courses>(n => n.AccessRoles.Contains(jobTitle));
                await GettingListsPhotosAzure(courcesJobTitle);

                return courcesJobTitle.ToList();

            }
            catch (Exception ex)
            {

                throw new Exception($"error, get courceByJobTitle, {ex}");
            }
        }



        /// <summary>
        /// Updates an existing course, including its Azure Blob data and associated roles.
        /// </summary>
        /// <param name="course">The course to update.</param>
        /// <returns>The updated course.</returns>
        /// <exception cref="Exception">Thrown if the course is invalid or an error occurs during the update.</exception>
        public async Task<Courses> UpdateCourse(Courses course)
        {
            try
            {
                if (course == null)
                    throw new Exception("cource is null");

                Courses oldCourse = await _repository.GetById<Courses>(course.Id, false);

                var updatedFields = new List<Expression<Func<Courses, object>>>();

                // Перевіряємо зміни в питаннях
                List<QuestionContext> oldQuestions = JsonConvert.DeserializeObject<List<QuestionContext>>(oldCourse.Questions);
                await AzureCourseOperation.UnloadFileFromBlobAsync(oldQuestions);
                List<QuestionContext> newQuestions = JsonConvert.DeserializeObject<List<QuestionContext>>(course.Questions);

                bool questionsChanged = !AreQuestionsEqual(oldQuestions, newQuestions);

                if (questionsChanged)
                {
                    // Оновлюємо статус IsRelevant на false для результатів цього курсу
                    List<UserResults> usersResults = (await _userResultService.GetAllResultCourses(oldCourse.Id)).ToList();
                    foreach (var userResult in usersResults)
                    {
                        await _userResultService.UpdateIsRelevantStatus(userResult.Id, false); // Оновлення статусу в базі даних
                    }

                    // Очищення старих зображень з Blob
                    await AzureCourseOperation.DeleteFilesFromBlobAsync(oldQuestions);

                    // Завантаження нових зображень у Blob
                    newQuestions = await AzureCourseOperation.UploadFileToBlobAsync(newQuestions);
                    course.Questions = JsonConvert.SerializeObject(newQuestions);
                }


                Courses updatingCourse = await _repository.GetById<Courses>(course.Id);

                if (updatingCourse.TitleCource != course.TitleCource)
                {
                    updatingCourse.TitleCource = course.TitleCource;
                    updatedFields.Add(c => c.TitleCource);
                }

                ///доробити
                /////find JobTitle for deleting (якщо вони більше не в списку)
                var jobTitlesToRemove = updatingCourse.AccessRoles
     .Where(cjt => !course.AccessRoles.Any(jt => jt.Id == cjt.Id))
     .ToList();

                // Remove redundant connections
                if (jobTitlesToRemove.Any())
                {
                    // Remove JobTitles from AccessRoles
                    foreach (var jobTitle in jobTitlesToRemove)
                    {
                        updatingCourse.AccessRoles.Remove(jobTitle);
                    }

                    // Get all user results for the course
                    var usersResults = (await _userResultService.GetAllResultCourses(course.Id)).ToList();

                    // Find and remove results related to the removed JobTitles
                    foreach (var result in usersResults)
                    {
                        if (jobTitlesToRemove.Any(jt => result.User.JobTitle.Id == jt.Id))
                        {
                            await _userResultService.DeleteResult(result.Id);
                        }
                    }
                }

                // Додаємо нові ролі, які ще не є в AccessRoles курсу
                var jobTitlesToAdd = course.AccessRoles
                    .Where(jt => !updatingCourse.AccessRoles.Any(cjt => cjt.Id == jt.Id))
                    .ToList();

                if (jobTitlesToAdd.Any())
                {
                    foreach (var jobTitle in jobTitlesToAdd)
                    {
                        updatingCourse.AccessRoles.Add(jobTitle);
                    }
                    updatedFields.Add(c => c.AccessRoles);
                }

                if (updatingCourse.ContentCourse != course.ContentCourse)
                {
                    updatingCourse.ContentCourse = course.ContentCourse;
                    updatedFields.Add(c => c.ContentCourse);
                }

                if (updatingCourse.Description != course.Description)
                {
                    updatingCourse.Description = course.Description;
                    updatedFields.Add(c => c.Description);
                }

                if (questionsChanged)
                {
                    updatingCourse.Questions = course.Questions;
                    updatedFields.Add(c => c.Questions);
                }
                return await _repository.UpdateOnlySelected(updatingCourse, updatedFields.ToArray());
            }
            catch (Exception ex)
            {

                throw new Exception($"Error get cource by id ,  error:{ex}");
            }
        }


        /// <summary>
        /// Retrieves a list of uncompleted courses for a specific user based on their job title and the enterprise ID.
        /// </summary>
        /// <param name="userId">The ID of the user for whom to retrieve uncompleted courses.</param>
        /// <param name="enterpriceId">The ID of the enterprise to which the courses belong.</param>
        /// <returns>An asynchronous task that returns an enumerable collection of uncompleted courses.</returns>
        /// <exception cref="Exception">Thrown when userId or enterpriceId is null or when an error occurs during retrieval.</exception>
        public async Task<IEnumerable<Courses>> GetUncompletedCoursesForUser(int userId, int enterpriceId)
        {
            try
            {
                // Validate input parameters
                if (userId == null)
                    throw new Exception("UserId is null");
                if (enterpriceId == null)
                    throw new Exception("enterprice is null");

                // Asynchronously retrieve the user's results
                var userCourses = await _userResultService.GetAllUserResult(userId);

                // Asynchronously retrieve all courses for the enterprise
                var allCourses = await _repository.GetQueryAsync<Courses>(u => u.Enterprise.Id == enterpriceId);

                // Asynchronously retrieve the user details
                var user = await _userService.GetUser(userId);

                // List to store uncompleted courses
                List<Courses> uncompletedCourses = new List<Courses>();

                // Iterate through all courses to find uncompleted ones
                foreach (var course in allCourses)
                {
                    // Check if the course has been completed
                    bool courseCompleted = userCourses.Any(uc => uc.Course.Id == course.Id);

                    // Check if the user's job title matches the course's access roles
                    bool isJobTitleMatch = course.AccessRoles.Any(c => c.Id == user.JobTitle.Id);

                    // If the course is accessible by the user's job title and has not been completed
                    if (isJobTitleMatch && !courseCompleted)
                    {
                        uncompletedCourses.Add(course);
                    }
                }

                // Retrieve photos for the uncompleted courses from Azure
                await GettingListsPhotosAzure(uncompletedCourses);

                // Повертаємо список непройдених курсів
                return await Task.FromResult(uncompletedCourses.AsEnumerable());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting uncompleted courses for user, error: {ex}");
            }
        }

        /// <summary>
        /// Retrieves the N most recently created courses for a specific enterprise.
        /// </summary>
        /// <param name="enterpriseId">The ID of the enterprise for which to retrieve new courses.</param>
        /// <param name="numberNewsCourses">The numbers of the new courses need getting </param>
        /// <returns>An asynchronous task that returns an enumerable collection of the five most recent courses.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during retrieval.</exception>
        public async Task<IEnumerable<Courses>> GetNewCourses(int enterpriseId, int numberNewsCourses)
        {
            try
            {
                IEnumerable<Courses> cources = (await _repository.GetQueryAsync<Courses>(
                    c => c.Enterprise.Id == enterpriseId)).OrderByDescending(course => course.DateCreate).Take(numberNewsCourses);

                // Retrieve photos for the courses from Azure
                await GettingListsPhotosAzure(cources);

                return cources;

            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving new courses: {ex.Message}", ex);
            }
        }
    }
}
