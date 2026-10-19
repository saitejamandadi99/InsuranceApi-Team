using Insurance_Backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class PlanRequestDTO
    {
        [Required(ErrorMessage = "Product Id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid product.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Plan Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "plan name should be between 3 to 100 characters")]
        public string PlanName { get; set; } = string.Empty;

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
        public string TermsAndConditions { get; set; } = string.Empty;
    }
}
