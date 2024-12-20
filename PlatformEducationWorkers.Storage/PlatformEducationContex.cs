using Microsoft.EntityFrameworkCore;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Storage
{
    public class PlatformEducationContex : DbContext
    {
        public PlatformEducationContex(DbContextOptions<PlatformEducationContex> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Enterprise>()
                .HasOne(e => e.Owner)
                .WithMany() // Вказує, що один користувач може бути власником кількох підприємств
                .HasForeignKey("OwnerId") // Додано вказівку зовнішнього ключа без властивості в моделі
                .OnDelete(DeleteBehavior.Restrict); // Або інший DeleteBehavior за необхідності
        }




        public DbSet<User> Users { get; set; }
        public DbSet<Enterprise> Enterprises { get; set; }
        public DbSet<Courses> Cources { get; set; }
        public DbSet<UserResults> UserResults { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
    }
}
