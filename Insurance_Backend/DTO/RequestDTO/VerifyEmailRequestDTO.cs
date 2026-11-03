using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class VerifyEmailRequestDTO
    {
        [Required(ErrorMessage = "Please enter the Email Address")][EmailAddress(ErrorMessage = "Email address is invalid")] 
        public string Email { get; set; } = string.Empty;

        [Required][RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain exactly six digits.")] 
        public string OTP { get; set; } = string.Empty;

    }
}