using Insurance_Backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class OfficerRemarkDTO
    {
        [Range(2, 3, ErrorMessage = "officer can only do recommend approve or recommend reject.")]
        public ClaimStatus ClaimStatus { get; set; }

        [Required]
        [StringLength(300, MinimumLength = 5)]
        public string OfficerRemarks { get; set; } = string.Empty;
    }
}
