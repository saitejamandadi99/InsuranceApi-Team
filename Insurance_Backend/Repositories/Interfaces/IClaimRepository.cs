using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IClaimRepository
    {
        Task<PaginatedResponseDTO<Claim>> ListClaims(PaginationQueryDTO query);

        Task<IEnumerable<Claim>> GetClaimsByPolicyId(int policyId);

        Task<IEnumerable<Claim>> GetClaimsByCustomerId(int customerId);

        Task<Claim?> GetClaimById(int claimId);

        Task<Claim?> GetClaimByClaimNumber(string claimNumber);

        Task<Claim> CreateClaim(Claim claim);

        Task UpdateClaim(Claim claim);

        Task<decimal> GetReserveAmountByPolicyId(int policyId);
    }
}
