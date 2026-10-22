
using Insurance_Backend.Data;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Insurance_Backend.Repositories.classes
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly AppDbContext _context;

        public PolicyRepository(AppDbContext context)
        {
            _context = context;
        }

        
        private IQueryable<Policy> PolicySummaryQuery()
        {
            return _context.Policy
                .AsNoTracking()
                .Select(p => new Policy
                {
                    PolicyId = p.PolicyId,
                    PolicyNumber = p.PolicyNumber,
                    CustomerId = p.CustomerId,
                    PlanId = p.PlanId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    PolicyStatus = p.PolicyStatus,
                    TotalPremiumPaid = p.TotalPremiumPaid,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,

                    Customer = new CustomerProfile
                    {
                        User = new User
                        {
                            FullName = p.Customer.User.FullName
                        }
                    },

                    PolicyPlan = new PolicyPlan
                    {
                        PlanName = p.PolicyPlan.PlanName,
                        PremiumAmount = p.PolicyPlan.PremiumAmount
                    }
                });
        }

        public async Task<Policy> CreatePolicy(Policy policy)
        {
            _context.Policy.Add(policy);

            await _context.SaveChangesAsync();

            var createdPolicy = await PolicySummaryQuery()
                .FirstAsync(p => p.PolicyId == policy.PolicyId);

            return createdPolicy;
        }

        public async Task<Policy?> CancelPolicy(int policyId)
        {
            var policy = await _context.Policy
                .FirstOrDefaultAsync(p => p.PolicyId == policyId);

            if (policy == null)
            {
                return null;
            }

            policy.PolicyStatus = PolicyStatus.Cancelled;
            policy.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await PolicySummaryQuery()
                .FirstOrDefaultAsync(p => p.PolicyId == policyId);
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByCustomerId(
            int customerId)
        {
            return await PolicySummaryQuery()
                .Where(p => p.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByPlanId(
            int planId)
        {
            return await PolicySummaryQuery()
                .Where(p => p.PlanId == planId)
                .ToListAsync();
        }

        public async Task<Policy?> GetPolicyById(int policyId)
        {
            return await PolicySummaryQuery()
                .FirstOrDefaultAsync(p => p.PolicyId == policyId);
        }

        public async Task<Policy?> GetPolicyByPolicyNumber(
            string policyNumber)
        {
            return await PolicySummaryQuery()
                .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
        }

        public async Task<PaginatedResponseDTO<Policy>> ListPolicies(
            PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            // Start with the entity query for filtering and sorting.
            IQueryable<Policy> policies = _context.Policy.AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim().ToLower();

                policies = policies.Where(p =>
                    p.PolicyNumber.ToLower().Contains(searchTerm) ||
                    p.Customer.User.FullName.ToLower().Contains(searchTerm) ||
                    p.PolicyPlan.PlanName.ToLower().Contains(searchTerm));
            }

            // Sorting
            policies = (
                query.SortBy?.ToLowerInvariant(),
                query.SortDirection?.ToLowerInvariant()
            ) switch
            {
                ("policynumber", "desc") =>
                    policies.OrderByDescending(p => p.PolicyNumber)
                            .ThenBy(p => p.PolicyId),

                ("policynumber", _) =>
                    policies.OrderBy(p => p.PolicyNumber)
                            .ThenBy(p => p.PolicyId),

                ("planname", "desc") =>
                    policies.OrderByDescending(p => p.PolicyPlan.PlanName)
                            .ThenBy(p => p.PolicyId),

                ("planname", _) =>
                    policies.OrderBy(p => p.PolicyPlan.PlanName)
                            .ThenBy(p => p.PolicyId),

                ("customername", "desc") =>
                    policies.OrderByDescending(p => p.Customer.User.FullName)
                            .ThenBy(p => p.PolicyId),

                ("customername", _) =>
                    policies.OrderBy(p => p.Customer.User.FullName)
                            .ThenBy(p => p.PolicyId),

                ("startdate", "desc") =>
                    policies.OrderByDescending(p => p.StartDate)
                            .ThenBy(p => p.PolicyId),

                ("startdate", _) =>
                    policies.OrderBy(p => p.StartDate)
                            .ThenBy(p => p.PolicyId),

                ("enddate", "desc") =>
                    policies.OrderByDescending(p => p.EndDate)
                            .ThenBy(p => p.PolicyId),

                ("enddate", _) =>
                    policies.OrderBy(p => p.EndDate)
                            .ThenBy(p => p.PolicyId),

                ("totalpremiumpaid", "desc") =>
                    policies.OrderByDescending(p => p.TotalPremiumPaid)
                            .ThenBy(p => p.PolicyId),

                ("totalpremiumpaid", _) =>
                    policies.OrderBy(p => p.TotalPremiumPaid)
                            .ThenBy(p => p.PolicyId),

                ("policystatus", "desc") =>
                    policies.OrderByDescending(p => p.PolicyStatus)
                            .ThenBy(p => p.PolicyId),

                ("policystatus", _) =>
                    policies.OrderBy(p => p.PolicyStatus)
                            .ThenBy(p => p.PolicyId),

                ("createddate", "desc") =>
                    policies.OrderByDescending(p => p.CreatedDate)
                            .ThenBy(p => p.PolicyId),

                ("createddate", _) =>
                    policies.OrderBy(p => p.CreatedDate)
                            .ThenBy(p => p.PolicyId),

                _ => policies.OrderBy(p => p.PolicyId)
            };

            int totalRecords = await policies.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalRecords / (double)pageSize);

            var pagedPolicies = await policies
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new Policy
                {
                    PolicyId = p.PolicyId,
                    PolicyNumber = p.PolicyNumber,
                    CustomerId = p.CustomerId,
                    PlanId = p.PlanId,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    PolicyStatus = p.PolicyStatus,
                    TotalPremiumPaid = p.TotalPremiumPaid,
                    CreatedDate = p.CreatedDate,
                    UpdatedDate = p.UpdatedDate,

                    Customer = new CustomerProfile
                    {
                        User = new User
                        {
                            FullName = p.Customer.User.FullName
                        }
                    },

                    PolicyPlan = new PolicyPlan
                    {
                        PlanName = p.PolicyPlan.PlanName,
                        PremiumAmount = p.PolicyPlan.PremiumAmount
                    }
                })
                .ToListAsync();

            return new PaginatedResponseDTO<Policy>
            {
                Records = pagedPolicies,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                IsLastPage = pageNumber >= totalPages,
                SortBy = query.SortBy,
                SortDirection = query.SortDirection
            };
        }

        public async Task UpdatePolicy(Policy policy)
        {
            policy.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
