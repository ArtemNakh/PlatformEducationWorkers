using System.ComponentModel.DataAnnotations;
using PlatformEducationWorkers.Attributes;
using PlatformEducationWorkers.Models.UserResults;

namespace PlatformEducationWorkers.Request.PassageCource
{
    public class UserResultRequest
    {
        [Required(ErrorMessage = " Потрібен ідентифікатор курсу.")]
        public int CourseId { get; set; }

        [ValidateUserAnswersAttribute]
        public List<UserQuestionRequest> Questions { get; set; } = new List<UserQuestionRequest>();

    }
}
