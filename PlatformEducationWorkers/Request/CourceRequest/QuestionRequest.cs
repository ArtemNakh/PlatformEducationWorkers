using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.CourceRequest
{
    public class QuestionRequest
    {
        [Required(ErrorMessage = "Необхідний текст запитання.")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Треба дати хоча б одну відповідь.")]
        public List<AnswerRequest> Answers { get; set; } = new List<AnswerRequest>();

       [CustomValidation(typeof(QuestionRequest), "ValidateAnswers")]
        public static ValidationResult ValidateAnswers(List<AnswerRequest> answers, ValidationContext context)
        {
            if (answers == null || !answers.Any())
            {
                return new ValidationResult("Потрібна принаймні одна відповідь.");
            }

            var hasCorrectAnswer = answers.Any(a => a.IsCorrect == true);
            var hasIncorrectAnswer = answers.Any(a => a.IsCorrect == false);

            if (!hasCorrectAnswer)
            {
                return new ValidationResult("Потрібна хоча б одна правильна відповідь.");
            }

            if (!hasIncorrectAnswer)
            {
                return new ValidationResult("Потрібна принаймні одна неправильна відповідь.");
            }

            return ValidationResult.Success;
        }

        public class AnswerRequest
        {
            [Required(ErrorMessage = "Необхідно ввести текст відповіді.")]
            public string Text { get; set; }
            [Required(ErrorMessage = "повинно бути хибним або істинним")]
            public bool IsCorrect { get; set; } = false;
        }
    }
}
