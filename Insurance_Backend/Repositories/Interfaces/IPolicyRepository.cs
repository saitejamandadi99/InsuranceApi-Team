using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IPolicyRepository
    {
        Task<Policy> CreatePolicy(Policy policy);

        Task<Policy?> CancelPolicy(int policyId);

        Task<IEnumerable<Policy>> GetPoliciesByCustomerId(int customerId);

        Task<IEnumerable<Policy>> GetPoliciesByPlanId(int planId);

        Task<Policy?> GetPolicyById(int policyId);

        Task<Policy?> GetPolicyByPolicyNumber(string policyNumber);

        Task<PaginatedResponseDTO<Policy>> ListPolicies(
            PaginationQueryDTO query);

        Task UpdatePolicy(Policy policy);

        Task SaveChangesAsync();
    }
}
