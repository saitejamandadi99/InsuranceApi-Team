using System.ComponentModel.DataAnnotations;

namespace Insurance_Backend.DTO.RequestDTO
{
    public class OfficerOrAdminPolicyIssueRequestDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Customer.")]
        public int CustomerId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid plan.")]
        public int PlanId { get; set; }
    }
}
