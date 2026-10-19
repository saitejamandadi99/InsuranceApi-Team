using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class ClaimRequestDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "please select a valid policy")]
        public int PolicyId { get; set; }

        [Required(ErrorMessage = "Claim Amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "claim amount should be greater than 0")]
        public decimal ClaimAmount { get; set; }


        [Required(ErrorMessage = "Claim Reason is required")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Reason should be between 5 and 300 characters")]

        public string ClaimReason { get; set; } = string.Empty;

        public DateTime IncidentDate { get; set; }

        [MinLength(1, ErrorMessage = "At least one supporting document reference is required. ")]
        public List<IFormFile> Files { get; set; } = new();
    }
}
