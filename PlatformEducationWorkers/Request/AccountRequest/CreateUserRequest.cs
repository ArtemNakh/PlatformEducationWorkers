using PlatformEducationWorkers.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.AccountRequest
{

    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Ім'я є обов'язковим.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Прізвище є обов'язковим.")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Дата народження є обов'язковою.")]
        public DateTime Birthday { get; set; }

        [Required(ErrorMessage = "Email є обов'язковим.")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пароль є обов'язковим.")]
        [StringLength(100, ErrorMessage = "Пароль повинен бути більше ніж 6 символів.", MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Логін є обов'язковим.")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Посада є обов'язковою.")]
        public int JobTitleId { get; set; }

        [Required(ErrorMessage = "Роль є обов'язковою.")]
        public Role Role { get; set; }
    }
}
