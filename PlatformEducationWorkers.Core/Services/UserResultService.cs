using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core.Azure;
using PlatformEducationWorkers.Core.AddingModels.UserResults;
using Newtonsoft.Json;

namespace PlatformEducationWorkers.Core.Services
{
    /// <summary>
    /// Service class for managing user results, including adding, deleting, and retrieving user course results.
    /// </summary>
    public class UserResultService : IUserResultService
    {
        private readonly IRepository _repository;
        private readonly EmailService _emailService;
        private readonly AzureBlobCourseOperation AzureCourseOperation;

        // <summary>
        /// Helper method to retrieve and update photos from Azure Blob storage for a list of user results.
        /// </summary>
        /// <param name="resultsEnterprise">List of user results to process.</param>
        private async Task GettingListPhotosAzure(IEnumerable<UserResults> resultsEnterprise)
        {
            foreach (UserResults userResult in resultsEnterprise)
            {
                List<UserQuestionRequest> questionContexts = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResult.answerJson);
                questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                userResult.answerJson = JsonConvert.SerializeObject(questionContexts);
            }
        }

        /// <summary>
        /// Constructor to initialize dependencies.
        /// </summary>
        public UserResultService(IRepository repository, EmailService emailService, AzureBlobCourseOperation azureCourseOperation)
        {
            _repository = repository;
            _emailService = emailService;
            AzureCourseOperation = azureCourseOperation;
        }

        /// <summary>
        /// Adds a new user result and handles associated file uploads and email notifications.
        /// </summary>
        public async Task<UserResults> AddResult(UserResults userResults)
        {
            try
            {
                if (userResults == null)
                    throw new Exception("$Error adding results,userResults is null");

                // Deserialize and upload files associated with user questions
                List<UserQuestionRequest> userQuestions = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResults.answerJson);
                userQuestions = await AzureCourseOperation.UploadFileToBlobAsync(userQuestions);

                // Update user results JSON
                userResults.answerJson = JsonConvert.SerializeObject(userQuestions);

                // Save user result to the repository
                UserResults userResult = await _repository.Add(userResults);

                // Send email notification
                var enterpriseEmail = userResults.User.Enterprise.Email;
                var subject = "Проходження курсу";
                var body = $"<p>Шановний {userResults.User.Name} {userResults.User.Surname},</p>" +
                     $"<p>Ви пройшли курс: {userResults.Course.TitleCource}</p>" +
                           $"<p>З найкращими побажаннями,<br>Команда {userResults.User.Enterprise.Title}</p>";

                await _emailService.SendEmailAsync(userResults.User.Enterprise.Email, userResults.User.Enterprise.PasswordEmail, userResults.User.Email, subject, body);

                return userResult;
            }
            catch (Exception)
            {
                throw new Exception("$Error adding results,error: {ex}");
            }
        }

        /// <summary>
        /// Deletes a user result by ID and handles associated cleanup of files and email notifications.
        /// </summary>
        public async Task DeleteResult(int resultId)
        {
            try
            {
                if (resultId == null)
                    throw new Exception("$Error delete results,resultId is null");

                // Retrieve user result by ID
                UserResults userResult = await _repository.GetById<UserResults>(resultId);

                // Deserialize and delete associated files from Azure Blob storage
                List<UserQuestionRequest> questionContexts = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResult.answerJson);
                await AzureCourseOperation.DeleteFilesFromBlobAsync(questionContexts);

                // Send email notification
                var enterpriseEmail = userResult.Course.Enterprise.Email;
                var subject = "Видалення проходження!";
                var body = $"<p>Шановний {userResult.User.Name} {userResult.User.Surname},</p>" +
                           $"<p>Вваш результат з курса{userResult.Course.TitleCource} був видален .</p>" +
                           $"<p>З найкращими побажаннями,<br>Команда {userResult.Course.Enterprise.Title}</p>";

                await _emailService.SendEmailAsync(userResult.Course.Enterprise.Email, userResult.Course.Enterprise.PasswordEmail, userResult.User.Email, subject, body);
                
                
                // Delete the result from the repository
                await _repository.Delete<UserResults>(resultId);
            }
            catch (Exception)
            {
                throw new Exception("$Error delete results,error: {ex}");
            }
        }

        /// <summary>
        /// Retrieves all user results associated with a specific enterprise.
        /// </summary>
        public async Task<IEnumerable<UserResults>> GetAllResultEnterprice(int enterpriseId)
        {
            try
            {
                if (enterpriseId == null)
                    throw new Exception("$Error  get all user results by entyerprice,enterpriceId is null");

                // Retrieve results from the repository
                IEnumerable<UserResults> resultsEnterprise = (await _repository.GetQuery<UserResults>(u => u.Course.Enterprise.Id == enterpriseId));

                // Receiving photos for the results
                await GettingListPhotosAzure(resultsEnterprise);
             
                return resultsEnterprise;
            }
            catch (Exception ex)
            {
                throw new Exception("$Error get all results,error: {ex}");
            }
        }


        /// <summary>
        /// Retrieves all results for a specific user.
        /// </summary>
        public async Task<IEnumerable<UserResults>> GetAllUserResult(int userId)
        {
            try
            {
                if (userId == null)
                    throw new Exception("$Error  get  user  all results,userId is null");

                // Retrieve user results
                var results = await _repository.GetQueryAsync<UserResults>(u => u.User.Id == userId);

                // Receiving photos for the results
                await GettingListPhotosAzure(results);

                return results;
            }
            catch (Exception)
            {
                throw new Exception("$Error get all results,error: {ex}");
            }
        }

        /// <summary>
        /// Retrieves a specific user result based on course ID.
        /// </summary>
        public async Task<UserResults> SearchUserResult(int courceId)
        {
            try
            {
                if (courceId == null)
                    throw new Exception("$Error  get user  results,courceId is null");

                // Retrieve user result by course ID
                UserResults userResult = (await _repository.GetQueryAsync<UserResults>(u => u.Course.Id == courceId)).FirstOrDefault();

                // Retrieve photos for the result
                List<UserQuestionRequest> questionContexts = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResult.answerJson);
                questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                userResult.answerJson = JsonConvert.SerializeObject(questionContexts);

                return userResult;
            }
            catch (Exception)
            {
                throw new Exception("$Error get user results,error: {ex}");
            }
        }

        /// <summary>
        /// Retrieves the last N course passages for a specific enterprise.
        /// </summary>
        public async Task<IEnumerable<UserResults>> GetLastPassages(int enterpriceId, int numbersPassage)
        {

            try
            {
                if (enterpriceId == 0)
                    throw new ArgumentException("Enterprise ID cannot be null or zero", nameof(enterpriceId));
                if (numbersPassage <= 0)
                    throw new ArgumentException("numbers last passage can`t be less 1", nameof(numbersPassage));

                // Retrieve results  and take need numbers
                var results =( await _repository.GetQueryAsync<UserResults>(
                    u => u.Course.Enterprise.Id == enterpriceId
                )).Take(numbersPassage);

                // Retrieve photos for the results
                await GettingListPhotosAzure(results);



                // sort by completion date
                return results.OrderByDescending(u => u.DateCompilation);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving last passages: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Calculates the average rating for all courses within a specific enterprise.
        /// </summary>
        public async Task<double> GetAverageRating(int enterpriseId)
        {
            try
            {
                var results = await _repository.GetQueryAsync<UserResults>(
                    r => r.Course.Enterprise.Id == enterpriseId
                );

                if (results.Any())
                {
                    return results.Average(r => (double)r.Rating / r.MaxRating) * 100;
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating average rating: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves all results for a specific course.
        /// </summary>
        public async Task<IEnumerable<UserResults>> GetAllResultCourses(int CourseId)
        {
            try
            {
                if (CourseId == null)
                    throw new Exception("$Error  get  user  all results,userId is null");

                // Retrieve user results
                var results = await _repository.GetQueryAsync<UserResults>(u => u.Course.Id == CourseId);

                // Receiving photos for the results
                await GettingListPhotosAzure(results);

                
                return results;
            }
            catch (Exception)
            {
                throw new Exception("$Error get all results,error: {ex}");
            }
        }

        /// <summary>
        /// Deletes all results for a specific user and cleans up associated files in Azure Blob storage.
        /// </summary>
        public async Task DeleteAllResultsUser(int userId)
        {
            if (userId == null || userId < 0)
            {
                throw new Exception("userId is null or less than 0");
            }

            IEnumerable<UserResults> userResults = await _repository.GetQueryAsync<UserResults>(u => u.User.Id == userId);

            foreach (UserResults userResult in userResults)
            {
                if (!string.IsNullOrEmpty(userResult.answerJson))
                {
                    // Deserialize and delete associated files from Azure Blob storage
                    List<UserQuestionRequest> userQuestions=JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResult.answerJson);
                    await AzureCourseOperation.DeleteFilesFromBlobAsync(userQuestions);
                }

                // Delete result from the repository
                await _repository.Delete<UserResults>(userResult.Id);
            }
        }
    }
}
