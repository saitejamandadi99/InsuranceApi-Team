using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IClaimStatusHistoryRepository
    {
        Task<PaginatedResponseDTO<ClaimStatusHistory>> GetAllHistory(
        PaginationQueryDTO query);

    Task<ClaimStatusHistory?> GetHistoryById(int id);

        Task<PaginatedResponseDTO<ClaimStatusHistory>> GetHistoryByClaimId(
            int claimId,
            PaginationQueryDTO query);

        Task<PaginatedResponseDTO<ClaimStatusHistory>> GetHistoryByUserId(
            int userId,
            PaginationQueryDTO query);

        Task<ClaimStatusHistory> AddHistory(ClaimStatusHistory history);

        Task<bool> HistoryExists(int id);

        Task<bool> ClaimExists(int claimId);

        Task<bool> UserExists(int userId);
    }
}
