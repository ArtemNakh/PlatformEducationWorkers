using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.JobTitlesRequest
{
    public class EditJobTitleRequest
    {
        [Required(ErrorMessage = "Job title ID is required.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Job title name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Job title name must be between 3 and 100 characters.")]
        public string Name { get; set; }
    }

}
