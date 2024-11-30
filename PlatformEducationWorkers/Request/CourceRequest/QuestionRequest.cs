using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.CourceRequest
{
    public class QuestionRequest
    {
        [Required(ErrorMessage = "Question text is required.")]
        public string Text { get; set; }

        [Required(ErrorMessage = "At least one answer must be provided.")]
        public List<AnswerRequest> Answers { get; set; } = new List<AnswerRequest>();

       [CustomValidation(typeof(QuestionRequest), "ValidateAnswers")]
        public static ValidationResult ValidateAnswers(List<AnswerRequest> answers, ValidationContext context)
        {
            if (answers == null || !answers.Any())
            {
                return new ValidationResult("At least one answer is required.");
            }

            var hasCorrectAnswer = answers.Any(a => a.IsCorrect == true);
            var hasIncorrectAnswer = answers.Any(a => a.IsCorrect == false);

            if (!hasCorrectAnswer)
            {
                return new ValidationResult("At least one correct answer is required.");
            }

            if (!hasIncorrectAnswer)
            {
                return new ValidationResult("At least one incorrect answer is required.");
            }

            return ValidationResult.Success;
        }

        public class AnswerRequest
        {
            [Required(ErrorMessage = "Answer text is required.")]
            public string Text { get; set; }
            [Required(ErrorMessage = "need be false or true")]
            public bool IsCorrect { get; set; } = false;
        }
    }
}
