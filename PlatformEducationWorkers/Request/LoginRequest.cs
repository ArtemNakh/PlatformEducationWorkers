using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Login is required.")]
        [StringLength(100, ErrorMessage = "Login must be at least {2} and at most {1} characters long.", MinimumLength = 3)]
        public string Login { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
