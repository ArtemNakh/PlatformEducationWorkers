using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Models.Questions;
using PlatformEducationWorkers.Models.Questions;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.CourceRequest
{
    public class CreateCourceRequest
    {
        [Required(ErrorMessage = "Назва курсу обов’язкова.")]
        [StringLength(100,MinimumLength =5, ErrorMessage = "Назва курсу має містити від 5 до 100 символів.")]
        public string TitleCource { get; set; }

        [Required(ErrorMessage = "Потрібен опис.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Опис курсу має містити від 5 до 500 символів.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Необхідний зміст курсу.")]
        public string ContentCourse { get; set; } 

        [ValidateQuestions]
        public List<QuestionContext> Questions { get; set; } = new List<QuestionContext>();

        [Required(ErrorMessage = "Потрібно вибрати принаймні одну роль доступу.")]
        public List<int> AccessRoleIds { get; set; }

        

        [Required(ErrorMessage = "Відображати запитання мають бути істинними або невірними.")]
        public bool ShowCorrectAnswers { get; set; }

        [Required(ErrorMessage = "Покажіть відповіді, які мають бути істинними або хибними.")]
        public bool ShowUserAnswers { get; set; }

        [Required(ErrorMessage = "Показати контекст у уривку має бути істинним або хибним.")]
        public bool ShowContextPassage { get; set; }
    }

}
