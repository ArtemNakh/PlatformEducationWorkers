using Serilog;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.AccountRequest
{
    public class UpdateUserCredentialsRequest: IValidatableObject
    {

        [MinLength(6, ErrorMessage = "Логін повинен містити як мінімум 6 літер.")]
        public string? NewLogin { get; set; }

        [MinLength(6, ErrorMessage = "Пароль повинен містити як мінімум 6 літер.")]
        public string? NewPassword { get; set; }

        public IFormFile? ProfileAvatar { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(NewLogin) && string.IsNullOrEmpty(NewPassword))
            {
                yield return new ValidationResult(
                    "Пароль є обов'язковим, якщо вказаний логін.",
                    new[] { nameof(NewPassword) }
                );
            }
            else if (!string.IsNullOrEmpty(NewPassword) && string.IsNullOrEmpty(NewLogin))
            {
                yield return new ValidationResult(
                   "Логін є обов'язковим, якщо вказаний пароль.",
                   new[] { nameof(NewLogin) }
               );
            }
        }
    }
}

