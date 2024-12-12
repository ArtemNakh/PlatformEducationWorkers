namespace PlatformEducationWorkers.Request.PassageCource
{
    public class UserQuestionRequest
    {
        public string Text { get; set; } // Текст питання
        public List<UserAnswerRequest> Answers { get; set; } = new List<UserAnswerRequest>(); // Список відповідей
        public string? PhotoQuestionBase64 { get; set; }
    }
}
