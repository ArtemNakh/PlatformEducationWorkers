using PlatformEducationWorkers.Models.Questions;
using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request
{
    public class CreateCourceRequest
    {
        [Required(ErrorMessage = "Title of the course is required.")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string TitleCource { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Course content is required.")]
        public string ContentCourse { get; set; } // JSON format for course content

        [Required(ErrorMessage = "At least one question is required.")]
        public List<QuestionRequest> Questions { get; set; } = new List<QuestionRequest>();

        [Required(ErrorMessage = "At least one access role must be selected.")]
        public List<int> AccessRoleIds { get; set; } // List of JobTitle IDs for access roles
    }

}
