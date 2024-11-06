namespace PlatformEducationWorkers.Request
{
    public class CreateCourceRequest
    {
        public string TitleCource { get; set; }
        public string Description { get; set; }
        public string ContentCourse { get; set; } // JSON format for course content
        public string Questions { get; set; } // JSON format for questions and answers
        public int EnterpriseId { get; set; }
        public List<int> AccessRoleIds { get; set; } // List of JobTitle IDs for access roles
    }
}
