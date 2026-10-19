using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.Models
{
    public class InsuranceProduct
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "product name should be between 3 to 100 characters")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Product Type is required")]
        public ProductType ProductType { get; set; }

        [Required(ErrorMessage = "Description is requried")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "description should be between 3 to 200 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Active Status is requried")]
        public ActiveStatus ActiveStatus { get; set; } = ActiveStatus.Active;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<PolicyPlan>? PolicyPlans { get; set; }
    }
}
