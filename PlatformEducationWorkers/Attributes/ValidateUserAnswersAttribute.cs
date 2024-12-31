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
                    return new ValidationResult("Each question must have at least one choose answer.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
