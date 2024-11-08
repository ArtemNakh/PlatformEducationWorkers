
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Interfaces
{
    public interface ICourcesService
    {
        Task<Cources> AddCource(Cources cources);
        Task<Cources> UpdateCource(Cources cources);
        Task DeleteCource(int courceId);
        Task<IEnumerable<Cources>> GetAllCourcesEnterprice(int enterpriceId);
        Task<IEnumerable<Cources>> GetAllCourcesUser(int userId);
        Task<Cources> GetCourcesById(int courceId);
        Task<IEnumerable<Cources>> GetCourcesByJobTitle(int jobTitleId,int enterpriceId);
        Task<IEnumerable<Cources>> GetUncompletedCourcesForUser(int userId, int enterpriceId);
    }
}
