using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using System.Security.Claims;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IPolicyService
    {
        Task<PolicyResponseDTO> PurchasePolicy(CustomerPolicyPurchaseRequestDTO request, ClaimsPrincipal user);
        Task<PolicyResponseDTO> IssuePolicy(OfficerOrAdminPolicyIssueRequestDTO request);
        Task<PolicyResponseDTO?> GetPolicyById(int policyId);
        Task<IEnumerable<PolicyResponseDTO>> GetPoliciesByCustomerId(int customerId);
        Task<IEnumerable<PolicyResponseDTO>> GetPoliciesByPlanId(int planId);
        Task<PaginatedResponseDTO<PolicyResponseDTO>> ListPolicies(PaginationQueryDTO query);
        Task<PolicyResponseDTO?> CancelPolicy(int policyId);
        Task<string> GeneratePolicyNumber();
    }
}
