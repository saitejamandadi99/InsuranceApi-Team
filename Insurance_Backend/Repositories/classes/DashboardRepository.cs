using Insurance_Backend.Data;
using Insurance_Backend.DTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        // Admin Dashboard
        public async Task<AdminDashboardResponseDTO>
            GetAdminDashboardAsync()
        {
            int activeUsers = await _context.User
                .AsNoTracking()
                .CountAsync(u =>
                    u.ActiveStatus == ActiveStatus.Active);

            int inactiveUsers = await _context.User
                .AsNoTracking()
                .CountAsync(u =>
                    u.ActiveStatus == ActiveStatus.Inactive);

            var pendingClaims = _context.Claim
                .AsNoTracking()
                .Where(c =>
                    c.ClaimStatus == ClaimStatus.Pending);

            int pendingClaimsCount =
                await pendingClaims.CountAsync();

            decimal pendingClaimsAmount =
                await pendingClaims
                    .SumAsync(c => (decimal?)c.ClaimAmount) ?? 0m;

            var topClaims = await _context.Claim
                .AsNoTracking()
                .OrderByDescending(c => c.ClaimAmount)
                .ThenBy(c => c.ClaimId)
                .Take(5)
                .Select(c => new DashboardClaimDTO
                {
                    ClaimId = c.ClaimId,
                    ClaimNumber = c.ClaimNumber,
                    ClaimAmount = c.ClaimAmount,
                    ClaimStatus = c.ClaimStatus
                })
                .ToListAsync();

            return new AdminDashboardResponseDTO
            {
                ActiveUsers = activeUsers,
                InactiveUsers = inactiveUsers,
                PendingClaimsCount = pendingClaimsCount,
                PendingClaimsAmount = pendingClaimsAmount,
                TopClaims = topClaims
            };
        }

        // Officer Dashboard
        public async Task<OfficerDashboardResponseDTO>
            GetOfficerDashboardAsync()
        {
            var pendingClaims = _context.Claim
                .AsNoTracking()
                .Where(c =>
                    c.ClaimStatus == ClaimStatus.Pending);

            int pendingClaimsCount =
                await pendingClaims.CountAsync();

            decimal pendingClaimsAmount =
                await pendingClaims
                    .SumAsync(c => (decimal?)c.ClaimAmount) ?? 0m;

            var topClaims = await pendingClaims
                .OrderByDescending(c => c.ClaimAmount)
                .ThenBy(c => c.ClaimId)
                .Take(5)
                .Select(c => new DashboardClaimDTO
                {
                    ClaimId = c.ClaimId,
                    ClaimNumber = c.ClaimNumber,
                    ClaimAmount = c.ClaimAmount,
                    ClaimStatus = c.ClaimStatus
                })
                .ToListAsync();

            return new OfficerDashboardResponseDTO
            {
                PendingClaimsCount = pendingClaimsCount,
                PendingClaimsAmount = pendingClaimsAmount,
                TopClaims = topClaims
            };
        }

        // Customer Dashboard
        public async Task<CustomerDashboardResponseDTO>
            GetCustomerDashboardAsync(int userId)
        {
            var customer = await _context.CustomerProfile
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.CustomerId
                })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                throw new KeyNotFoundException(
                    "Customer profile was not found.");
            }

            var customerClaims = _context.Claim
                .AsNoTracking()
                .Where(c =>
                    c.Policy != null &&
                    c.Policy.CustomerId == customer.CustomerId);

            int totalClaimsCount =
                await customerClaims.CountAsync();

            var pendingClaims = customerClaims
                .Where(c =>
                    c.ClaimStatus == ClaimStatus.Pending);

            int pendingClaimsCount =
                await pendingClaims.CountAsync();

            decimal pendingClaimsAmount =
                await pendingClaims
                    .SumAsync(c => (decimal?)c.ClaimAmount) ?? 0m;

            var claimHistory = await customerClaims
                .OrderByDescending(c => c.CreatedDate)
                .ThenByDescending(c => c.ClaimId)
                .Take(10)
                .Select(c => new DashboardClaimHistoryDTO
                {
                    ClaimId = c.ClaimId,
                    ClaimNumber = c.ClaimNumber,
                    ClaimAmount = c.ClaimAmount,
                    ClaimStatus = c.ClaimStatus,
                    CreatedDate = c.CreatedDate,
                    UpdatedDate = c.UpdatedDate
                })
                .ToListAsync();

            return new CustomerDashboardResponseDTO
            {
                TotalClaimsCount = totalClaimsCount,
                PendingClaimsCount = pendingClaimsCount,
                PendingClaimsAmount = pendingClaimsAmount,
                ClaimHistory = claimHistory
            };
        }
    }

}
