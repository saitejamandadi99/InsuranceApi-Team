using Insurance_Backend.Config;
using Insurance_Backend.Enums;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insurance_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: api/Dashboard/admin
        [HttpGet("admin")]
        [Authorize(Roles = nameof(Role.Admin))]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var dashboard =
                await _dashboardService.GetAdminDashboardAsync();

            return Success(
                dashboard,
                "Admin dashboard retrieved successfully");
        }

        // GET: api/Dashboard/officer
        [HttpGet("officer")]
        [Authorize(Roles = nameof(Role.Officer))]
        public async Task<IActionResult> GetOfficerDashboard()
        {
            var dashboard =
                await _dashboardService.GetOfficerDashboardAsync();

            return Success(
                dashboard,
                "Officer dashboard retrieved successfully");
        }

        // GET: api/Dashboard/customer
        [HttpGet("customer")]
        [Authorize(Roles = nameof(Role.Customer))]
        public async Task<IActionResult> GetCustomerDashboard()
        {
            var userId = User.GetUserId();

            var dashboard =
                await _dashboardService.GetCustomerDashboardAsync(userId);

            return Success(
                dashboard,
                "Customer dashboard retrieved successfully");
        }
    }
}
