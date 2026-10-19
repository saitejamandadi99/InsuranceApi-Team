using Insurance_Backend.Enums;

namespace Insurance_Backend.DTO.ResponseDTO
{
    public class PlanResponseDTO
    {
        public int PlanId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public ProductType ProductType { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal CoverageAmount { get; set; }
        public decimal PremiumAmount { get; set; }
        public PremiumType PremiumType { get; set; }
        public int Duration { get; set; } // in years 
        public string TermsAndConditions { get; set; } = string.Empty;
        public ActiveStatus ActiveStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
