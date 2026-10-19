using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class CustomerPolicyPurchaseRequestDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid plan.")]
        public int PlanId { get; set; }
    }
}
