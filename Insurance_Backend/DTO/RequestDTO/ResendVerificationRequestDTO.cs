using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class ResendVerificationRequestDTO
    {
        [Required][EmailAddress] 
        public string Email { get; set; } = string.Empty;
    }
}
