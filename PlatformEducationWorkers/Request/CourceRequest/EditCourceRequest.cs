using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Models.Questions;
using PlatformEducationWorkers.Models.Questions;
namespace PlatformEducationWorkers.Request.CourceRequest
{
    public class EditCourceRequest
    {
        [Required(ErrorMessage = "Потрібен ідентифікатор курсу.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Необхідно вказати назву курсу.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Назва курсу має містити від 5 до 100 символів.")]
        public string TitleCource { get; set; }

        [Required(ErrorMessage = "Опис курсу обов'язковий.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Опис курсу має містити від 10 до 500 символів.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Необхідний зміст курсу.")]
        public string ContentCourse { get; set; } 

        [Required(ErrorMessage = "Повинні бути потрібні ролі доступу.")]
        public List<int> AccessRoles { get; set; }

        [ValidateQuestions]
        public List<QuestionContext> Questions { get; set; } = new List<QuestionContext>();
    }
}
