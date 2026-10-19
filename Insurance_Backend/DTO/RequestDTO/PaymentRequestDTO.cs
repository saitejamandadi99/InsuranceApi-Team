using Insurance_Backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class PaymentRequestDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Policy.")]
        public int PolicyId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "amount  should be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment mode is required")]
        public PaymentMode PaymentMode { get; set; }
    }
}
