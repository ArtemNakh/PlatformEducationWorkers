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
                return new ValidationResult("At least one question is required.");
            }

            foreach (var question in questions)
            {
                if (string.IsNullOrWhiteSpace(question.Text))
                {
                    return new ValidationResult("Each question must have a text.");
                }

                if (question.Answers == null || question.Answers.Count < 2)
                {
                    return new ValidationResult("Each question must have at least two answers.");
                }

                if (!question.Answers.Any(a => a.IsCorrect))
                {
                    return new ValidationResult("Each question must have at least one correct answer.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
