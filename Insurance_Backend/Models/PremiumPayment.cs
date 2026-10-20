using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Insurance_Backend.Enums;
namespace Insurance_Backend.Models
{
    public class PremiumPayment
    {
        [Key]
        public int PaymentId { get; set; }

        [ForeignKey("Policy")]
        public int PolicyId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, double.MaxValue, ErrorMessage = "amount  should be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "PaymentDate is required")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Payment mode is required")]

        public PaymentMode PaymentMode { get; set; }
        [Required(ErrorMessage = "Transaction Reference is required")]
        [StringLength(50)]
        public string TransactionReference { get; set; }

        [Required(ErrorMessage = "Payment Status  is required")]

        public PaymentStatus PaymentStatus { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual Policy? Policy { get; set; }
    }
}
