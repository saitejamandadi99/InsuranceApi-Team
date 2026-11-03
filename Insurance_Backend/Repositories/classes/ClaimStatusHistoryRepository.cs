using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    /// <summary>
    /// Data access only.
    /// Claim status history is append-only.
    /// Business validation and authorization belong in the service layer.
    /// </summary>
    public class ClaimStatusHistoryRepository : IClaimStatusHistoryRepository
    {
        private readonly AppDbContext _context;

        public ClaimStatusHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        // Base query with Claim and User details
        private IQueryable<ClaimStatusHistory> GetHistoryQuery()
        {
            return _context.ClaimStatusHistory
                .AsNoTracking()
                .Select(h => new ClaimStatusHistory
                {
                    HistoryId = h.HistoryId,
                    ClaimId = h.ClaimId,
                    PreviousStatus = h.PreviousStatus,
                    NewStatus = h.NewStatus,
                    Remarks = h.Remarks,
                    UpdatedBy = h.UpdatedBy,
                    UpdatedDate = h.UpdatedDate,

                    Claim = h.Claim == null
                        ? null
                        : new Claim
                        {
                            ClaimId = h.Claim.ClaimId,
                            ClaimNumber = h.Claim.ClaimNumber,
                            ClaimStatus = h.Claim.ClaimStatus
                        },

                    User = h.User == null
                        ? null
                        : new User
                        {
                            UserId = h.User.UserId,
                            FullName = h.User.FullName
                        }
                });
        }

        // Get All Claim Status History
        public async Task<PaginatedResponseDTO<ClaimStatusHistory>>
            GetAllHistory(PaginationQueryDTO query)
        {
            IQueryable<ClaimStatusHistory> histories = GetHistoryQuery();

            // Search by remarks, claim number, or updater name
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim();

                histories = histories.Where(h =>
                    h.Remarks.Contains(searchTerm) ||
                    (h.Claim != null &&
                     h.Claim.ClaimNumber.Contains(searchTerm)) ||
                    (h.User != null &&
                     h.User.FullName.Contains(searchTerm)));
            }

            return await GetPaginatedHistory(histories, query);
        }

        // Get Claim Status History By ID
        public async Task<ClaimStatusHistory?> GetHistoryById(int id)
        {
            return await GetHistoryQuery()
                .FirstOrDefaultAsync(h => h.HistoryId == id);
        }

        // Get Claim Status History By Claim ID
        public async Task<PaginatedResponseDTO<ClaimStatusHistory>>
            GetHistoryByClaimId(
                int claimId,
                PaginationQueryDTO query)
        {
            IQueryable<ClaimStatusHistory> histories = GetHistoryQuery()
                .Where(h => h.ClaimId == claimId);

            return await GetPaginatedHistory(histories, query);
        }

        // Get Claim Status History By User ID
        public async Task<PaginatedResponseDTO<ClaimStatusHistory>>
            GetHistoryByUserId(
                int userId,
                PaginationQueryDTO query)
        {
            IQueryable<ClaimStatusHistory> histories = GetHistoryQuery()
                .Where(h => h.UpdatedBy == userId);

            return await GetPaginatedHistory(histories, query);
        }

        // Add Claim Status History and save immediately
        public async Task<ClaimStatusHistory> AddHistory(
            ClaimStatusHistory history)
        {
            history.UpdatedDate = DateTime.UtcNow;

            await _context.ClaimStatusHistory.AddAsync(history);
            await _context.SaveChangesAsync();

            return await GetHistoryById(history.HistoryId)
                ?? throw new InvalidOperationException(
                    "Claim status history was saved but could not be retrieved.");
        }

        // Stage history without saving
        // Allows the service to save the claim update and history together.
        public void StageHistory(ClaimStatusHistory history)
        {
            history.UpdatedDate = DateTime.UtcNow;

            _context.ClaimStatusHistory.Add(history);
        }

        // Check Claim Status History Exists
        public async Task<bool> HistoryExists(int id)
        {
            return await _context.ClaimStatusHistory
                .AsNoTracking()
                .AnyAsync(h => h.HistoryId == id);
        }

        // Check Claim Exists
        public async Task<bool> ClaimExists(int claimId)
        {
            return await _context.Claim
                .AsNoTracking()
                .AnyAsync(c => c.ClaimId == claimId);
        }

        // Check User Exists
        public async Task<bool> UserExists(int userId)
        {
            return await _context.User
                .AsNoTracking()
                .AnyAsync(u => u.UserId == userId);
        }

        public async Task<Dictionary<int, string?>>
    GetLatestUpdatedByForClaims(IEnumerable<int> claimIds)
        {
            var ids = claimIds.Distinct().ToList();

            if (ids.Count == 0)
            {
                return new Dictionary<int, string?>();
            }

            var result = await _context.ClaimStatusHistory
                .AsNoTracking()
                .Where(h => ids.Contains(h.ClaimId))
                .GroupBy(h => h.ClaimId)
                .Select(g => new
                {
                    ClaimId = g.Key,
                    UpdatedByName = g
                        .OrderByDescending(h => h.UpdatedDate)
                        .ThenByDescending(h => h.HistoryId)
                        .Select(h => h.User.FullName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return result.ToDictionary(
                x => x.ClaimId,
                x => x.UpdatedByName);
        }

        // Get complete Claim Status History
        public async Task<IEnumerable<ClaimStatusHistory>> GetCompleteHistoryByClaimId(int claimId)
        {
            return await GetHistoryQuery()
                .Where(h => h.ClaimId == claimId)
                .OrderBy(h => h.UpdatedDate)
                .ThenBy(h => h.HistoryId)
                .ToListAsync();
        }

        // Get all history records for claims owned by a customer
        public async Task<PaginatedResponseDTO<ClaimStatusHistory>> GetMyClaimHistory(
            int userId,
            PaginationQueryDTO query)
        {
            // First, find the claims belonging to this customer.
            var customerClaimIds = _context.Claim
                .AsNoTracking()
                .Where(c => c.Policy.Customer.UserId == userId)
                .Select(c => c.ClaimId);

            // Then, retrieve all history records for those claims.
            IQueryable<ClaimStatusHistory> histories = GetHistoryQuery()
                .Where(h => customerClaimIds.Contains(h.ClaimId));

            return await GetPaginatedHistory(histories, query);
        }


        // Shared database query for sorting and pagination
        private async Task<PaginatedResponseDTO<ClaimStatusHistory>>
            GetPaginatedHistory(
                IQueryable<ClaimStatusHistory> histories,
                PaginationQueryDTO query)
        {
            int pageNumber = query.PageNumber;
            int pageSize = query.PageSize;

            string sortBy = string.IsNullOrWhiteSpace(query.SortBy)
                ? "updateddate"
                : query.SortBy.Trim().ToLowerInvariant();

            string sortDirection =
                string.Equals(
                    query.SortDirection?.Trim(),
                    "asc",
                    StringComparison.OrdinalIgnoreCase)
                    ? "asc"
                    : "desc";

            bool descending = sortDirection == "desc";

            IOrderedQueryable<ClaimStatusHistory> orderedHistories =
                sortBy switch
                {
                    "historyid" => descending
                        ? histories.OrderByDescending(h => h.HistoryId)
                        : histories.OrderBy(h => h.HistoryId),

                    "claimid" => descending
                        ? histories.OrderByDescending(h => h.ClaimId)
                        : histories.OrderBy(h => h.ClaimId),

                    "previousstatus" => descending
                        ? histories.OrderByDescending(h => h.PreviousStatus)
                        : histories.OrderBy(h => h.PreviousStatus),

                    "newstatus" => descending
                        ? histories.OrderByDescending(h => h.NewStatus)
                        : histories.OrderBy(h => h.NewStatus),

                    "updatedby" => descending
                        ? histories.OrderByDescending(h => h.UpdatedBy)
                        : histories.OrderBy(h => h.UpdatedBy),

                    "updateddate" => descending
                        ? histories.OrderByDescending(h => h.UpdatedDate)
                        : histories.OrderBy(h => h.UpdatedDate),

                    _ => descending
                        ? histories.OrderByDescending(h => h.UpdatedDate)
                        : histories.OrderBy(h => h.UpdatedDate)
                };

            // Stable ordering for pagination
            orderedHistories = orderedHistories
                .ThenBy(h => h.HistoryId);

            int totalRecords = await orderedHistories.CountAsync();

            int totalPages = pageSize > 0
                ? (int)Math.Ceiling(
                    totalRecords / (double)pageSize)
                : 0;

            List<ClaimStatusHistory> records = await orderedHistories
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<ClaimStatusHistory>
            {
                Records = records,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = totalPages == 0 ||
                             pageNumber >= totalPages,
                SortBy = sortBy,
                SortDirection = sortDirection
            };
        }
    }
}
