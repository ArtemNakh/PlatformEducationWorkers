using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Interfaces.Repositories;
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Services
{
    public class UserResultService : IUserResultService
    {
        private readonly IRepository _repository;

        public UserResultService(IRepository repository)
        {
            _repository = repository;
        }

        public Task<UserResults> AddResult(UserResults userResults)
        {
            try
            {
                if (userResults == null)
                    throw new Exception("$Error adding results,userResults is null");
                return _repository.Add(userResults);
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

                await _repository.Delete<UserResults>(resultId);
            }
            catch (Exception)
            {
                throw new Exception("$Error delete results,error: {ex}");
            }
        }

        public async Task<IEnumerable<UserResults>> GetAllResultEnterprice(int enterpriceId)
        {
            try
            {
                if (enterpriceId == null)
                    throw new Exception("$Error  get all user results by entyerprice,enterpriceId is null");
                return await _repository.GetQueryAsync<UserResults>(u => u.Cource.Enterprise.Id == enterpriceId);
            }
            catch (Exception)
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
                return (await _repository.GetQueryAsync<UserResults>(u => u.Cource.Id == courceId)).FirstOrDefault();
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

        public async Task<IEnumerable<UserResults>> GetLastPassages(int enterpriceId)
        {

            try
            {
                if (enterpriceId == 0)
                    throw new ArgumentException("Enterprise ID cannot be null or zero", nameof(enterpriceId));

                // Отримати останні проходження курсів
                var results = await _repository.GetQueryAsync<UserResults>(
                    u => u.Cource.Enterprise.Id == enterpriceId 
                );

                // Сортуємо за датою завершення та беремо останні 5 записів
                return results
                    .OrderByDescending(u => u.DateCompilation)
                    .Take(5);
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
                    r => r.Cource.Enterprise.Id == enterpriseId
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

    }
}
