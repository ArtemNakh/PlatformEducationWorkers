using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Storage
{
    public class PlatformEducationContex: DbContext
    {
        public PlatformEducationContex(DbContextOptions<PlatformEducationContex> options) : base(options)
        {

        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Enterprice> Enterprises { get; set; }
        public DbSet<Cources> Cources { get; set; }
        public DbSet<UserResults> UserResults { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
    }
}
