using AutoMapper;
using Insurance_Backend.Config;
using Insurance_Backend.Custom;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Insurance_Backend.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Insurance_Backend.Services.Classes
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepo;
        private readonly ICustomerProfileRepository _cusRepo;
        private readonly IPolicyPlanRepository _planRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<PolicyService> _logger;

        public PolicyService(
            IPolicyRepository policyRepo,
            ICustomerProfileRepository cusRepo,
            IPolicyPlanRepository planRepo,
            IMapper mapper,
            ILogger<PolicyService> logger)
        {
            _policyRepo = policyRepo;
            _cusRepo = cusRepo;
            _planRepo = planRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PolicyResponseDTO?> CancelPolicy(int policyId)
        {
            var existingPolicy =
                await _policyRepo.GetPolicyById(policyId);

            if (existingPolicy == null)
            {
                throw new BusinessException(
                    "Policy does not exist.", 404);
            }

            if (existingPolicy.PolicyStatus == PolicyStatus.Cancelled)
            {
                throw new BusinessException(
                    "Policy is already cancelled.", 409);
            }

            // Repository must load the tracked entity and save the change.
            var cancelledPolicy =
                await _policyRepo.CancelPolicy(policyId);

            if (cancelledPolicy == null)
            {
                throw new BusinessException(
                    "Policy does not exist.", 404);
            }

            _logger.LogInformation(
                "Policy '{PolicyNumber}' was cancelled.",
                cancelledPolicy.PolicyNumber);

            return _mapper.Map<PolicyResponseDTO>(cancelledPolicy);
        }

        public async Task<IEnumerable<PolicyResponseDTO>>
            GetPoliciesByCustomerId(int customerId)
        {
            var policies =
                await _policyRepo.GetPoliciesByCustomerId(customerId);

            return _mapper.Map<IEnumerable<PolicyResponseDTO>>(policies);
        }

        public async Task<IEnumerable<PolicyResponseDTO>>
            GetPoliciesByPlanId(int planId)
        {
            var policies =
                await _policyRepo.GetPoliciesByPlanId(planId);

            return _mapper.Map<IEnumerable<PolicyResponseDTO>>(policies);
        }

        public async Task<PolicyResponseDTO?> GetPolicyById(int policyId)
        {
            var existingPolicy =
                await _policyRepo.GetPolicyById(policyId);

            if (existingPolicy == null)
            {
                throw new BusinessException(
                    "Policy does not exist.", 404);
            }

            return _mapper.Map<PolicyResponseDTO>(existingPolicy);
        }

        public async Task<PolicyResponseDTO> IssuePolicy(
            OfficerOrAdminPolicyIssueRequestDTO request)
        {
            var customer =
                await _cusRepo.GetCustomerProfileById(request.CustomerId);

            if (customer == null)
            {
                throw new BusinessException(
                    "Customer does not exist.", 404);
            }

            if (!IsCustomerProfileComplete(customer))
            {
                throw new BusinessException(
                    "Customer profile is incomplete.", 400);
            }

            if (customer.User.ActiveStatus != ActiveStatus.Active)
            {
                throw new BusinessException(
                    "Customer is not active.", 400);
            }

            var plan = await _planRepo.GetPolicyPlanById(request.PlanId);

            if (plan == null)
            {
                throw new BusinessException(
                    "Plan does not exist.", 404);
            }

            if (plan.ActiveStatus != ActiveStatus.Active)
            {
                throw new BusinessException(
                    "Plan is not active.", 400);
            }

            var existingPolicies =
                await _policyRepo.GetPoliciesByCustomerId(
                    customer.CustomerId);

            if (existingPolicies.Any(p => p.PlanId == plan.PlanId))
            {
                throw new BusinessException(
                    "Customer has already subscribed to this policy plan.",
                    409);
            }

            // Backend determines the enrollment date.
            var startDate = DateTime.Today;

            var addPolicy = new Policy
            {
                CustomerId = customer.CustomerId,
                PlanId = plan.PlanId,
                PolicyNumber = await GeneratePolicyNumber(),
                StartDate = startDate,
                EndDate = startDate.AddDays(15),
                PolicyStatus = PolicyStatus.PendingPayment,
                TotalPremiumPaid = 0,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            try
            {
                var createdPolicy =
                    await _policyRepo.CreatePolicy(addPolicy);

                // Used for response mapping.
                createdPolicy.Customer = customer;
                createdPolicy.PolicyPlan = plan;

                _logger.LogInformation(
                    "Policy '{PolicyNumber}' issued for customer '{CustomerId}'.",
                    createdPolicy.PolicyNumber,
                    customer.CustomerId);

                return _mapper.Map<PolicyResponseDTO>(createdPolicy);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx &&
                    (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    throw new BusinessException(
                        "Customer has already subscribed to this policy plan.",
                        409);
                }

                // Preserve unexpected database exceptions.
                throw;
            }
        }

        public async Task<PaginatedResponseDTO<PolicyResponseDTO>>
            ListPolicies(PaginationQueryDTO query)
        {
            var policies = await _policyRepo.ListPolicies(query);

            return new PaginatedResponseDTO<PolicyResponseDTO>
            {
                Records =
                    _mapper.Map<IEnumerable<PolicyResponseDTO>>(
                        policies.Records),
                CurrentPage = policies.CurrentPage,
                PageSize = policies.PageSize,
                TotalRecords = policies.TotalRecords,
                TotalPages = policies.TotalPages,
                IsLastPage = policies.IsLastPage,
                SortBy = policies.SortBy,
                SortDirection = policies.SortDirection
            };
        }

        public async Task<PolicyResponseDTO> PurchasePolicy(
            CustomerPolicyPurchaseRequestDTO request,
            ClaimsPrincipal user)
        {
            var userId = user.GetUserId();

            var customer =
                await _cusRepo.GetCustomerProfileByUserId(userId);

            if (customer == null)
            {
                throw new BusinessException(
                    "Customer does not exist.", 404);
            }

            if (customer.User.ActiveStatus != ActiveStatus.Active)
            {
                throw new BusinessException(
                    "Customer is not active.", 400);
            }

            if (!IsCustomerProfileComplete(customer))
            {
                throw new BusinessException(
                    "Customer profile is incomplete.", 400);
            }

            var plan = await _planRepo.GetPolicyPlanById(request.PlanId);

            if (plan == null)
            {
                throw new BusinessException(
                    "Plan does not exist.", 404);
            }

            if (plan.ActiveStatus != ActiveStatus.Active)
            {
                throw new BusinessException(
                    "Plan is not active.", 400);
            }

            var existingPolicies =
                await _policyRepo.GetPoliciesByCustomerId(
                    customer.CustomerId);

            if (existingPolicies.Any(p => p.PlanId == plan.PlanId))
            {
                throw new BusinessException(
                    "You have already subscribed to this policy plan.",
                    409);
            }

            // Backend determines the enrollment date.
            var startDate = DateTime.Today;

            var addPolicy = new Policy
            {
                CustomerId = customer.CustomerId,
                PlanId = plan.PlanId,
                PolicyNumber = await GeneratePolicyNumber(),
                StartDate = startDate,
                EndDate = startDate.AddDays(15),
                PolicyStatus = PolicyStatus.PendingPayment,
                TotalPremiumPaid = 0,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            try
            {
                var createdPolicy =
                    await _policyRepo.CreatePolicy(addPolicy);

                createdPolicy.Customer = customer;
                createdPolicy.PolicyPlan = plan;

                _logger.LogInformation(
                    "Customer '{CustomerId}' purchased policy '{PolicyNumber}'.",
                    customer.CustomerId,
                    createdPolicy.PolicyNumber);

                return _mapper.Map<PolicyResponseDTO>(createdPolicy);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx &&
                    (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    throw new BusinessException(
                        "You have already subscribed to this policy plan.",
                        409);
                }

                // Preserve unexpected database exceptions.
                throw;
            }
        }

        public async Task<string> GeneratePolicyNumber()
        {
            string policyNumber;

            do
            {
                policyNumber = Random.Shared
                    .NextInt64(1000000000, 9999999999)
                    .ToString();
            }
            while (
                await _policyRepo.GetPolicyByPolicyNumber(policyNumber)
                != null);

            return policyNumber;
        }

        private bool IsCustomerProfileComplete(
            CustomerProfile customer)
        {
            return customer.DateOfBirth != default &&
                   !string.IsNullOrWhiteSpace(customer.Address) &&
                   !string.IsNullOrWhiteSpace(customer.City) &&
                   !string.IsNullOrWhiteSpace(customer.State) &&
                   !string.IsNullOrWhiteSpace(customer.Pincode) &&
                   !string.IsNullOrWhiteSpace(customer.NomineeName) &&
                   !string.IsNullOrWhiteSpace(
                       customer.NomineeRelationship);
        }
    }
}
