﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Models
{
    public class JobTitle
    {
        public int Id { get; set; }
        public string  Name { get; set; }

        public virtual Enterprise Enterprise { get; set; }


        public virtual ICollection<Courses> Courses { get; set; }




    }
}
