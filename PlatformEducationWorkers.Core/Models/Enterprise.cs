
namespace PlatformEducationWorkers.Core.Models
{
    public class Enterprise
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DateCreate { get; set; }
        public string Email { get; set; }
        public string PasswordEmail {  get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Courses> Courses { get; set; }
        public virtual User? Owner { get; set; }
    }
}
