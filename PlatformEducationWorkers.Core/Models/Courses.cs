

namespace PlatformEducationWorkers.Core.Models
{
    public class Courses
    {
        public int Id { get; set; }

        public virtual Enterprise Enterprise { get; set; }
        public string TitleCource { get; set; }
        public string Description { get; set; }
        public DateTime DateCreate { get; set; }
        public virtual ICollection<JobTitle> AccessRoles { get; set; }
        public string Questions { get; set; }
        public string ContentCourse { get; set; }//вміст курсу записується у json

        public bool ShowCorrectAnswers { get; set; }
        public bool ShowSelectedAnswers { get; set; }
        public bool ShowContextPassage { get; set; }
    }
}
