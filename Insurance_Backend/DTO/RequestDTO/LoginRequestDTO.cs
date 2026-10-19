using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email address is invalid")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "password must be between 8 and 20 characters")]
        public string Password { get; set; } = string.Empty;

    }
}
