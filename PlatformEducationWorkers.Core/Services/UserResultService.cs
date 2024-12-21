using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Core;
using PlatformEducationWorkers.Core.Azure;
using PlatformEducationWorkers.Core.AddingModels.UserResults;
using Newtonsoft.Json;
using System.Collections.Generic;
using PlatformEducationWorkers.Core.AddingModels.Questions;

namespace PlatformEducationWorkers.Core.Services
{
    public class UserResultService : IUserResultService
    {
        private readonly IRepository _repository;
        private readonly EmailService _emailService;
        private readonly AzureBlobCourseOperation AzureCourseOperation;

        public UserResultService(IRepository repository, EmailService emailService, AzureBlobCourseOperation azureCourseOperation)
        {
            _repository = repository;
            _emailService = emailService;
            AzureCourseOperation = azureCourseOperation;
        }

        public async Task<UserResults> AddResult(UserResults userResults)
        {
            try
            {
                if (userResults == null)
                    throw new Exception("$Error adding results,userResults is null");

                List<UserQuestionRequest> userQuestions = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResults.answerJson);
                userQuestions = await AzureCourseOperation.UploadFileToBlobAsync(userQuestions);

                userResults.answerJson = JsonConvert.SerializeObject(userQuestions);

                UserResults userResult = await _repository.Add(userResults);


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

        public async Task DeleteResult(int resultId)
        {
            try
            {
                if (resultId == null)
                    throw new Exception("$Error delete results,resultId is null");
                UserResults userResult = await _repository.GetById<UserResults>(resultId);
                List<UserQuestionRequest> questionContexts = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResult.answerJson);

                await AzureCourseOperation.DeleteFilesFromBlobAsync(questionContexts);


                await _repository.Delete<UserResults>(resultId);
            }
            catch (Exception)
            {
                throw new Exception("$Error delete results,error: {ex}");
            }
        }

        public async Task<IEnumerable<UserResults>> GetAllResultEnterprice(int enterpriseId)
        {
            try
            {
                if (enterpriseId == null)
                    throw new Exception("$Error  get all user results by entyerprice,enterpriceId is null");

                IEnumerable<UserResults> resultsEnterprise = (await _repository.GetQuery<UserResults>(u => u.Course.Enterprise.Id == enterpriseId));
                foreach (UserResults userResult in resultsEnterprise)
                {
                    List<UserQuestionRequest> questionContexts = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResult.answerJson);
                    questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                    userResult.answerJson = JsonConvert.SerializeObject(questionContexts);
                }
                return resultsEnterprise;
            }
            catch (Exception ex)
            {
                throw new Exception("$Error get all results,error: {ex}");
            }
        }



        public async Task<IEnumerable<UserResults>> GetAllUserResult(int userId)
        {
            try
            {
                if (userId == null)
                    throw new Exception("$Error  get  user  all results,userId is null");
                // Виконання запиту для отримання результатів користувача
                var results = await _repository.GetQueryAsync<UserResults>(u => u.User.Id == userId);
                foreach (UserResults userResult in results)
                {
                    List<UserQuestionRequest> questionContexts = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResult.answerJson);
                    questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                    userResult.answerJson = JsonConvert.SerializeObject(questionContexts);
                }
                return results;
            }
            catch (Exception)
            {
                throw new Exception("$Error get all results,error: {ex}");
            }
        }

        public async Task<UserResults> SearchUserResult(int courceId)
        {
            try
            {
                if (courceId == null)
                    throw new Exception("$Error  get user  results,courceId is null");

                UserResults userResult = (await _repository.GetQueryAsync<UserResults>(u => u.Course.Id == courceId)).FirstOrDefault();


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

        public Task<UserResults> UpdateResult(UserResults userResults)
        {
            //todo
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<UserResults>> GetLastPassages(int enterpriceId, int numbersPassage)
        {

            try
            {
                if (enterpriceId == 0)
                    throw new ArgumentException("Enterprise ID cannot be null or zero", nameof(enterpriceId));
                if (numbersPassage <= 0)
                    throw new ArgumentException("numbers last passage can`t be less 1", nameof(numbersPassage));

                // Отримати останні проходження курсів
                var results = await _repository.GetQueryAsync<UserResults>(
                    u => u.Course.Enterprise.Id == enterpriceId
                );


                foreach (UserResults userResult in results)
                {
                    List<UserQuestionRequest> questionContexts = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResult.answerJson);
                    questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                    userResult.answerJson = JsonConvert.SerializeObject(questionContexts);
                }
                // Сортуємо за датою завершення та беремо останні 5 записів
                return results
                    .OrderByDescending(u => u.DateCompilation)
                    .Take(numbersPassage);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving last passages: {ex.Message}", ex);
            }
        }

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


        public async Task<IEnumerable<UserResults>> GetAllResultCourses(int CourseId)
        {
            try
            {
                if (CourseId == null)
                    throw new Exception("$Error  get  user  all results,userId is null");


                var results = await _repository.GetQueryAsync<UserResults>(u => u.Course.Id == CourseId);

                foreach (UserResults userResult in results)
                {
                    List<UserQuestionRequest> questionContexts = JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResult.answerJson);
                    questionContexts = await AzureCourseOperation.UnloadFileFromBlobAsync(questionContexts);
                    userResult.answerJson = JsonConvert.SerializeObject(questionContexts);
                }
                return results;
            }
            catch (Exception)
            {
                throw new Exception("$Error get all results,error: {ex}");
            }
        }

        public async Task DeleteAllResultsUser(int userId)
        {
            if (userId == null || userId < 0)
            {
                throw new Exception("userId is null or less than 0");
            }

            IEnumerable<UserResults> userResults = await _repository.GetQueryAsync<UserResults>(u => u.User.Id == userId);

            foreach (UserResults userResult in userResults)
            {
                if (userResult.answerJson != null)
                {
                    List<UserQuestionRequest> userQuestions=JsonConvert.DeserializeObject<List<UserQuestionRequest>>(userResult.answerJson);
                    
                    await AzureCourseOperation.DeleteFilesFromBlobAsync(userQuestions);

                }
                await _repository.Delete<UserResults>(userResult.Id);
            }
        }
    }
}
