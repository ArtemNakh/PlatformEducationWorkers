using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.Login_RegisterRequest
{
    public class RegisterCompanyRequest
    {
        [Required(ErrorMessage = "Назва компанії є обов'язковою")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Електронна пошта є обов'язковою")]
        [EmailAddress(ErrorMessage = "Некоректний формат електронної пошти")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Ім'я власника є обов'язковим")]
        public string OwnerName { get; set; }

        [Required(ErrorMessage = "Прізвище власника є обов'язковим")]
        public string OwnerSurname { get; set; }

        [Required(ErrorMessage = "Дата народження є обов'язковою")]
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }

        [Required(ErrorMessage = "Логін є обов'язковим")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [MinLength(6, ErrorMessage = "Пароль має містити щонайменше 6 символів")]
        public string Password { get; set; }

        public IFormFile? ProfileAvatar {  get; set; }
    }

}
