using Insurance_Backend.Enums;

namespace Insurance_Backend.DTO.ResponseDTO
{
    public class ClaimResponseDTO
    {
        public int ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;

        public string PolicyNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public decimal ClaimAmount { get; set; }

        public string ClaimReason { get; set; } = string.Empty;

        public DateTime IncidentDate { get; set; }

        public ClaimStatus ClaimStatus { get; set; }

        public string OfficerRemarks { get; set; } = string.Empty;

        public string AdminRemarks { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
