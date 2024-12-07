using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatformEducationWorkers.Core.Models;
namespace PlatformEducationWorkers.Core.Interfaces
{
    public interface IUserResultService
    {
        Task<UserResults> AddResult(UserResults userResults);
        Task<UserResults> UpdateResult(UserResults userResults);
        Task DeleteResult(int resultId);
        Task<IEnumerable<UserResults>> GetAllUserResult(int userId);
        Task<IEnumerable<UserResults>> GetAllResultEnterprice(int enterpriceId);
        Task<IEnumerable<UserResults>> GetAllResultCourses(int CourseId);
        Task<UserResults> SearchUserResult(int courceId);

        Task<IEnumerable<UserResults>> GetLastPassages(int enterpriseId,int numbersPassage);

        Task<double> GetAverageRating(int enterpriseId);
    }
}
