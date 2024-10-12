using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Services
{
    public class UserResultService : IUserResultService
    {
        public Task<UserResults> AddResult(UserResults userResults)
        {
            throw new NotImplementedException();
        }

        public Task DeleteResult(int resultId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserResults>> GetAllResultEnterrprice(int enterpriceId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserResults>> GetAllUserResult(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<UserResults> SearchUserResult(int courceId)
        {
            throw new NotImplementedException();
        }

        public Task<UserResults> UpdateResult(UserResults userResults)
        {
            throw new NotImplementedException();
        }
    }
}
