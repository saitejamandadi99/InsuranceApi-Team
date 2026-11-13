using Insurance_Backend.Enums;

namespace Insurance_Backend.DTO.ResponseDTO
{
    public class DashboardPolicyResponseDTO
    {
        public int PolicyId { get; set; }

        public string PolicyNumber { get; set; }

        public string CustomerName { get; set; }

        public string ProductName { get; set; }

        public string PlanName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public PolicyStatus PolicyStatus { get; set; }

        public decimal TotalPremiumPaid { get; set; }

        public DateTime CreatedDate { get; set; }

    }
}
