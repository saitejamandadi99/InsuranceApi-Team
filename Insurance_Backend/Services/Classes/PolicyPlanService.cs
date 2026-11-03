

using AutoMapper;

using Insurance_Backend.Custom;

using Insurance_Backend.DTO.RequestDTO;

using Insurance_Backend.DTO.ResponseDTO;

using Insurance_Backend.Enums;

using Insurance_Backend.Models;

using Insurance_Backend.Repositories.Interfaces;

using Insurance_Backend.Services.Interfaces;

using Microsoft.AspNetCore.Http;

namespace Insurance_Backend.Services.Classes

{

    public class PolicyPlanService : IPolicyPlanService

    {

        private readonly IPolicyPlanRepository _policyPlanRepo;

        private readonly IMapper _mapper;

        private readonly ILogger<PolicyPlanService> _logger;

        public PolicyPlanService(

            IPolicyPlanRepository policyPlanRepo,

            IMapper mapper,

            ILogger<PolicyPlanService> logger)

        {

            _policyPlanRepo = policyPlanRepo;

            _mapper = mapper;

            _logger = logger;

        }

        // Add Policy Plan

        public async Task<PlanResponseDTO> AddPolicyPlan(

            PlanRequestDTO request)

        {

            ValidatePolicyPlan(request);

            await EnsureProductIsActive(request.ProductId);

            var policyPlan = _mapper.Map<PolicyPlan>(request);

            policyPlan.PlanName = request.PlanName.Trim();

            policyPlan.TermsAndConditions =

                request.TermsAndConditions.Trim();

            policyPlan.ActiveStatus = ActiveStatus.Active;

            policyPlan.CreatedDate = DateTime.UtcNow;

            policyPlan.UpdatedDate = DateTime.UtcNow;

            var result =

                await _policyPlanRepo.AddPolicyPlan(policyPlan);

            _logger.LogInformation(

                "Policy plan {PlanId} created successfully.",

                result.PlanId);

            return _mapper.Map<PlanResponseDTO>(result);

        }

        // Get Policy Plan By ID

        public async Task<PlanResponseDTO> GetPolicyPlanById(int id)

        {

            ValidateId(id);

            var plan =

                await _policyPlanRepo.GetPolicyPlanById(id);

            if (plan == null)

            {

                throw Reject(

                    "Policy plan not found.",

                    StatusCodes.Status404NotFound);

            }

            return _mapper.Map<PlanResponseDTO>(plan);

        }

        // Get All Policy Plans

        public async Task<PaginatedResponseDTO<PlanResponseDTO>>

            GetAllPolicyPlans(PaginationQueryDTO query)

        {

            var plans =

                await _policyPlanRepo.GetAllPolicyPlans(query);

            return new PaginatedResponseDTO<PlanResponseDTO>

            {

                Records = _mapper.Map<List<PlanResponseDTO>>(

                    plans.Records),

                CurrentPage = plans.CurrentPage,

                PageSize = plans.PageSize,

                TotalRecords = plans.TotalRecords,

                TotalPages = plans.TotalPages,

                IsLastPage = plans.IsLastPage,

                SortBy = plans.SortBy,

                SortDirection = plans.SortDirection

            };

        }

        // Get Policy Plans By Product ID

        public async Task<PaginatedResponseDTO<PlanResponseDTO>>

            GetPlansByProductId(

                int productId,

                PaginationQueryDTO query)

        {

            ValidateId(productId);

            var plans =

                await _policyPlanRepo.GetPlansByProductId(

                    productId,

                    query);

            return new PaginatedResponseDTO<PlanResponseDTO>

            {

                Records = _mapper.Map<List<PlanResponseDTO>>(

                    plans.Records),

                CurrentPage = plans.CurrentPage,

                PageSize = plans.PageSize,

                TotalRecords = plans.TotalRecords,

                TotalPages = plans.TotalPages,

                IsLastPage = plans.IsLastPage,

                SortBy = plans.SortBy,

                SortDirection = plans.SortDirection

            };

        }

        // Update Policy Plan

        public async Task<PlanResponseDTO> UpdatePolicyPlan(

            int id,

            PlanRequestDTO request)

        {

            ValidateId(id);

            ValidatePolicyPlan(request);

            var existingPlan =

                await _policyPlanRepo.GetPolicyPlanById(id);

            if (existingPlan == null)

            {

                throw Reject(

                    "Policy plan not found.",

                    StatusCodes.Status404NotFound);

            }

            // Check product activity if the product is changed.

            if (existingPlan.ProductId != request.ProductId)

            {

                await EnsureProductIsActive(request.ProductId);

            }

            // Preserve the existing ID, status, and creation date.

            _mapper.Map(request, existingPlan);

            existingPlan.PlanId = id;

            existingPlan.PlanName = request.PlanName.Trim();

            existingPlan.TermsAndConditions =

                request.TermsAndConditions.Trim();

            existingPlan.UpdatedDate = DateTime.UtcNow;

            var result =

                await _policyPlanRepo.UpdatePolicyPlan(existingPlan);

            if (result == null)

            {

                throw Reject(

                    "Policy plan not found.",

                    StatusCodes.Status404NotFound);

            }

            _logger.LogInformation(

                "Policy plan {PlanId} updated successfully.",

                result.PlanId);

            return _mapper.Map<PlanResponseDTO>(result);

        }

        // Activate Policy Plan

        public async Task<PlanResponseDTO> ActivatePolicyPlan(int id)

        {

            ValidateId(id);

            var existingPlan =

                await _policyPlanRepo.GetPolicyPlanById(id);

            if (existingPlan == null)

            {

                throw Reject(

                    "Policy plan not found.",

                    StatusCodes.Status404NotFound);

            }

            if (existingPlan.ActiveStatus == ActiveStatus.Active)

            {

                return _mapper.Map<PlanResponseDTO>(existingPlan);

            }

            var result =

                await _policyPlanRepo.ActivatePolicyPlan(id);

            if (result == null)

            {

                throw Reject(

                    "Policy plan not found.",

                    StatusCodes.Status404NotFound);

            }

            _logger.LogInformation(

                "Policy plan {PlanId} activated successfully.",

                result.PlanId);

            return _mapper.Map<PlanResponseDTO>(result);

        }

        // Deactivate Policy Plan

        public async Task<PlanResponseDTO> DeactivatePolicyPlan(int id)

        {

            ValidateId(id);

            var existingPlan =

                await _policyPlanRepo.GetPolicyPlanById(id);

            if (existingPlan == null)

            {

                throw Reject(

                    "Policy plan not found.",

                    StatusCodes.Status404NotFound);

            }

            if (existingPlan.ActiveStatus == ActiveStatus.Inactive)

            {

                return _mapper.Map<PlanResponseDTO>(existingPlan);

            }

            var result =

                await _policyPlanRepo.DeactivatePolicyPlan(id);

            if (result == null)

            {

                throw Reject(

                    "Policy plan not found.",

                    StatusCodes.Status404NotFound);

            }

            _logger.LogInformation(

                "Policy plan {PlanId} deactivated successfully.",

                result.PlanId);

            return _mapper.Map<PlanResponseDTO>(result);

        }

        // Validate Policy Plan Request

        private void ValidatePolicyPlan(PlanRequestDTO? request)

        {

            if (request == null)

            {

                throw Reject(

                    "Invalid policy plan request.",

                    StatusCodes.Status400BadRequest);

            }

            if (request.ProductId <= 0)

            {

                throw Reject(

                    "A valid product ID is required.",

                    StatusCodes.Status400BadRequest);

            }

            if (string.IsNullOrWhiteSpace(request.PlanName) ||

                request.PlanName.Trim().Length < 3 ||

                request.PlanName.Trim().Length > 100)

            {

                throw Reject(

                    "Plan name must contain between 3 and 100 characters.",

                    StatusCodes.Status400BadRequest);

            }

            if (request.CoverageAmount <= 0)

            {

                throw Reject(

                    "Coverage amount must be greater than zero.",

                    StatusCodes.Status400BadRequest);

            }

            if (request.PremiumAmount <= 0)

            {

                throw Reject(

                    "Premium amount must be greater than zero.",

                    StatusCodes.Status400BadRequest);

            }

            if (request.CoverageAmount <= request.PremiumAmount)

            {

                throw Reject(

                    "Coverage amount must be greater than premium amount.",

                    StatusCodes.Status400BadRequest);

            }

            if (!Enum.IsDefined(

                    typeof(PremiumType),

                    request.PremiumType))

            {

                throw Reject(

                    "Invalid premium type.",

                    StatusCodes.Status400BadRequest);

            }

            if (request.Duration < 1 || request.Duration > 100)

            {

                throw Reject(

                    "Duration must be between 1 and 100.",

                    StatusCodes.Status400BadRequest);

            }

            if (string.IsNullOrWhiteSpace(request.TermsAndConditions) ||

                request.TermsAndConditions.Trim().Length < 3 ||

                request.TermsAndConditions.Trim().Length > 300)

            {

                throw Reject(

                    "Terms and conditions must contain between 3 and 300 characters.",

                    StatusCodes.Status400BadRequest);

            }

        }

        // Validate Policy Plan ID

        private void ValidateId(int id)

        {

            if (id <= 0)

            {

                throw Reject(

                    "Invalid policy plan ID.",

                    StatusCodes.Status400BadRequest);

            }

        }

        // Ensure the product exists and is active.

        private async Task EnsureProductIsActive(int productId)

        {

            bool isActive =

                await _policyPlanRepo.IsProductActive(productId);

            if (!isActive)

            {

                throw Reject(

                    "Product not found or is inactive.",

                    StatusCodes.Status400BadRequest);

            }

        }

        // Log and create a business exception.

        private BusinessException Reject(

            string message,

            int statusCode)

        {

            _logger.LogWarning(

                "Policy plan rule violation: {Message}",

                message);

            return new BusinessException(message, statusCode);

        }

    }

}

