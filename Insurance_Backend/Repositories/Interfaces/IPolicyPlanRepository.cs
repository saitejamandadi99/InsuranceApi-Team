using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IPolicyPlanRepository
    {
        // Add Policy Plan
        Task<PolicyPlan> AddPolicyPlan(PolicyPlan policyPlan);

        // Get Policy Plan By ID
        Task<PolicyPlan?> GetPolicyPlanById(int id);

        // Get All Policy Plans with filtering, sorting and pagination
        Task<PaginatedResponseDTO<PolicyPlan>> GetAllPolicyPlans(
            PaginationQueryDTO query);

        // Update Policy Plan
        Task<PolicyPlan?> UpdatePolicyPlan(PolicyPlan policyPlan);

        Task<PaginatedResponseDTO<PolicyPlan>> GetPlansByProductId(

    int productId,

    PaginationQueryDTO query);

        Task<PolicyPlan?> ActivatePolicyPlan(int id);


        // Deactivate Policy Plan
        Task<PolicyPlan?> DeactivatePolicyPlan(int id);

        // Check whether Policy Plan exists
        Task<bool> PolicyPlanExists(int id);

        // Check whether an Insurance Product exists and is active
        Task<bool> IsProductActive(int productId);
    }
}
