using AutoMapper;
using Insurance_Backend.Custom;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Insurance_Backend.Services.Classes
{
    public class ClaimStatusHistoryService : IClaimStatusHistoryService
    {
        private readonly IClaimStatusHistoryRepository _historyRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<ClaimStatusHistoryService> _logger;

        public ClaimStatusHistoryService(
            IClaimStatusHistoryRepository historyRepo,
            IMapper mapper,
            ILogger<ClaimStatusHistoryService> logger)
        {
            _historyRepo = historyRepo;
            _mapper = mapper;
            _logger = logger;
        }

        // Get all claim status histories
        public async Task<PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>>
            GetAllHistory(PaginationQueryDTO query)
        {
            ValidatePagination(query);

            var result = await _historyRepo.GetAllHistory(query);

            return MapPaginatedResponse(result);
        }

        // Get claim status history by ID
        public async Task<ClaimStatusHistoryResponseDTO?> GetHistoryById(int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid history ID.",
                    StatusCodes.Status400BadRequest);
            }

            var history = await _historyRepo.GetHistoryById(id);

            if (history == null)
            {
                throw new BusinessException(
                    "Claim status history not found.",
                    StatusCodes.Status404NotFound);
            }

            return _mapper.Map<ClaimStatusHistoryResponseDTO>(history);
        }

        // Get claim status history by claim ID
        public async Task<PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>>
            GetHistoryByClaimId(
                int claimId,
                PaginationQueryDTO query)
        {
            if (claimId <= 0)
            {
                throw new BusinessException(
                    "Invalid claim ID.",
                    StatusCodes.Status400BadRequest);
            }

            ValidatePagination(query);

            if (!await _historyRepo.ClaimExists(claimId))
            {
                throw new BusinessException(
                    "Claim not found.",
                    StatusCodes.Status404NotFound);
            }

            var result = await _historyRepo.GetHistoryByClaimId(
                claimId,
                query);

            return MapPaginatedResponse(result);
        }

        // Get claim status history by user ID
        public async Task<PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>>
            GetHistoryByUserId(
                int userId,
                PaginationQueryDTO query)
        {
            if (userId <= 0)
            {
                throw new BusinessException(
                    "Invalid user ID.",
                    StatusCodes.Status400BadRequest);
            }

            ValidatePagination(query);

            if (!await _historyRepo.UserExists(userId))
            {
                throw new BusinessException(
                    "User not found.",
                    StatusCodes.Status404NotFound);
            }

            var result = await _historyRepo.GetHistoryByUserId(
                userId,
                query);

            return MapPaginatedResponse(result);
        }

        // Add claim status history
        public async Task<ClaimStatusHistoryResponseDTO> AddHistory(
            ClaimStatusHistory history)
        {
            if (history == null)
            {
                throw new BusinessException(
                    "Claim status history is required.",
                    StatusCodes.Status400BadRequest);
            }

            if (history.ClaimId <= 0)
            {
                throw new BusinessException(
                    "A valid claim ID is required.",
                    StatusCodes.Status400BadRequest);
            }

            if (history.UpdatedBy <= 0)
            {
                throw new BusinessException(
                    "A valid updater ID is required.",
                    StatusCodes.Status400BadRequest);
            }

            if (!Enum.IsDefined(typeof(ClaimStatus), history.PreviousStatus) ||
                !Enum.IsDefined(typeof(ClaimStatus), history.NewStatus))
            {
                throw new BusinessException(
                    "Invalid claim status.",
                    StatusCodes.Status400BadRequest);
            }

            if (history.PreviousStatus == history.NewStatus)
            {
                throw new BusinessException(
                    "Previous status and new status cannot be the same.",
                    StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrWhiteSpace(history.Remarks) ||
                history.Remarks.Trim().Length < 3 ||
                history.Remarks.Trim().Length > 300)
            {
                throw new BusinessException(
                    "Remarks must contain between 3 and 300 characters.",
                    StatusCodes.Status400BadRequest);
            }

            if (!await _historyRepo.ClaimExists(history.ClaimId))
            {
                throw new BusinessException(
                    "Claim not found.",
                    StatusCodes.Status404NotFound);
            }

            if (!await _historyRepo.UserExists(history.UpdatedBy))
            {
                throw new BusinessException(
                    "User not found.",
                    StatusCodes.Status404NotFound);
            }

            history.HistoryId = 0;
            history.Remarks = history.Remarks.Trim();
            history.UpdatedDate = DateTime.UtcNow;

            var result = await _historyRepo.AddHistory(history);

            _logger.LogInformation(
                "Claim status history created. HistoryId: {HistoryId}, ClaimId: {ClaimId}, UpdatedBy: {UpdatedBy}",
                result.HistoryId,
                result.ClaimId,
                result.UpdatedBy);

            return _mapper.Map<ClaimStatusHistoryResponseDTO>(result);
        }

        // Check whether history exists
        public async Task<bool> HistoryExists(int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid history ID.",
                    StatusCodes.Status400BadRequest);
            }

            return await _historyRepo.HistoryExists(id);
        }

        // Validate pagination parameters
        private static void ValidatePagination(PaginationQueryDTO query)
        {
            if (query == null)
            {
                throw new BusinessException(
                    "Pagination query is required.",
                    StatusCodes.Status400BadRequest);
            }

            if (query.PageNumber < 1)
            {
                throw new BusinessException(
                    "Page number must be greater than zero.",
                    StatusCodes.Status400BadRequest);
            }

            if (query.PageSize < 1 || query.PageSize > 100)
            {
                throw new BusinessException(
                    "Page size must be between 1 and 100.",
                    StatusCodes.Status400BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(query.SortDirection) &&
                !query.SortDirection.Equals(
                    "asc",
                    StringComparison.OrdinalIgnoreCase) &&
                !query.SortDirection.Equals(
                    "desc",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException(
                    "Sort direction must be 'asc' or 'desc'.",
                    StatusCodes.Status400BadRequest);
            }

            string[] allowedSortFields =
            {
                "historyid",
                "claimid",
                "previousstatus",
                "newstatus",
                "updatedby",
                "updateddate"
            };

            if (!string.IsNullOrWhiteSpace(query.SortBy) &&
                !allowedSortFields.Contains(
                    query.SortBy.Trim(),
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new BusinessException(
                    "Invalid sort field.",
                    StatusCodes.Status400BadRequest);
            }
        }

        // Map paginated repository results to response DTOs
        private PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>
            MapPaginatedResponse(
                PaginatedResponseDTO<ClaimStatusHistory> result)
        {
            return new PaginatedResponseDTO<ClaimStatusHistoryResponseDTO>
            {
                Records = _mapper.Map<
                    IEnumerable<ClaimStatusHistoryResponseDTO>>(result.Records),

                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = result.TotalPages,
                IsLastPage = result.IsLastPage,
                SortBy = result.SortBy,
                SortDirection = result.SortDirection
            };
        }
    }
}
