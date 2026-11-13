using Insurance_Backend.DTO.ResponseDTO;

namespace Insurance_Backend.DTO
{
    public class CustomerDashboardResponseDTO
    {
        public int TotalClaimsCount { get; set; }

        public int PendingClaimsCount { get; set; }

        public decimal PendingClaimsAmount { get; set; }

        public IEnumerable<DashboardClaimHistoryDTO> ClaimHistory { get; set; }
            = new List<DashboardClaimHistoryDTO>();
        public IEnumerable<DashboardPolicyResponseDTO> RecentPolicies { get; set; }
           = new List<DashboardPolicyResponseDTO>();

        public IEnumerable<DashboardPaymentResponseDTO> RecentPayments { get; set; }
            = new List<DashboardPaymentResponseDTO>();
    }
}