using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    public class PolicyPlanRepository : IPolicyPlanRepository
    {
        private readonly AppDbContext _context;

        public PolicyPlanRepository(AppDbContext context)
        {
            _context = context;
        }

        // Add Policy Plan
        public async Task<PolicyPlan> AddPolicyPlan(PolicyPlan policyPlan)
        {
            policyPlan.CreatedDate = DateTime.Now;
            policyPlan.UpdatedDate = DateTime.Now;

            _context.PolicyPlan.Add(policyPlan);

            await _context.SaveChangesAsync();

            return await _context.PolicyPlan
                .AsNoTracking()
                .Where(p => p.PlanId == policyPlan.PlanId)
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
                    ActiveStatus = p.ActiveStatus
                })
                .FirstAsync();
        }

        // Get Policy Plan By ID
        public async Task<PolicyPlan?> GetPolicyPlanById(int id)
        {
            return await _context.PolicyPlan
                .AsNoTracking()
                .Where(p => p.PlanId == id)
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
                    ActiveStatus = p.ActiveStatus
                })
                .FirstOrDefaultAsync();
        }

        // Get All Policy Plans With Search, Sorting, And Pagination
        public async Task<PaginatedResponseDTO<PolicyPlan>>
            GetAllPolicyPlans(PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            IQueryable<PolicyPlan> plans = _context.PolicyPlan
                .AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim();

                plans = plans.Where(p =>
                    p.PlanName.Contains(searchTerm) ||
                    p.TermsAndConditions.Contains(searchTerm));
            }

            // Sorting
            plans = (
                query.SortBy?.ToLowerInvariant(),
                query.SortDirection?.ToLowerInvariant()
            ) switch
            {
                ("planname", "desc") =>
                    plans.OrderByDescending(p => p.PlanName)
                         .ThenBy(p => p.PlanId),

                ("planname", _) =>
                    plans.OrderBy(p => p.PlanName)
                         .ThenBy(p => p.PlanId),

                ("coverageamount", "desc") =>
                    plans.OrderByDescending(p => p.CoverageAmount)
                         .ThenBy(p => p.PlanId),

                ("coverageamount", _) =>
                    plans.OrderBy(p => p.CoverageAmount)
                         .ThenBy(p => p.PlanId),

                ("premiumamount", "desc") =>
                    plans.OrderByDescending(p => p.PremiumAmount)
                         .ThenBy(p => p.PlanId),

                ("premiumamount", _) =>
                    plans.OrderBy(p => p.PremiumAmount)
                         .ThenBy(p => p.PlanId),

                ("createddate", "desc") =>
                    plans.OrderByDescending(p => p.CreatedDate)
                         .ThenBy(p => p.PlanId),

                ("createddate", _) =>
                    plans.OrderBy(p => p.CreatedDate)
                         .ThenBy(p => p.PlanId),

                _ => plans.OrderBy(p => p.PlanId)
            };

            int totalRecords = await plans.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var pagedPlans = await plans
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
                    ActiveStatus = p.ActiveStatus
                })
                .ToListAsync();

            return new PaginatedResponseDTO<PolicyPlan>
            {
                Records = pagedPlans,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        // Update Policy Plan
        public async Task<PolicyPlan?> UpdatePolicyPlan(
            PolicyPlan policyPlan)
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
            existingPlan.TermsAndConditions =
                policyPlan.TermsAndConditions;

            existingPlan.ActiveStatus = policyPlan.ActiveStatus;
            existingPlan.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return await _context.PolicyPlan
                .AsNoTracking()
                .Where(p => p.PlanId == policyPlan.PlanId)
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
                    ActiveStatus = p.ActiveStatus
                })
                .FirstOrDefaultAsync();
        }

        // Delete Policy Plan
        // Prevent deletion if any policy uses this plan.
        public async Task<bool> DeletePolicyPlan(int id)
        {
            var existingPlan = await _context.PolicyPlan
                .FirstOrDefaultAsync(p => p.PlanId == id);

            if (existingPlan == null)
            {
                return false;
            }

            bool hasPolicies = await _context.Policy
                .AnyAsync(p => p.PlanId == id);

            if (hasPolicies)
            {
                return false;
            }

            _context.PolicyPlan.Remove(existingPlan);

            await _context.SaveChangesAsync();

            return true;
        }

        // Check Policy Plan Exists
        public async Task<bool> PolicyPlanExists(int id)
        {
            return await _context.PolicyPlan
                .AsNoTracking()
                .AnyAsync(p => p.PlanId == id);
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
            existingPlan.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return await _context.PolicyPlan
                .AsNoTracking()
                .Where(p => p.PlanId == id)
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
                    ActiveStatus = p.ActiveStatus
                })
                .FirstOrDefaultAsync();
        }
    }
}
