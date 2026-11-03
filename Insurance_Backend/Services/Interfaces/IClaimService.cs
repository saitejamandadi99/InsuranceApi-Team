using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using System.Security.Claims;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IClaimService
    {
        Task<PaginatedResponseDTO<ClaimResponseDTO>> ListClaims(
    PaginationQueryDTO query);

        Task<IEnumerable<ClaimResponseDTO>> GetClaimsByCustomerId(int customerId);


        Task<IEnumerable<ClaimResponseDTO>> GetClaimsByPolicyId(int policyId);

        Task<IEnumerable<ClaimResponseDTO>> GetClaimsByUserId(int userId);



        Task<ClaimResponseDTO?> GetClaimById(int claimId);


        Task<ClaimResponseDTO> RaiseClaim(ClaimRequestDTO request,

            ClaimsPrincipal user);


        Task<ClaimResponseDTO> OfficerReview(int claimId,

            OfficerRemarkDTO request,

            ClaimsPrincipal user);


        Task<ClaimResponseDTO> AdminReview(int claimId,

            AdminRemarkDTO request,

            ClaimsPrincipal user);


        Task<string> GenerateClaimNumber();

    }
}