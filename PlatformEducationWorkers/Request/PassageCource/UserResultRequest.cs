namespace PlatformEducationWorkers.Request.PassageCource
{
    public class UserResultRequest
    {
        public int CourseId { get; set; }
        public List<UserAnswerRequest> Questions { get; set; } = new List<UserAnswerRequest>();
    }
}
