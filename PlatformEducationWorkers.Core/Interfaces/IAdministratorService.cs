using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Interfaces
{
    public interface IAdministratorService
    {
        Task<Administrator> Login(string login,string password);
        Task<Administrator> Register(Administrator administrator);
        Task<Administrator> AddAdministrator(Administrator administrator);
        Task<Administrator> UpdateAdministrator(Administrator administrator);
        Task DeleteAdministraor(int adminId);
        Task<IEnumerable<Administrator>> GetAllAdministratorsEnterprice(int enterpriceId);
        Task<Administrator> GetAdministratorById(int adminId);
        Task<Administrator> SearchAdministrator(string name, string surname, DateTime birthday);
    }
}
