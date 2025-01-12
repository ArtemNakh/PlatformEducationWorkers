using PlatformEducationWorkers.Models.Questions;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Attributes
{
    public class ValidateQuestionsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var questions = value as List<QuestionContext>;
            if (questions == null || questions.Count == 0)
            {
                return new ValidationResult("Повинно бути хоча б одне питання.");
            }

            foreach (var question in questions)
            {
                if (string.IsNullOrWhiteSpace(question.Text))
                {
                    return new ValidationResult("Кожне питання повинно мати текст запитання.");
                }

                if (question.Answers == null || question.Answers.Count < 2)
                {
                    return new ValidationResult("Кожне питання повинно мати хоча б 2 відповіді.");
                }

                if (!question.Answers.Any(a => a.IsCorrect))
                {
                    return new ValidationResult("Кожне питання повинно мати хоча б одну вірну відповідь.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
