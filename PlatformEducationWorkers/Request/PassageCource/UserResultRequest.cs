namespace PlatformEducationWorkers.Request.PassageCource
{
    public class UserResultRequest
    {
        public int CourseId { get; set; }
        public List<UserQuestionRequest> Questions { get; set; } = new List<UserQuestionRequest>();
    }
}
