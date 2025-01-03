﻿using PlatformEducationWorkers.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.AccountRequest
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

        [StringLength(20, MinimumLength = 5, ErrorMessage = "Password must be between 8 and 100 characters.")]
        public string? Password { get; set; }

        [StringLength(20, MinimumLength = 5, ErrorMessage = "Login must be between 5 and 50 characters.")]
        public string? Login { get; set; }

        [Required(ErrorMessage = "Посада є обов'язковою.")]
        public int JobTitleId { get; set; }

        [Required(ErrorMessage = "Роль є обов'язковою.")]
        public Role Role { get; set; }

        public IFormFile? ProfileAvatar { get; set; }
    }
}
