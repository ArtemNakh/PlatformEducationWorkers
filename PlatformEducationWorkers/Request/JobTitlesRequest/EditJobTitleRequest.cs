using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.JobTitlesRequest
{
    public class EditJobTitleRequest
    {
        [Required(ErrorMessage = "Потрібно вказати ідентифікатор посади.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Необхідно вказати назву посади.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Назва посади має містити від 3 до 100 символів.")]
        public string Name { get; set; }
    }

}
