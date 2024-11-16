using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core.Models
{
    public class Cources
    {
        public int Id { get; set; }

        public virtual Enterprice Enterprise { get; set; }
        public string TitleCource { get; set; }
        public string Description { get; set; }
        public DateTime DateCreate { get; set; }
        public virtual ICollection<JobTitle> AccessRoles { get; set; }
        public string Questions { get; set; }//питання,варіанти відповіді, вірні відповіді записуються у json
        public string ContentCourse { get; set; }//вміст курсу записується у json

        public bool ShowCorrectAnswers { get; set; }
    }
}
