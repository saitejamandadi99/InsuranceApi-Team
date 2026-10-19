using Insurance_Backend.Enums;
using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class ProductRequestDTO
    {
        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "product name should be between 3 to 100 characters")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product Type is required")]
        public ProductType ProductType { get; set; }

        [Required(ErrorMessage = "Description is requried")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "description should be between 3 to 200 characters")]
        public string Description { get; set; } = string.Empty;

    }
}
