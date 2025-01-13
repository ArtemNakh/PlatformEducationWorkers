
namespace PlatformEducationWorkers.Core.Models
{
    public class UserResults
    {
        public int Id { get; set; }
        public virtual User User  { get; set; }
        public virtual Courses Course { get; set; }
        public DateTime DateCompilation { get; set; }
        public int Rating { get; set; }
        public int MaxRating { get; set; }
        public string answerJson { get; set; }

        public bool IsRelevant { get; set; } = true;
    }
}
