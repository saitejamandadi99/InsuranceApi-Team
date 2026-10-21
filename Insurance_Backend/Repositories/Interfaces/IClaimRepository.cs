using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IClaimRepository
    {
        Task<PaginatedResponseDTO<ClaimResponseDTO>> ListClaims(PaginationQueryDTO query);

        Task<IEnumerable<ClaimResponseDTO>> GetClaimsByPolicyId(int policyId);

        Task<IEnumerable<ClaimResponseDTO>> GetClaimByCustomerId(int customerId);

        Task<ClaimResponseDTO?> GetClaimById(int claimId);

        Task<ClaimResponseDTO?> GetClaimByClaimNumber(string claimNumber);

        Task<ClaimResponseDTO> CreateClaim(Claim claim);

        Task UpdateClaim(Claim claim);

        Task<decimal> GetReserveAmountByPolicyId(int policyId);
    }
}
