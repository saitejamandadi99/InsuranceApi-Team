using Insurance_Backend.Enums;

namespace Insurance_Backend.DTO
{
    public class DashboardClaimHistoryDTO
    {
        public int ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public decimal ClaimAmount { get; set; }
        public ClaimStatus ClaimStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}