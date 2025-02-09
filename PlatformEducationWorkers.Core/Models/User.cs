
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
        public string Salt { get; set; } 
        public string? ProfileAvatar { get; set; }
        public virtual Enterprise Enterprise { get; set; }
        public virtual JobTitle JobTitle { get; set; }
        public Role Role { get; set; }

    }

  
}
