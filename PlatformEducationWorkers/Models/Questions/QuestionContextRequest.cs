namespace PlatformEducationWorkers.Models.Questions
{
    public class QuestionContextRequest
    {
        public string Text { get; set; }
        public List<AnswerContextRequest> Answers { get; set; } = new List<AnswerContextRequest>();
    }
}
