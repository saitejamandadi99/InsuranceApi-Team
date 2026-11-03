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
    public class ClaimStatusHistoryController : BaseController
    {
        private readonly IClaimStatusHistoryService _historyService;

        public ClaimStatusHistoryController(
            IClaimStatusHistoryService historyService)
        {
            _historyService = historyService;
        }

        // GET: api/ClaimStatusHistory
        // Admin and Officer can view all claim status histories.
        [HttpGet]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        public async Task<IActionResult> GetAllHistory(
            [FromQuery] PaginationQueryDTO query)
        {
            var histories = await _historyService.GetAllHistory(query);

            return Success(
                histories,
                "Claim status histories retrieved successfully.");
        }

        // GET: api/ClaimStatusHistory/5
        // Admin and Officer can view history by its ID.
        [HttpGet("{id:int}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        public async Task<IActionResult> GetHistoryById(int id)
        {
            var history = await _historyService.GetHistoryById(id);

            if (history == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Claim status history not found."
                });
            }

            return Success(
                history,
                "Claim status history retrieved successfully.");
        }

        // GET: api/ClaimStatusHistory/claim/5?pageNumber=1&pageSize=10
        // Admin and Officer can view history for a particular claim.
        [HttpGet("claim/{claimId:int}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Officer)}")]
        public async Task<IActionResult> GetHistoryByClaimId(
            int claimId,
            [FromQuery] PaginationQueryDTO query)
        {
            var histories = await _historyService.GetHistoryByClaimId(
                claimId,
                query);

            return Success(
                histories,
                "Claim status histories retrieved successfully.");
        }

        // GET: api/ClaimStatusHistory/my-history?pageNumber=1&pageSize=10
        // Customers can view only their own claim status history.
        [HttpGet("my-history")]
        [Authorize(Roles = nameof(Role.Customer))]
        public async Task<IActionResult> GetMyHistory(
            [FromQuery] PaginationQueryDTO query)
        {
            var userId = User.GetUserId();

            var histories = await _historyService.GetHistoryByUserId(
                userId,
                query);

            return Success(
                histories,
                "Your claim status histories retrieved successfully.");
        }
    }
}
