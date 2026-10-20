using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insurance_Backend.Models
{
    public class CustomerProfile
    {
        [Key]
        public int CustomerId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual User? User { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateOnly DateOfBirth { get; set; }
        [Required(ErrorMessage = "Address is required")]
        [StringLength(150, MinimumLength = 10, ErrorMessage = "Address should be written between 10 to 150 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "City should be written between 3 to 30 characters")]
        public string City { get; set; }


        [Required(ErrorMessage = "state is required")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "state should be written between 3 to 30 characters")]
        public string State { get; set; }

        [Required(ErrorMessage = "Pincode is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must contain exactly 6 digits")]
        public string Pincode { get; set; }

        [Required(ErrorMessage = "NomineeName is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "NomineeName should be written between 3 to 50 characters")]
        public string NomineeName { get; set; }

        [Required(ErrorMessage = "NomineeRelationship is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "NomineeRelationship should be written between 3 to 50 characters")]
        public string NomineeRelationship { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public ICollection<Policy>? Policies { get; set; }

        public string? ProfileImageUrl { get; set; }


    }
}
