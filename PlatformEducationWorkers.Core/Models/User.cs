﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime DateCreate { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string  Login { get; set; }

        public Enterprice Enterprise { get; set; }

        public Role Role { get; set; }
    }
}
