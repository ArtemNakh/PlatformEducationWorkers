using System.ComponentModel.DataAnnotations;

namespace PlatformEducationWorkers.Request.AccountRequest
{
    public class UpdateUserCredentialsRequest
    {
       
        //[StringLength(20, MinimumLength = 0, ErrorMessage = "Login must be between 3 and 50 characters.")]
        public string? NewLogin { get; set; }

       
        //[StringLength(20, MinimumLength = 0, ErrorMessage = "Password must be between 8 and 100 characters.")]
        public string? NewPassword { get; set; }


        public IFormFile? ProfileAvatar { get; set; }
    }
}

