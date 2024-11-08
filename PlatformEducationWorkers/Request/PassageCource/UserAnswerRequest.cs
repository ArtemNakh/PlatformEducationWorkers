namespace PlatformEducationWorkers.Request.PassageCource
{
    public class UserAnswerRequest
    {
        public string Text { get; set; }          // Питання
        public string SelectedAnswer { get; set; } // Відповідь користувача
        public string CorrectAnswer { get; set; }  // Правильна відповідь
        public bool IsCorrect => SelectedAnswer == CorrectAnswer;
    }
}
