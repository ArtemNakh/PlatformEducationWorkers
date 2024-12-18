using System.ComponentModel.DataAnnotations;
using PlatformEducationWorkers.Models.Results;

namespace PlatformEducationWorkers.Request.PassageCource
{
    public class UserResultRequest
    {
        [Required(ErrorMessage = " Course ID is required.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "At least one question is required.")]
        public List<UserQuestionRequest> Questions { get; set; } = new List<UserQuestionRequest>();

    }
}
