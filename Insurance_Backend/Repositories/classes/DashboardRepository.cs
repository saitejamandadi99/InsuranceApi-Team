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
                .Where(c => c.ClaimStatus != ClaimStatus.Rejected && c.ClaimStatus != ClaimStatus.Approved)
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

            var recentPolicies = await GetRecentPoliciesAsync();

            var recentPayments = await GetRecentPaymentsAsync();
            return new AdminDashboardResponseDTO
            {
                ActiveUsers = activeUsers,

                InactiveUsers = inactiveUsers,

                PendingClaimsCount = pendingClaimsCount,

                PendingClaimsAmount = pendingClaimsAmount,

                TopClaims = topClaims,

                RecentPolicies = recentPolicies,

                RecentPayments = recentPayments
            };
        }

        // Officer Dashboard
        public async Task<OfficerDashboardResponseDTO>
            GetOfficerDashboardAsync()
        {
            var recentPolicies = await GetRecentPoliciesAsync();
            var recentPayments = await GetRecentPaymentsAsync();

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
                TopClaims = topClaims,
                RecentPolicies = recentPolicies,
                RecentPayments = recentPayments
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
            var recentPolicies =
                await GetRecentPoliciesAsync(customer.CustomerId);

            var recentPayments =
                await GetRecentPaymentsAsync(customer.CustomerId);



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
                ClaimHistory = claimHistory,
                RecentPolicies = recentPolicies,
                RecentPayments = recentPayments
            };
        }

        private async Task<List<DashboardPolicyResponseDTO>>
    GetRecentPoliciesAsync(int? customerId = null)
        {
            var query = _context.Policy
                .AsNoTracking()
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(p =>
                    p.CustomerId == customerId.Value);
            }

            return await query
                .OrderByDescending(p => p.CreatedDate)
                .ThenByDescending(p => p.PolicyId)
                .Take(5)
                .Select(p => new DashboardPolicyResponseDTO
                {
                    PolicyId = p.PolicyId,

                    PolicyNumber = p.PolicyNumber,

                    CustomerName =
                        p.Customer != null && p.Customer.User != null
                            ? p.Customer.User.FullName
                            : "",

                    ProductName =
                        p.PolicyPlan != null &&
                        p.PolicyPlan.InsuranceProduct != null
                            ? p.PolicyPlan.InsuranceProduct.ProductName
                            : "",

                    PlanName =
                        p.PolicyPlan != null
                            ? p.PolicyPlan.PlanName
                            : "",

                    StartDate = p.StartDate,

                    EndDate = p.EndDate,

                    PolicyStatus = p.PolicyStatus,

                    TotalPremiumPaid = p.TotalPremiumPaid,

                    CreatedDate = p.CreatedDate
                })
                .ToListAsync();
        }

        private async Task<List<DashboardPaymentResponseDTO>>
    GetRecentPaymentsAsync(int? customerId = null)
        {
            var query = _context.PremiumPayment
                .AsNoTracking()
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(p =>
                    p.Policy != null &&
                    p.Policy.CustomerId == customerId.Value);
            }

            return await query
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId)
                .Take(5)
                .Select(p => new DashboardPaymentResponseDTO
                {
                    PaymentId = p.PaymentId,

                    PolicyId = p.PolicyId,

                    PolicyNumber =
                        p.Policy != null
                            ? p.Policy.PolicyNumber
                            : "",

                    CustomerName =
                        p.Policy != null &&
                        p.Policy.Customer != null &&
                        p.Policy.Customer.User != null
                            ? p.Policy.Customer.User.FullName
                            : "",

                    Amount = p.Amount,

                    PaymentDate = p.PaymentDate,

                    PaymentMode = p.PaymentMode,

                    TransactionReference = p.TransactionReference,

                    PaymentStatus = p.PaymentStatus
                })
                .ToListAsync();
        }
    }

}
