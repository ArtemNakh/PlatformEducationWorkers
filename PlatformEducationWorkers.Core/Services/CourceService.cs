using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;


namespace PlatformEducationWorkers.Core.Services
{
    public class CourceService : ICourcesService
    {
        public Task<Cources> AddCource(Cources cources)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCource(int courceId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Cources>> GetAllCourcesEnterprice(int enterpriceId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Cources>> GetAllCourcesUser(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<Cources> GetCourcesById(int courceId)
        {
            throw new NotImplementedException();
        }

        public Task<Cources> GetCourcesBytitle(int titleCource)
        {
            throw new NotImplementedException();
        }

        public Task<Cources> UpdateCource(Cources cources)
        {
            throw new NotImplementedException();
        }
    }
}
