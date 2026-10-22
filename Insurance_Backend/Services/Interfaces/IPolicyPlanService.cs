using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IPolicyPlanService
    {
        Task<PlanResponseDTO> AddPolicyPlan(
            PlanRequestDTO request);

        Task<PlanResponseDTO?> GetPolicyPlanById(int id);

        Task<PaginatedResponseDTO<PlanResponseDTO>>
            GetAllPolicyPlans(PaginationQueryDTO query);

        Task<PlanResponseDTO?> UpdatePolicyPlan(
            int id,
            PlanRequestDTO request);

        Task<bool> DeletePolicyPlan(int id);

        Task<PlanResponseDTO?> DeactivatePolicyPlan(int id);
    }
}