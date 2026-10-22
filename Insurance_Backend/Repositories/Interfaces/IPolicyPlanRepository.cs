using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IPolicyPlanRepository
    {
        Task<PolicyPlan> AddPolicyPlan(PolicyPlan policyPlan);

        Task<PolicyPlan?> GetPolicyPlanById(int id);

        Task<PaginatedResponseDTO<PolicyPlan>> GetAllPolicyPlans(
            PaginationQueryDTO query);

        Task<PolicyPlan?> UpdatePolicyPlan(PolicyPlan policyPlan);

        Task<bool> DeletePolicyPlan(int id);

        Task<bool> PolicyPlanExists(int id);

        Task<PolicyPlan?> DeactivatePolicyPlan(int id);
    }
}