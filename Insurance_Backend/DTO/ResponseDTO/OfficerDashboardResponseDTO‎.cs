namespace Insurance_Backend.DTO
{
    public class OfficerDashboardResponseDTO
    {
        public int PendingClaimsCount { get; set; }
        public decimal PendingClaimsAmount { get; set; }

        public IEnumerable<DashboardClaimDTO> TopClaims { get; set; }
    }
}