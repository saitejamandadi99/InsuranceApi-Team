using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Exceptions;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Insurance_Backend.Services.Classes
{
    public class PolicyPlanService : IPolicyPlanService
    {
        private readonly IPolicyPlanRepository _policyPlanRepo;
        private readonly ILogger<PolicyPlanService> _logger;

        public PolicyPlanService(
            IPolicyPlanRepository policyPlanRepo,
            ILogger<PolicyPlanService> logger)
        {
            _policyPlanRepo = policyPlanRepo;
            _logger = logger;
        }

        // Add Policy Plan
        public async Task<PlanResponseDTO> AddPolicyPlan(
            PlanRequestDTO request)
        {
            // Validate coverage amount.
            if (request.CoverageAmount <= 0)
            {
                throw new BusinessException(
                    "Coverage amount must be greater than zero.",
                    StatusCodes.Status400BadRequest);
            }

            // Validate premium amount.
            if (request.PremiumAmount <= 0)
            {
                throw new BusinessException(
                    "Premium amount must be greater than zero.",
                    StatusCodes.Status400BadRequest);
            }

            // Coverage must be greater than premium.
            if (request.CoverageAmount <= request.PremiumAmount)
            {
                throw new BusinessException(
                    "Coverage amount must be greater than premium amount.",
                    StatusCodes.Status400BadRequest);
            }

            // Create the policy plan.
            var policyPlan = new PolicyPlan
            {
                ProductId = request.ProductId,
                PlanName = request.PlanName.Trim(),
                CoverageAmount = request.CoverageAmount,
                PremiumAmount = request.PremiumAmount,
                PremiumType = request.PremiumType,
                Duration = request.Duration,
                TermsAndConditions = request.TermsAndConditions.Trim(),
                ActiveStatus = ActiveStatus.Active,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            try
            {
                var result = await _policyPlanRepo.AddPolicyPlan(policyPlan);

                _logger.LogInformation(
                    "Policy plan {PlanId} - {PlanName} added successfully.",
                    result.PlanId,
                    result.PlanName);

                return result;
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessException(
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
        }

        // Get Policy Plan By ID
        public async Task<PlanResponseDTO?> GetPolicyPlanById(int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid policy plan ID.",
                    StatusCodes.Status400BadRequest);
            }

            var plan = await _policyPlanRepo.GetPolicyPlanById(id);

            if (plan == null)
            {
                throw new BusinessException(
                    "Policy plan not found.",
                    StatusCodes.Status404NotFound);
            }

            return plan;
        }

        // Get All Policy Plans
        public async Task<PaginatedResponseDTO<PlanResponseDTO>>
            GetAllPolicyPlans(PaginationQueryDTO query)
        {
            return await _policyPlanRepo.GetAllPolicyPlans(query);
        }

        // Update Policy Plan
        public async Task<PlanResponseDTO?> UpdatePolicyPlan(
            int id,
            PlanRequestDTO request)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid policy plan ID.",
                    StatusCodes.Status400BadRequest);
            }

            // Check whether the plan exists.
            bool exists = await _policyPlanRepo.PolicyPlanExists(id);

            if (!exists)
            {
                throw new BusinessException(
                    "Policy plan not found.",
                    StatusCodes.Status404NotFound);
            }

            // Validate coverage amount.
            if (request.CoverageAmount <= 0)
            {
                throw new BusinessException(
                    "Coverage amount must be greater than zero.",
                    StatusCodes.Status400BadRequest);
            }

            // Validate premium amount.
            if (request.PremiumAmount <= 0)
            {
                throw new BusinessException(
                    "Premium amount must be greater than zero.",
                    StatusCodes.Status400BadRequest);
            }

            // Coverage must be greater than premium.
            if (request.CoverageAmount <= request.PremiumAmount)
            {
                throw new BusinessException(
                    "Coverage amount must be greater than premium amount.",
                    StatusCodes.Status400BadRequest);
            }

            // Preserve the current active status.
            var existingPlan = await _policyPlanRepo.GetPolicyPlanById(id);

            if (existingPlan == null)
            {
                throw new BusinessException(
                    "Policy plan not found.",
                    StatusCodes.Status404NotFound);
            }

            var policyPlan = new PolicyPlan
            {
                PlanId = id,
                ProductId = request.ProductId,
                PlanName = request.PlanName.Trim(),
                CoverageAmount = request.CoverageAmount,
                PremiumAmount = request.PremiumAmount,
                PremiumType = request.PremiumType,
                Duration = request.Duration,
                TermsAndConditions = request.TermsAndConditions.Trim(),
                ActiveStatus = existingPlan.ActiveStatus,
                UpdatedDate = DateTime.UtcNow
            };

            try
            {
                var result = await _policyPlanRepo.UpdatePolicyPlan(policyPlan);

                if (result == null)
                {
                    throw new BusinessException(
                        "Policy plan not found.",
                        StatusCodes.Status404NotFound);
                }

                _logger.LogInformation(
                    "Policy plan {PlanId} - {PlanName} updated successfully.",
                    result.PlanId,
                    result.PlanName);

                return result;
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessException(
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
        }

        // Delete Policy Plan
        public async Task<bool> DeletePolicyPlan(int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid policy plan ID.",
                    StatusCodes.Status400BadRequest);
            }

            bool exists = await _policyPlanRepo.PolicyPlanExists(id);

            if (!exists)
            {
                throw new BusinessException(
                    "Policy plan not found.",
                    StatusCodes.Status404NotFound);
            }

            bool deleted = await _policyPlanRepo.DeletePolicyPlan(id);

            if (!deleted)
            {
                throw new BusinessException(
                    "Policy plan cannot be deleted because it is referenced by existing policies.",
                    StatusCodes.Status409Conflict);
            }

            _logger.LogInformation(
                "Policy plan {PlanId} deleted successfully.",
                id);

            return true;
        }

        // Deactivate Policy Plan
        public async Task<PlanResponseDTO?> DeactivatePolicyPlan(int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid policy plan ID.",
                    StatusCodes.Status400BadRequest);
            }

            var result = await _policyPlanRepo.DeactivatePolicyPlan(id);

            if (result == null)
            {
                throw new BusinessException(
                    "Policy plan not found.",
                    StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(
                "Policy plan {PlanId} - {PlanName} deactivated successfully.",
                result.PlanId,
                result.PlanName);

            return result;
        }
    }
}