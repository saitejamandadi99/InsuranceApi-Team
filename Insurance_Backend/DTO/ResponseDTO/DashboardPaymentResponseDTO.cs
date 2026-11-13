using Insurance_Backend.Enums;

namespace Insurance_Backend.DTO.ResponseDTO
{
    public class DashboardPaymentResponseDTO
    {
        public int PaymentId { get; set; }

        public int PolicyId { get; set; }

        public string PolicyNumber { get; set; }

        public string CustomerName { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public PaymentMode PaymentMode { get; set; }

        public string TransactionReference { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

    }
}
