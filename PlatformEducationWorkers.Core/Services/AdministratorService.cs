using PlatformEducationWorkers.Core.Interfaces;
using PlatformEducationWorkers.Core.Models;


namespace PlatformEducationWorkers.Core.Services
{
    public class AdministratorService : IAdministratorService
    {
        private readonly IRepository _context;

        public AdministratorService(IRepository context)
        {
            _context = context;
        }

        public Task<Administrator> AddAdministrator(Administrator administrator)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAdministraor(int adminId)
        {
            throw new NotImplementedException();
        }

        public Task<Administrator> GetAdministratorById(int adminId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Administrator>> GetAllAdministratorsEnterprice(int enterpriceId)
        {
            throw new NotImplementedException();
        }

        public Task<Administrator> Login(string login, string password)
        {
            throw new NotImplementedException();
        }

        public Task<Administrator> Register(Administrator administrator)
        {
            throw new NotImplementedException();
        }

        public Task<Administrator> SearchAdministrator(string name, string surname, DateTime birthday)
        {
            throw new NotImplementedException();
        }

        public Task<Administrator> UpdateAdministrator(Administrator administrator)
        {
            throw new NotImplementedException();
        }
    }
}
