using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.Login_RegisterRequest
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Необхідно ввести логін.")]
        [StringLength(100, ErrorMessage = "Довжина облікового запису має бути принаймні {2} і не більше {1} символів.", MinimumLength = 3)]
        public string Login { get; set; }

        [Required(ErrorMessage = "Необхідно ввести пароль.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Пароль має містити принаймні {2} і не більше {1} символів.", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
