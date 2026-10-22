namespace Insurance_Backend.DTO
{
    public class CustomerDashboardResponseDTO
    {
        public int TotalClaimsCount { get; set; }

        public int PendingClaimsCount { get; set; }

        public decimal PendingClaimsAmount { get; set; }

        public IEnumerable<DashboardClaimHistoryDTO> ClaimHistory { get; set; }
            = new List<DashboardClaimHistoryDTO>();
    }
}