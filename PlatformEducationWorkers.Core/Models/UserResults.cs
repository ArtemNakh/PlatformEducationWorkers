using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Models
{
    public class UserResults
    {
        public int Id { get; set; }
        public virtual User User  { get; set; }
        public virtual Cources Cource { get; set; }
        public DateTime DateCompilation { get; set; }
        public int Rating { get; set; }
        public int MaxRating { get; set; }




    }
}
