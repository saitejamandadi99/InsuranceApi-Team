using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using Insurance_Backend.Enums;
namespace Insurance_Backend.Models
{
    public class ClaimStatusHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [ForeignKey("Claim")]
        public int ClaimId { get; set; }

        public ClaimStatus PreviousStatus { get; set; }

        public ClaimStatus NewStatus { get; set; }

        [Required(ErrorMessage = "Remarks are required")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "Remarks should be between 3 and 300 characters")]
        public string Remarks { get; set; }

        [Required(ErrorMessage = "Updated By is required.")]
        public int UpdatedBy { get; set; }

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public virtual Claim? Claim { get; set; }
        [ForeignKey(nameof(UpdatedBy))]
        public virtual User? User { get; set; }  //FOREIGN KEY (UpdatedBy) REFERENCES User(UserId)

    }
}