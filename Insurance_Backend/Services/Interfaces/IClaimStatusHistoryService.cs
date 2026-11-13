using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IClaimStatusHistoryService
    {
        Task<PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>>
        GetAllHistory(PaginationQueryDTO query);

    Task<ClaimStatusHistoryResponseDTO?> GetHistoryById(int id);

        Task<PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>>
            GetHistoryByClaimId(
                int claimId,
                PaginationQueryDTO query);

        Task<PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>>
            GetHistoryByUserId(
                int userId,
                PaginationQueryDTO query);

        Task<ClaimStatusHistoryResponseDTO> AddHistory(ClaimStatusHistory history);

        Task<PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>>
            GetMyClaimHistory(
            int userId,
            PaginationQueryDTO query);
        Task<bool> HistoryExists(int id);
    }

}
