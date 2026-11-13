using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class ResetPasswordRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^\d{6}$",
            ErrorMessage = "OTP must contain exactly six digits.")]
        public string OTP { get; set; }

        [Required]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@#$!]).{8,20}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number and special character (@, #, $, !).")]
        public string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword),
            ErrorMessage = "Password and confirm password do not match.")]
        public string ConfirmPassword { get; set; }


    }
}
