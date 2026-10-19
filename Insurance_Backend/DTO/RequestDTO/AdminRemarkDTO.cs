using Insurance_Backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class AdminRemarkDTO
    {
        [Range(4, 5, ErrorMessage = "Admin can only approve or reject.")]
        public ClaimStatus ClaimStatus { get; set; }
        [Required]
        [StringLength(300, MinimumLength = 5)]
        public string AdminRemarks { get; set; } = string.Empty;
    }
}
