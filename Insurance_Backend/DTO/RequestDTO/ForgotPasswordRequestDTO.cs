using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
        public class ForgotPasswordRequestDTO
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

    }