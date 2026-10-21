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

        public async Task<PolicyResponseDTO?> CreatePolicy(Policy policy)
        {
            _context.Policy.Add(policy);
            await _context.SaveChangesAsync();

            return new PolicyResponseDTO
            {
                PolicyId = policy.PolicyId,
                PolicyNumber = policy.PolicyNumber,
                CustomerId = policy.CustomerId,
                CustomerName = policy.Customer.User.FullName,
                PlanId = policy.PlanId,
                PlanName = policy.PolicyPlan.PlanName,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                PolicyStatus = policy.PolicyStatus,
                TotalPremiumPaid = policy.TotalPremiumPaid,
                PremiumAmount = policy.PolicyPlan.PremiumAmount
            };
        }

        public async Task<PolicyResponseDTO?> CancelPolicy(int policyId)
        {
            var policy = await _context.Policy.FindAsync(policyId);

            if (policy == null)
            {
                return null;
            }

            policy.PolicyStatus = PolicyStatus.Cancelled;
            policy.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedPolicy = await _context.Policy.FirstOrDefaultAsync(p => p.PolicyId == policyId);

            if (updatedPolicy == null)
            {
                return null;
            }

            return new PolicyResponseDTO
            {
                PolicyId = updatedPolicy.PolicyId,
                PolicyNumber = updatedPolicy.PolicyNumber,
                CustomerId = updatedPolicy.CustomerId,
                CustomerName = updatedPolicy.Customer.User.FullName,
                PlanId = updatedPolicy.PlanId,
                PlanName = updatedPolicy.PolicyPlan.PlanName,
                StartDate = updatedPolicy.StartDate,
                EndDate = updatedPolicy.EndDate,
                PolicyStatus = updatedPolicy.PolicyStatus,
                TotalPremiumPaid = updatedPolicy.TotalPremiumPaid,
                PremiumAmount = updatedPolicy.PolicyPlan.PremiumAmount
            };
        }

        public async Task<IEnumerable<PolicyResponseDTO>> GetPoliciesByCustomerId(
            int customerId)
        {
            return await _context.Policy.AsNoTracking().Where(p => p.CustomerId == customerId)
                .Select(p => new PolicyResponseDTO
                {
                    PolicyId = p.PolicyId,
                    PolicyNumber = p.PolicyNumber,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer.User.FullName,
                    PlanId = p.PlanId,
                    PlanName = p.PolicyPlan.PlanName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    PolicyStatus = p.PolicyStatus,
                    TotalPremiumPaid = p.TotalPremiumPaid,
                    PremiumAmount = p.PolicyPlan.PremiumAmount
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<PolicyResponseDTO>> GetPoliciesByPlanId(
            int planId)
        {
            return await _context.Policy.AsNoTracking().Where(p => p.PlanId == planId)
                .Select(p => new PolicyResponseDTO
                {
                    PolicyId = p.PolicyId,
                    PolicyNumber = p.PolicyNumber,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer.User.FullName,
                    PlanId = p.PlanId,
                    PlanName = p.PolicyPlan.PlanName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    PolicyStatus = p.PolicyStatus,
                    TotalPremiumPaid = p.TotalPremiumPaid,
                    PremiumAmount = p.PolicyPlan.PremiumAmount
                })
                .ToListAsync();
        }

        public async Task<PolicyResponseDTO?> GetPolicyById(int policyId)
        {
            return await _context.Policy.AsNoTracking().Where(p => p.PolicyId == policyId)
                .Select(p => new PolicyResponseDTO
                {
                    PolicyId = p.PolicyId,
                    PolicyNumber = p.PolicyNumber,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer.User.FullName,
                    PlanId = p.PlanId,
                    PlanName = p.PolicyPlan.PlanName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    PolicyStatus = p.PolicyStatus,
                    TotalPremiumPaid = p.TotalPremiumPaid,
                    PremiumAmount = p.PolicyPlan.PremiumAmount
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PolicyResponseDTO?> GetPolicyByPolicyNumber(
            string policyNumber)
        {
            return await _context.Policy.AsNoTracking().Where(p => p.PolicyNumber == policyNumber)
                .Select(p => new PolicyResponseDTO
                {
                    PolicyId = p.PolicyId,
                    PolicyNumber = p.PolicyNumber,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer.User.FullName,
                    PlanId = p.PlanId,
                    PlanName = p.PolicyPlan.PlanName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    PolicyStatus = p.PolicyStatus,
                    TotalPremiumPaid = p.TotalPremiumPaid,
                    PremiumAmount = p.PolicyPlan.PremiumAmount
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatedResponseDTO<PolicyResponseDTO>> ListPolicies(PaginationQueryDTO query)
        {
            int pageNumber = Math.Max(query.PageNumber, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, 100);

            IQueryable<Policy> policies = _context.Policy.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string searchTerm = query.Search.Trim().ToLower();

                policies = policies.Where(p => p.PolicyNumber.ToLower().Contains(searchTerm) || p.Customer.User.FullName.ToLower().Contains(searchTerm) ||
                    p.PolicyPlan.PlanName.ToLower().Contains(searchTerm));
            }

            policies = (query.SortBy?.ToLowerInvariant(), query.SortDirection?.ToLowerInvariant()) switch
            {
                ("policynumber", "desc") =>
                    policies.OrderByDescending(p => p.PolicyNumber).ThenBy(p => p.PolicyId),
                ("policynumber", _) =>
                    policies.OrderBy(p => p.PolicyNumber).ThenBy(p => p.PolicyId),

                ("planname", "desc") =>
                    policies.OrderByDescending(p => p.PolicyPlan.PlanName).ThenBy(p => p.PolicyId),
                ("planname", _) =>
                    policies.OrderBy(p => p.PolicyPlan.PlanName).ThenBy(p => p.PolicyId),

                ("customername", "desc") =>
                    policies.OrderByDescending(p => p.Customer.User.FullName).ThenBy(p => p.PolicyId),
                ("customername", _) =>
                    policies.OrderBy(p => p.Customer.User.FullName).ThenBy(p => p.PolicyId),

                ("startdate", "desc") =>
                    policies.OrderByDescending(p => p.StartDate).ThenBy(p => p.PolicyId),
                ("startdate", _) =>
                    policies.OrderBy(p => p.StartDate).ThenBy(p => p.PolicyId),

                ("enddate", "desc") =>
                    policies.OrderByDescending(p => p.EndDate).ThenBy(p => p.PolicyId),
                ("enddate", _) =>
                    policies.OrderBy(p => p.EndDate).ThenBy(p => p.PolicyId),

                ("totalpremiumpaid", "desc") =>
                    policies.OrderByDescending(p => p.TotalPremiumPaid).ThenBy(p => p.PolicyId),
                ("totalpremiumpaid", _) =>
                    policies.OrderBy(p => p.TotalPremiumPaid).ThenBy(p => p.PolicyId),

                ("policystatus", "desc") =>
                    policies.OrderByDescending(p => p.PolicyStatus).ThenBy(p => p.PolicyId),
                ("policystatus", _) =>
                    policies.OrderBy(p => p.PolicyStatus).ThenBy(p => p.PolicyId),

                ("createddate", "desc") =>
                    policies.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.PolicyId),
                ("createddate", _) =>
                    policies.OrderBy(p => p.CreatedDate).ThenBy(p => p.PolicyId),

                _ => policies.OrderBy(p => p.PolicyId)
            };

            int totalRecords = await policies.CountAsync();
            int totalPages = (int)Math.Ceiling( totalRecords / (double)pageSize);

            var pagedPolicies = await policies.Select(p => new PolicyResponseDTO
                {
                    PolicyId = p.PolicyId,
                    PolicyNumber = p.PolicyNumber,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer.User.FullName,
                    PlanId = p.PlanId,
                    PlanName = p.PolicyPlan.PlanName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    PolicyStatus = p.PolicyStatus,
                    TotalPremiumPaid = p.TotalPremiumPaid,
                    PremiumAmount = p.PolicyPlan.PremiumAmount
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponseDTO<PolicyResponseDTO>
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
