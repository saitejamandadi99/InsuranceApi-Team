using Insurance_Backend.Custom;
using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    public class ClaimRepository : IClaimRepository
    {
        private readonly AppDbContext _context;

        public ClaimRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reusable projection containing only the fields needed by the service.
        private IQueryable<Claim> ClaimSummaryQuery()
        {
            return _context.Claim
                .AsNoTracking()
                .Select(c => new Claim
                {
                    ClaimId = c.ClaimId,
                    ClaimNumber = c.ClaimNumber,
                    PolicyId = c.PolicyId,
                    ClaimAmount = c.ClaimAmount,
                    ClaimReason = c.ClaimReason,
                    IncidentDate = c.IncidentDate,
                    ClaimStatus = c.ClaimStatus,
                    OfficerRemarks = c.OfficerRemarks,
                    AdminRemarks = c.AdminRemarks,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,

                    Policy = new Policy
                    {
                        PolicyNumber = c.Policy.PolicyNumber,
                        Customer = new CustomerProfile
                        {
                            UserId = c.Policy.Customer.UserId,
                            User = new User
                            {
                                FullName = c.Policy.Customer.User.FullName
                            }
                        }
                    }
                });
        }

        public async Task<Claim> CreateClaim(Claim claim)
        {
            _context.Claim.Add(claim);

            await _context.SaveChangesAsync();

            // Return only the required columns, including PolicyId.
            return await ClaimSummaryQuery()
                .FirstAsync(c => c.ClaimId == claim.ClaimId);
        }

        public async Task<Claim?> GetClaimByClaimNumber(string claimNumber)
        {
            return await ClaimSummaryQuery()
                .FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber);
        }

        public async Task<IEnumerable<Claim>> GetClaimsByCustomerId(
            int customerId)
        {
            return await ClaimSummaryQuery()
                .Where(c => c.Policy.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<Claim?> GetClaimById(int claimId)
        {
            return await _context.Claim.Include(c => c.Policy).ThenInclude(p => p!.Customer).ThenInclude(cu => cu!.User).FirstOrDefaultAsync(c => c.ClaimId == claimId);
       
        }

        public async Task<IEnumerable<Claim>> GetClaimsByPolicyId(int policyId)
        {
            return await ClaimSummaryQuery()
                .Where(c => c.PolicyId == policyId)
                .ToListAsync();
        }

        public async Task<decimal> GetReserveAmountByPolicyId(int policyId)
        {
            return await _context.Claim
                .Where(c =>
                    c.PolicyId == policyId &&
                    (c.ClaimStatus == ClaimStatus.Pending ||
                     c.ClaimStatus == ClaimStatus.RecommendedForApproval ||
                     c.ClaimStatus == ClaimStatus.RecommendedForRejection ||
                     c.ClaimStatus == ClaimStatus.Approved))
                .SumAsync(c => (decimal?)c.ClaimAmount) ?? 0m;
        }

        public async Task<PaginatedResponseDTO<Claim>> ListClaims(
    PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            IQueryable<Claim> claims = _context.Claim
                .AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim().ToLower();
                var statusMatches = Enum.GetValues<ClaimStatus>()
                .Where(status => status.ToString()
                    .Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

                claims = claims.Where(c =>
                    c.ClaimNumber.ToLower().Contains(searchTerm) ||
                    c.Policy.PolicyNumber.ToLower().Contains(searchTerm) ||
                    c.Policy.Customer.User.FullName.ToLower().Contains(searchTerm) ||
                    statusMatches.Contains(c.ClaimStatus)
                );
            }

            // Sorting
            claims = (
                query.SortBy?.ToLowerInvariant(),
                query.SortDirection?.ToLowerInvariant()
            ) switch
            {
                ("claimnumber", "desc") =>
                    claims.OrderByDescending(c => c.ClaimNumber)
                          .ThenBy(c => c.ClaimId),

                ("claimnumber", _) =>
                    claims.OrderBy(c => c.ClaimNumber)
                          .ThenBy(c => c.ClaimId),

                ("policynumber", "desc") =>
                    claims.OrderByDescending(c => c.Policy.PolicyNumber)
                          .ThenBy(c => c.ClaimId),

                ("policynumber", _) =>
                    claims.OrderBy(c => c.Policy.PolicyNumber)
                          .ThenBy(c => c.ClaimId),

                ("customername", "desc") =>
                    claims.OrderByDescending(c => c.Policy.Customer.User.FullName)
                          .ThenBy(c => c.ClaimId),

                ("customername", _) =>
                    claims.OrderBy(c => c.Policy.Customer.User.FullName)
                          .ThenBy(c => c.ClaimId),

                ("claimamount", "desc") =>
                    claims.OrderByDescending(c => c.ClaimAmount)
                          .ThenBy(c => c.ClaimId),

                ("claimamount", _) =>
                    claims.OrderBy(c => c.ClaimAmount)
                          .ThenBy(c => c.ClaimId),

                ("claimstatus", "desc") =>
                    claims.OrderByDescending(c => c.ClaimStatus)
                          .ThenBy(c => c.ClaimId),

                ("claimstatus", _) =>
                    claims.OrderBy(c => c.ClaimStatus)
                          .ThenBy(c => c.ClaimId),

                ("incidentdate", "desc") =>
                    claims.OrderByDescending(c => c.IncidentDate)
                          .ThenBy(c => c.ClaimId),

                ("incidentdate", _) =>
                    claims.OrderBy(c => c.IncidentDate)
                          .ThenBy(c => c.ClaimId),

                ("createddate", "desc") =>
                    claims.OrderByDescending(c => c.CreatedDate)
                          .ThenBy(c => c.ClaimId),

                ("createddate", _) =>
                    claims.OrderBy(c => c.CreatedDate)
                          .ThenBy(c => c.ClaimId),

                _ => claims.OrderBy(c => c.ClaimId)
            };

            // Total Records
            int totalRecords = await claims.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            // Pagination and projection
            var pagedClaims = await claims
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new Claim
                {
                    ClaimId = c.ClaimId,
                    ClaimNumber = c.ClaimNumber,
                    PolicyId = c.PolicyId,
                    ClaimAmount = c.ClaimAmount,
                    ClaimReason = c.ClaimReason,
                    IncidentDate = c.IncidentDate,
                    ClaimStatus = c.ClaimStatus,
                    OfficerRemarks = c.OfficerRemarks,
                    AdminRemarks = c.AdminRemarks,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate,

                    Policy = new Policy
                    {
                        PolicyNumber = c.Policy.PolicyNumber,

                        Customer = new CustomerProfile
                        {
                            User = new User
                            {
                                FullName = c.Policy.Customer.User.FullName
                            }
                        }
                    }
                })
                .ToListAsync();

            return new PaginatedResponseDTO<Claim>
            {
                Records = pagedClaims,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        public async Task UpdateClaim(Claim claim)
        {
            claim.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Claim>> GetClaimsByUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new BusinessException(
                    "Invalid user ID.",
                    StatusCodes.Status400BadRequest);
            }

            return await ClaimSummaryQuery()
                .Where(c => c.Policy.Customer.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .ThenByDescending(c => c.ClaimId)
                .ToListAsync();
        }

        public async Task<string?> GetLatestUpdatedBy(int claimId)
        {
            return await _context.ClaimStatusHistory.Where(h => h.ClaimId == claimId).OrderByDescending(h => h.UpdatedDate).ThenByDescending(h => h.HistoryId).Select(h => h.User.FullName).FirstOrDefaultAsync();
        }

    }
}