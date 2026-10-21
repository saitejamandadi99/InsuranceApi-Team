using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IPolicyRepository
    {
        Task<PolicyResponseDTO?> CreatePolicy(Policy policy);

        Task<PolicyResponseDTO?> GetPolicyByPolicyNumber(string policyNumber);

        Task<PolicyResponseDTO?> CancelPolicy(int policyId);

        Task<PolicyResponseDTO?> GetPolicyById(int policyId);

        Task<IEnumerable<PolicyResponseDTO>> GetPoliciesByCustomerId(int customerId);

        Task<IEnumerable<PolicyResponseDTO>> GetPoliciesByPlanId(int planId);

        Task UpdatePolicy(Policy policy);

        Task SaveChangesAsync();
    }
}
