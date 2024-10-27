using PlatformEducationWorkers.Core.Interfaces;
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

        public Task DeleteResult(int resultId)
        {
            try
            {
                if (resultId == null)
                    throw new Exception("$Error delete results,resultId is null");
                return _repository.Delete<UserResults>(resultId);
            }
            catch (Exception)
            {
                throw new Exception("$Error delete results,error: {ex}");
            }
        }

        public Task<IEnumerable<UserResults>> GetAllResultEnterprice(int enterpriceId)
        {
            try
            {
                if (enterpriceId == null)
                    throw new Exception("$Error  get all user results by entyerprice,enterpriceId is null");
                return _repository.GetQuery<UserResults>(u => u.Cource.Enterprise.Id == enterpriceId);
            }
            catch (Exception)
            {
                throw new Exception("$Error get all results,error: {ex}");
            }
        }

       

        public Task<IEnumerable<UserResults>> GetAllUserResult(int userId)
        {
            try
            {
                if (userId == null)
                    throw new Exception("$Error  get  user  all results,userId is null");
                return _repository.GetQuery<UserResults>(u => u.User.Id == userId);
            }
            catch (Exception)
            {
                throw new Exception("$Error get all results,error: {ex}");
            }
        }

        public Task<UserResults> SearchUserResult(int courceId)
        {
            try
            {
                if (courceId == null)
                    throw new Exception("$Error  get user  results,courceId is null");
                return Task.FromResult(_repository.GetQuery<UserResults>(u => u.Cource.Id == courceId).Result.FirstOrDefault());
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
    }
}
