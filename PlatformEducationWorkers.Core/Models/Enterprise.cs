using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Models
{
    public class Enterprice
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DateCreate { get; set; }
        public string Email { get; set; }

        public virtual IEnumerable<User> Users { get; set; }
        public virtual IEnumerable<Cources> Cources { get; set; }
       // public virtual IEnumerable<Administrator> Administrators { get; set; }

    }
}
