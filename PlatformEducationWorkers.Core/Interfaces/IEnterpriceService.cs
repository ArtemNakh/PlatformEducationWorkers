using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlatformEducationWorkers.Core.Models;

namespace PlatformEducationWorkers.Core.Interfaces
{
    public interface IEnterpriceService
    {
        Task<Enterprice> AddingEnterprice(Enterprice enterprice);
        Task<Enterprice> UpdateingEnterprice(Enterprice enterprice);
        Task DeleteingEnterprice(int enterpriceId);
        Task<Enterprice> GetEnterprice(int enterpriceId);
        Task<Enterprice> GetEnterpriceByUser(int userId);
        Task<Enterprice> GetEnterprice(string title);
    }
}
