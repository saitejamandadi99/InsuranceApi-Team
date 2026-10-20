using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insurance_Backend.Models
{
    public class PolicyPlan
    {
        [Key]
        public int PlanId { get; set; }

        [ForeignKey("InsuranceProduct")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Plan Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "plan name should be between 3 to 100 characters")]
        public string PlanName { get; set; }

        [Required(ErrorMessage = "Coverage Amount is required")]
        [Range(1000, double.MaxValue, ErrorMessage = "Coverage Amount must be greater than 1000")]
        public decimal CoverageAmount { get; set; }

        [Required(ErrorMessage = "Premium Amount is required")]
        [Range(1000, double.MaxValue, ErrorMessage = "Premium Amount must be greater than 1000")]

        public decimal PremiumAmount { get; set; }


        [Required(ErrorMessage = "Premium Type is required")]

        public PremiumType PremiumType { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 100, ErrorMessage = "duration should be  between 1 to 100")]
        public int Duration { get; set; } // in years 


        [Required(ErrorMessage = "Terms And Conditions is required")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "TermsAndConditions should be between 3 to 300 characters")]
        public string TermsAndConditions { get; set; }

        [Required(ErrorMessage = "Active Status is required")]
        public ActiveStatus ActiveStatus { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public virtual InsuranceProduct? InsuranceProduct { get; set; }
        public virtual ICollection<Policy>? Policies { get; set; }


    }
}