using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PlatformEducationWorkers.Core.Models;
using PlatformEducationWorkers.Models.Questions;
namespace PlatformEducationWorkers.Request.CourceRequest
{
    public class EditCourceRequest
    {
        [Required(ErrorMessage = "Course ID is required.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Course title is required.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Course title must be between 5 and 100 characters.")]
        public string TitleCource { get; set; }

        [Required(ErrorMessage = "Course description is required.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Course description must be between 10 and 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Course content is required.")]
        public string ContentCourse { get; set; } // JSON рядок із вмістом курсу

        [Required(ErrorMessage = "Access Roles must be required.")]
        public List<int> AccessRoles { get; set; }

        [Required(ErrorMessage = "Questions are required.")]
        [MinLength(1, ErrorMessage = "At least one question is required.")]
        public List<QuestionContext> Questions { get; set; } = new List<QuestionContext>();
    }
}
