using PlatformEducationWorkers.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request
{
    public class UpdateUserRequest
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

        // Поле Password може бути необов'язковим, якщо ви не хочете оновлювати пароль
        public string? Password { get; set; }

        // Логін можна не змінювати, тому це поле можна зробити необов'язковим
        public string? Login { get; set; }

        [Required(ErrorMessage = "Посада є обов'язковою.")]
        public int JobTitleId { get; set; }

        [Required(ErrorMessage = "Роль є обов'язковою.")]
        public Role Role { get; set; }
    }
}
