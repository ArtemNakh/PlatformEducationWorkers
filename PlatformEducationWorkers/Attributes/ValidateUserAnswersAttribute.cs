using PlatformEducationWorkers.Models.UserResults;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Attributes
{
    public class ValidateUserAnswersAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var questions = value as List<UserQuestionRequest>;
         
            foreach (var question in questions)
            {
                if (!question.Answers.Any(a => a.IsSelected))
                {
                    return new ValidationResult("Кожне питання повинно містити хоча б одну обрану відповідь.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
