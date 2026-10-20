using Insurance_Backend.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insurance_Backend.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }
        [Required(ErrorMessage = "Claim number is required")]
        public string ClaimNumber { get; set; }

        [ForeignKey("Policy")]
        public int PolicyId { get; set; }
        [Required(ErrorMessage = "Claim Amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "claim amount should be greater than 0")]
        public decimal ClaimAmount { get; set; }
        [Required(ErrorMessage = "Claim Reason is required")]
        [StringLength(300, ErrorMessage = "Reason should be under 300 characters")]

        public string ClaimReason { get; set; }

        [Required(ErrorMessage = "Incident Date is required")]

        public DateTime IncidentDate { get; set; }

        [Required(ErrorMessage = "Claim Status is required")]

        public ClaimStatus ClaimStatus { get; set; } = ClaimStatus.Pending;

        public string? OfficerRemarks { get; set; }

        public string? AdminRemarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public virtual Policy? Policy { get; set; }
        public virtual ICollection<ClaimDocument>? ClaimDocuments { get; set; }
        public virtual ICollection<ClaimStatusHistory>? ClaimStatusHistories { get; set; }
    }
}
