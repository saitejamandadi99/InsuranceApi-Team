using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;


namespace Insurance_Backend.Models
{
    public class ClaimDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [ForeignKey("Claim")]
        public int ClaimId { get; set; }
        [Required(ErrorMessage = "Document Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Document Name should be between 3 and 100 characters")]
        public string DocumentName { get; set; }

        [Required(ErrorMessage = "Document Type is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Document Type should be between 3 and 50 characters")]
        public string DocumentType { get; set; }

        [Required(ErrorMessage = "Document Reference is required")]
        [StringLength(300)]
        public string DocumentReference { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.Now;

        public virtual Claim? Claim { get; set; }

    }
}
