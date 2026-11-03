using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insurance_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PolicyPlanController : BaseController
    {
        private readonly IPolicyPlanService _policyPlanService;

        public PolicyPlanController(
            IPolicyPlanService policyPlanService)
        {
            _policyPlanService = policyPlanService;
        }

        // GET: api/plan/getAllPlans
        // Retrieve all policy plans.
        [HttpGet("getAllPlans")]
        public async Task<IActionResult> GetAllPolicyPlans(
            [FromQuery] PaginationQueryDTO query)
        {
            var plans =
                await _policyPlanService.GetAllPolicyPlans(query);

            return Success(
                plans,
                "Policy plans retrieved successfully.");
        }

        // GET: api/plan/5
        // Retrieve a policy plan by ID.
        [HttpGet("{planId:int}")]
        public async Task<IActionResult> GetPolicyPlanById(int planId)
        {
            var plan =
                await _policyPlanService.GetPolicyPlanById(planId);

            if (plan == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Policy plan not found."
                });
            }

            return Success(
                plan,
                "Policy plan retrieved successfully.");
        }

        // GET: api/plan/product/5
        // Retrieve plans belonging to a product.
        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetPlansByProductId(
            int productId,
            [FromQuery] PaginationQueryDTO query)
        {
            var plans =
                await _policyPlanService.GetPlansByProductId(
                    productId,
                    query);

            return Success(
                plans,
                "Product policy plans retrieved successfully.");
        }

        // POST: api/plan/createPlan
        // Only Admin can create a plan.
        [HttpPost("createPlan")]
        [Authorize(Roles = nameof(Role.Admin))]
        public async Task<IActionResult> AddPolicyPlan(
            [FromBody] PlanRequestDTO request)
        {
            var plan =
                await _policyPlanService.AddPolicyPlan(request);

            return CreatedAtSuccess(
                plan,
                "Policy plan created successfully.",
                nameof(GetPolicyPlanById),
                new { planId = plan.PlanId });
        }

        // PUT: api/plan/5
        // Only Admin can update a plan.
        [HttpPut("{planId:int}")]
        [Authorize(Roles = nameof(Role.Admin))]
        public async Task<IActionResult> UpdatePolicyPlan(
            int planId,
            [FromBody] PlanRequestDTO request)
        {
            var plan =
                await _policyPlanService.UpdatePolicyPlan(
                    planId,
                    request);

            return Success(
                plan,
                "Policy plan updated successfully.");
        }

        // PUT: api/plan/activate/5
        // Only Admin can activate a plan.
        [HttpPut("activate/{planId:int}")]
        [Authorize(Roles = nameof(Role.Admin))]
        public async Task<IActionResult> ActivatePolicyPlan(int planId)
        {
            var plan =
                await _policyPlanService.ActivatePolicyPlan(planId);

            return Success(
                plan,
                "Policy plan activated successfully.");
        }

        // PUT: api/plan/deactivate/5
        // Only Admin can deactivate a plan.
        [HttpPut("deactivate/{planId:int}")]
        [Authorize(Roles = nameof(Role.Admin))]
        public async Task<IActionResult> DeactivatePolicyPlan(int planId)
        {
            var plan =
                await _policyPlanService.DeactivatePolicyPlan(planId);

            return Success(
                plan,
                "Policy plan deactivated successfully.");
        }
    }
}
