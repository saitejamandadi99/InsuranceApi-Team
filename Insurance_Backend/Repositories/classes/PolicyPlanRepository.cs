using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    /// <summary>
    /// Handles database operations for PolicyPlan.
    /// Business validation belongs in the service layer.
    /// </summary>
    public class PolicyPlanRepository : IPolicyPlanRepository
    {
        private readonly AppDbContext _context;

        public PolicyPlanRepository(AppDbContext context)
        {
            _context = context;
        }

        // Base query for policy plans with related insurance product details
        private IQueryable<PolicyPlan> PolicyPlanSummaryQuery()
        {
            return _context.PolicyPlan
                .AsNoTracking()
                .Select(p => new PolicyPlan
                {
                    PlanId = p.PlanId,
                    ProductId = p.ProductId,
                    PlanName = p.PlanName,
                    CoverageAmount = p.CoverageAmount,
                    PremiumAmount = p.PremiumAmount,
                    PremiumType = p.PremiumType,
                    Duration = p.Duration,
                    TermsAndConditions = p.TermsAndConditions,
                    ActiveStatus = p.ActiveStatus,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,

                    InsuranceProduct = p.InsuranceProduct == null
                        ? null
                        : new InsuranceProduct
                        {
                            ProductId = p.InsuranceProduct.ProductId,
                            ProductName = p.InsuranceProduct.ProductName,
                            ProductType = p.InsuranceProduct.ProductType
                        }
                });
        }

        // Add Policy Plan
        public async Task<PolicyPlan> AddPolicyPlan(PolicyPlan policyPlan)
        {
            DateTime currentDate = DateTime.UtcNow;

            policyPlan.CreatedDate = currentDate;
            policyPlan.UpdatedDate = currentDate;

            await _context.PolicyPlan.AddAsync(policyPlan);
            await _context.SaveChangesAsync();

            return await PolicyPlanSummaryQuery()
                .FirstAsync(p => p.PlanId == policyPlan.PlanId);
        }

        // Get Policy Plan By ID
        public async Task<PolicyPlan?> GetPolicyPlanById(int id)
        {
            return await _context.PolicyPlan
                .FirstOrDefaultAsync(p => p.PlanId == id);
        }

        // Get All Policy Plans with search, sorting and pagination
        public async Task<PaginatedResponseDTO<PolicyPlan>> GetAllPolicyPlans(
            PaginationQueryDTO query)
        {
            int pageNumber = query.PageNumber;
            int pageSize = query.PageSize;

            IQueryable<PolicyPlan> plans = PolicyPlanSummaryQuery();

            // Search by plan name or terms and conditions
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim();

                plans = plans.Where(p =>
                    p.PlanName.Contains(searchTerm) ||
                    p.TermsAndConditions.Contains(searchTerm));
            }

            // Determine sorting
            string sortBy = string.IsNullOrWhiteSpace(query.SortBy)
                ? "createddate"
                : query.SortBy.Trim().ToLowerInvariant();

            string sortDirection =
                string.Equals(
                    query.SortDirection?.Trim(),
                    "asc",
                    StringComparison.OrdinalIgnoreCase)
                    ? "asc"
                    : "desc";

            bool descending = sortDirection == "desc";

            IOrderedQueryable<PolicyPlan> orderedPlans = sortBy switch
            {
                "planname" => descending
                    ? plans.OrderByDescending(p => p.PlanName)
                    : plans.OrderBy(p => p.PlanName),

                "coverageamount" => descending
                    ? plans.OrderByDescending(p => p.CoverageAmount)
                    : plans.OrderBy(p => p.CoverageAmount),

                "premiumamount" => descending
                    ? plans.OrderByDescending(p => p.PremiumAmount)
                    : plans.OrderBy(p => p.PremiumAmount),

                "duration" => descending
                    ? plans.OrderByDescending(p => p.Duration)
                    : plans.OrderBy(p => p.Duration),

                "createddate" => descending
                    ? plans.OrderByDescending(p => p.CreatedDate)
                    : plans.OrderBy(p => p.CreatedDate),

                "updateddate" => descending
                    ? plans.OrderByDescending(p => p.UpdatedDate)
                    : plans.OrderBy(p => p.UpdatedDate),

                _ => descending
                    ? plans.OrderByDescending(p => p.CreatedDate)
                    : plans.OrderBy(p => p.CreatedDate)
            };

            // Stable ordering for pagination
            orderedPlans = orderedPlans.ThenBy(p => p.PlanId);

            int totalRecords = await orderedPlans.CountAsync();

            int totalPages = pageSize > 0
                ? (int)Math.Ceiling(totalRecords / (double)pageSize)
                : 0;

            var pagedPlans = await orderedPlans
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<PolicyPlan>
            {
                Records = pagedPlans,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = totalPages == 0 || pageNumber >= totalPages,
                SortBy = sortBy,
                SortDirection = sortDirection
            };
        }

        // Update Policy Plan
        // ActiveStatus is managed through the deactivation method.
        public async Task<PolicyPlan?> UpdatePolicyPlan(PolicyPlan policyPlan)
        {
            var existingPlan = await _context.PolicyPlan
                .FirstOrDefaultAsync(p => p.PlanId == policyPlan.PlanId);

            if (existingPlan == null)
            {
                return null;
            }

            existingPlan.ProductId = policyPlan.ProductId;
            existingPlan.PlanName = policyPlan.PlanName;
            existingPlan.CoverageAmount = policyPlan.CoverageAmount;
            existingPlan.PremiumAmount = policyPlan.PremiumAmount;
            existingPlan.PremiumType = policyPlan.PremiumType;
            existingPlan.Duration = policyPlan.Duration;
            existingPlan.TermsAndConditions = policyPlan.TermsAndConditions;
            existingPlan.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await PolicyPlanSummaryQuery()
                .FirstOrDefaultAsync(p => p.PlanId == policyPlan.PlanId);
        }

        // Deactivate Policy Plan
        public async Task<PolicyPlan?> DeactivatePolicyPlan(int id)
        {
            var existingPlan = await _context.PolicyPlan
                .FirstOrDefaultAsync(p => p.PlanId == id);

            if (existingPlan == null)
            {
                return null;
            }

            existingPlan.ActiveStatus = ActiveStatus.Inactive;
            existingPlan.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await PolicyPlanSummaryQuery()
                .FirstOrDefaultAsync(p => p.PlanId == id);
        }

        // Check whether Policy Plan exists
        public async Task<bool> PolicyPlanExists(int id)
        {
            return await _context.PolicyPlan
                .AsNoTracking()
                .AnyAsync(p => p.PlanId == id);
        }

        // Check whether an Insurance Product exists and is active
        public async Task<bool> IsProductActive(int productId)
        {
            return await _context.InsuranceProduct
                .AsNoTracking()
                .AnyAsync(p =>
                    p.ProductId == productId &&
                    p.ActiveStatus == ActiveStatus.Active);
        }
        // Activate Policy Plan
        public async Task<PolicyPlan?> ActivatePolicyPlan(int id)
        {
            var existingPlan = await _context.PolicyPlan
                .FirstOrDefaultAsync(p => p.PlanId == id);

            if (existingPlan == null)
            {
                return null;
            }

            existingPlan.ActiveStatus = ActiveStatus.Active;
            existingPlan.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await PolicyPlanSummaryQuery()
                .FirstOrDefaultAsync(p => p.PlanId == id);
        }
        // Get Policy Plans by Product ID with search, sorting and pagination

        public async Task<PaginatedResponseDTO<PolicyPlan>> GetPlansByProductId(

            int productId,

            PaginationQueryDTO query)

        {

            int pageNumber = query.PageNumber;

            int pageSize = query.PageSize;

            IQueryable<PolicyPlan> plans = PolicyPlanSummaryQuery()

                .Where(p => p.ProductId == productId);

            // Search by plan name or terms and conditions

            if (!string.IsNullOrWhiteSpace(query.Search))

            {

                string searchTerm = query.Search.Trim();

                plans = plans.Where(p =>

                    p.PlanName.Contains(searchTerm) ||

                    p.TermsAndConditions.Contains(searchTerm));

            }

            // Determine sorting

            string sortBy = string.IsNullOrWhiteSpace(query.SortBy)

                ? "createddate"

                : query.SortBy.Trim().ToLowerInvariant();

            string sortDirection =

                string.Equals(

                    query.SortDirection?.Trim(),

                    "asc",

                    StringComparison.OrdinalIgnoreCase)

                    ? "asc"

                    : "desc";

            bool descending = sortDirection == "desc";

            IOrderedQueryable<PolicyPlan> orderedPlans = sortBy switch

            {

                "planname" => descending

                    ? plans.OrderByDescending(p => p.PlanName)

                    : plans.OrderBy(p => p.PlanName),

                "coverageamount" => descending

                    ? plans.OrderByDescending(p => p.CoverageAmount)

                    : plans.OrderBy(p => p.CoverageAmount),

                "premiumamount" => descending

                    ? plans.OrderByDescending(p => p.PremiumAmount)

                    : plans.OrderBy(p => p.PremiumAmount),

                "duration" => descending

                    ? plans.OrderByDescending(p => p.Duration)

                    : plans.OrderBy(p => p.Duration),

                "createddate" => descending

                    ? plans.OrderByDescending(p => p.CreatedDate)

                    : plans.OrderBy(p => p.CreatedDate),

                "updateddate" => descending

                    ? plans.OrderByDescending(p => p.UpdatedDate)

                    : plans.OrderBy(p => p.UpdatedDate),

                _ => descending

                    ? plans.OrderByDescending(p => p.CreatedDate)

                    : plans.OrderBy(p => p.CreatedDate)

            };

            // Stable ordering for pagination

            orderedPlans = orderedPlans.ThenBy(p => p.PlanId);

            int totalRecords = await orderedPlans.CountAsync();

            int totalPages = pageSize > 0

                ? (int)Math.Ceiling(totalRecords / (double)pageSize)

                : 0;

            var pagedPlans = await orderedPlans

                .Skip((pageNumber - 1) * pageSize)

                .Take(pageSize)

                .ToListAsync();

            return new PaginatedResponseDTO<PolicyPlan>

            {

                Records = pagedPlans,

                PageSize = pageSize,

                CurrentPage = pageNumber,

                TotalRecords = totalRecords,

                TotalPages = totalPages,

                IsLastPage = totalPages == 0 || pageNumber >= totalPages,

                SortBy = sortBy,

                SortDirection = sortDirection

            };

        }

    }
}
