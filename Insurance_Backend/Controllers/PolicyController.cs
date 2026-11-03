using Insurance_Backend.Config;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insurance_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolicyController : BaseController
    {
        private readonly IPolicyService _policyService;
        private readonly ICustomerProfileService _customerService;

        public PolicyController(IPolicyService policyService, ICustomerProfileService customerService)
        {
            _policyService = policyService;
            _customerService = customerService;
        }

        // Customer purchases policy
        [Authorize(Roles = nameof(Role.Customer))]
        [HttpPost("purchase")]
        public async Task<IActionResult> PurchasePolicy([FromBody] CustomerPolicyPurchaseRequestDTO request)
        {
            var policy = await _policyService.PurchasePolicy(request, User);
            return CreatedAtSuccess(policy, "policy purchased successfully", nameof(GetPolicyById), new { id = policy.PolicyId });
        }

        // Admin/Officer issues policy
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        [HttpPost("issue")]
        public async Task<IActionResult> IssuePolicy([FromBody] OfficerOrAdminPolicyIssueRequestDTO request)
        {
            var policy = await _policyService.IssuePolicy(request);
            return CreatedAtSuccess(policy, "policy issued successfully", nameof(GetPolicyById), new { id = policy.PolicyId });
        }


        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPolicyById(int id)
        {
            var policy = await _policyService.GetPolicyById(id);
            return Success(policy, "Policy retrieved  successfully");
        }


        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        [HttpGet]
        public async Task<IActionResult> ListPolicies([FromQuery] PaginationQueryDTO query)
        {
            var policies = await _policyService.ListPolicies(query);
            return Success(policies, "Policies retrieved  successfully");

        }



        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        [HttpGet("customer/{customerId:int}")]
        public async Task<IActionResult> GetPoliciesByCustomerId(int customerId)
        {
            var policies = await _policyService.GetPoliciesByCustomerId(customerId);
            return Success(policies, "Policies retrieved  successfully");

        }

        [Authorize(Roles = nameof(Role.Customer))]
        [HttpGet("my-policies")]
        public async Task<IActionResult> GetMyPolicies()
        {
            var userId = User.GetUserId();
            var customer = await _customerService.GetCustomerProfileByUserId(userId);
            var policies = await _policyService.GetPoliciesByCustomerId(customer.CustomerId);
            return Success(policies, "Policies retrieved  successfully");

        }

        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        [HttpGet("plan/{planId}")]
        public async Task<IActionResult> GetPoliciesByPlanId(int planId)
        {
            var policies = await _policyService.GetPoliciesByPlanId(planId);
            return Success(policies, "Policies retrieved  successfully");

        }

        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelPolicy(int id)
        {
            var policy = await _policyService.CancelPolicy(id);
            return Success(policy, "Policy cancelled successfully");

        }
    }
}
