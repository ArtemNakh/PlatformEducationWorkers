using PlatformEducationWorkers.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.AccountRequest
{
    public class UpdateUserRequest : IValidatableObject
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

        [StringLength(40, MinimumLength = 6, ErrorMessage = "Пароль повинен бути від 6 до 40 символів")]
        public string? Password { get; set; }

        [StringLength(40, MinimumLength = 6, ErrorMessage = "Логін повинен бути від 6 до 40 символів")]
        public string? Login { get; set; }

        [Required(ErrorMessage = "Посада є обов'язковою.")]
        public int JobTitleId { get; set; }

        [Required(ErrorMessage = "Роль є обов'язковою.")]
        public Role Role { get; set; }

        public IFormFile? ProfileAvatar { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Login) && string.IsNullOrEmpty(Password))
            {
                yield return new ValidationResult(
                    "Пароль є обов'язковим, якщо вказаний логін.",
                    new[] { nameof(Password) }
                );
            }
            else if(!string.IsNullOrEmpty(Password) && string.IsNullOrEmpty(Login))
            {
                yield return new ValidationResult(
                   "Логін є обов'язковим, якщо вказаний пароль.",
                   new[] { nameof(Login) }
               );
            }
        }
    }
}
