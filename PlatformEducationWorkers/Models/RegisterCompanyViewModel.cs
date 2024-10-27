using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Models
{
    public class RegisterCompanyViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string OwnerName { get; set; }

        [Required]
        public string OwnerSurname { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Login { get; set; }
    }

}
