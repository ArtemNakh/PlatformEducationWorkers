namespace PlatformEducationWorkers.Models
{
    public class QuestionRequest
    {
        public string Text { get; set; }
        public List<AnswerRequest> Answers { get; set; } = new List<AnswerRequest>();
    }
}
