using System.Security.Claims;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IClaimStatusHistoryService
    {
        Task<ClaimStatusHistoryResponseDTO> AddClaimStatusHistory(
            ClaimStatusHistory history,
            ClaimsPrincipal user);

        Task<ClaimStatusHistoryResponseDTO?> GetClaimStatusHistoryById(
            int id);

        Task<PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>>
            GetAllClaimStatusHistories(PaginationQueryDTO query);

        Task<PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>>
            GetClaimStatusHistoryByClaimId(
                int claimId,
                PaginationQueryDTO query);

        Task<PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>>
            GetClaimStatusHistoryByUserId(
                int userId,
                PaginationQueryDTO query);

        Task<bool> ClaimStatusHistoryExists(int id);
    }
}