using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    public class ClaimStatusHistoryRepository : IClaimStatusHistoryRepository
    {
        private readonly AppDbContext _context;

    public ClaimStatusHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reusable Claim Status History Projection
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

                    Claim = h.Claim == null ? null : new Claim
                    {
                        ClaimId = h.Claim.ClaimId,
                        ClaimNumber = h.Claim.ClaimNumber,
                        ClaimStatus = h.Claim.ClaimStatus
                    },

                    User = h.User == null ? null : new User
                    {
                        UserId = h.User.UserId,
                        FullName = h.User.FullName
                    }
                });
        }

        // Get All Claim Status History with Pagination, Search and Sorting
        public async Task<PaginatedResponseDTO<ClaimStatusHistory>>
            GetAllHistory(PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            IQueryable<ClaimStatusHistory> histories = GetHistoryQuery();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim().ToLower();

                histories = histories.Where(h =>
                    (h.Remarks != null &&
                     h.Remarks.ToLower().Contains(searchTerm)) ||
                    (h.Claim != null &&
                     h.Claim.ClaimNumber.ToLower().Contains(searchTerm)) ||
                    (h.User != null &&
                     h.User.FullName.ToLower().Contains(searchTerm)));
            }

            histories = (
                query.SortBy?.ToLowerInvariant(),
                query.SortDirection?.ToLowerInvariant()
            ) switch
            {
                ("historyid", "desc") =>
                    histories.OrderByDescending(h => h.HistoryId),

                ("historyid", _) =>
                    histories.OrderBy(h => h.HistoryId),

                ("claimid", "desc") =>
                    histories.OrderByDescending(h => h.ClaimId)
                             .ThenBy(h => h.HistoryId),

                ("claimid", _) =>
                    histories.OrderBy(h => h.ClaimId)
                             .ThenBy(h => h.HistoryId),

                ("updatedby", "desc") =>
                    histories.OrderByDescending(h => h.UpdatedBy)
                             .ThenBy(h => h.HistoryId),

                ("updatedby", _) =>
                    histories.OrderBy(h => h.UpdatedBy)
                             .ThenBy(h => h.HistoryId),

                ("updateddate", "asc") =>
                    histories.OrderBy(h => h.UpdatedDate)
                             .ThenBy(h => h.HistoryId),

                _ =>
                    histories.OrderByDescending(h => h.UpdatedDate)
                             .ThenBy(h => h.HistoryId)
            };

            int totalRecords = await histories.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var records = await histories
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
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
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
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            IQueryable<ClaimStatusHistory> histories = GetHistoryQuery()
                .Where(h => h.ClaimId == claimId);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim().ToLower();

                histories = histories.Where(h =>
                    h.Remarks != null &&
                    h.Remarks.ToLower().Contains(searchTerm));
            }

            histories = histories
                .OrderByDescending(h => h.UpdatedDate)
                .ThenBy(h => h.HistoryId);

            int totalRecords = await histories.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var records = await histories
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
                IsLastPage = pageNumber >= totalPages,
                SortBy = "UpdatedDate",
                SortDirection = "desc"
            };
        }

        // Get Claim Status History By User ID
        public async Task<PaginatedResponseDTO<ClaimStatusHistory>>
            GetHistoryByUserId(
                int userId,
                PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            IQueryable<ClaimStatusHistory> histories = GetHistoryQuery()
                .Where(h => h.UpdatedBy == userId);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim().ToLower();

                histories = histories.Where(h =>
                    (h.Remarks != null &&
                     h.Remarks.ToLower().Contains(searchTerm)) ||
                    (h.Claim != null &&
                     h.Claim.ClaimNumber.ToLower().Contains(searchTerm)));
            }

            histories = histories
                .OrderByDescending(h => h.UpdatedDate)
                .ThenBy(h => h.HistoryId);

            int totalRecords = await histories.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var records = await histories
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
                IsLastPage = pageNumber >= totalPages,
                SortBy = "UpdatedDate",
                SortDirection = "desc"
            };
        }

        // Add Claim Status History
        public async Task<ClaimStatusHistory> AddHistory(
            ClaimStatusHistory history)
        {
            bool claimExists = await ClaimExists(history.ClaimId);

            if (!claimExists)
            {
                throw new InvalidOperationException(
                    "The specified claim does not exist.");
            }

            bool userExists = await UserExists(history.UpdatedBy);

            if (!userExists)
            {
                throw new InvalidOperationException(
                    "The specified user does not exist.");
            }

            if (history.PreviousStatus == history.NewStatus)
            {
                throw new InvalidOperationException(
                    "Previous status and new status cannot be the same.");
            }

            if (string.IsNullOrWhiteSpace(history.Remarks) ||
                history.Remarks.Trim().Length < 3 ||
                history.Remarks.Trim().Length > 300)
            {
                throw new InvalidOperationException(
                    "Remarks must contain between 3 and 300 characters.");
            }

            history.Remarks = history.Remarks.Trim();
            history.UpdatedDate = DateTime.UtcNow;

            await _context.ClaimStatusHistory.AddAsync(history);
            await _context.SaveChangesAsync();

            return await GetHistoryById(history.HistoryId)
                ?? throw new InvalidOperationException(
                    "Unable to retrieve the created claim status history.");
        }

        // Check Claim Status History Exists
        public async Task<bool> HistoryExists(int id)
        {
            return await _context.ClaimStatusHistory
                .AnyAsync(h => h.HistoryId == id);
        }

        // Check Claim Exists
        public async Task<bool> ClaimExists(int claimId)
        {
            return await _context.Claim
                .AnyAsync(c => c.ClaimId == claimId);
        }

        // Check User Exists
        public async Task<bool> UserExists(int userId)
        {
            return await _context.User
                .AnyAsync(u => u.UserId == userId);
        }
    }
}
