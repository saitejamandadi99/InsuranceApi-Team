namespace Insurance_Backend.DTO
{
    public class AdminDashboardResponseDTO
    {
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int PendingClaimsCount { get; set; }

        public decimal PendingClaimsAmount { get; set; }

        public IEnumerable<DashboardClaimDTO> TopClaims { get; set; }
    }
}