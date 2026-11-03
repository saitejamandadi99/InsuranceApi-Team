using Insurance_Backend.Custom;
using Insurance_Backend.DTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Repositories.Interfaces;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Insurance_Backend.Services.Classes
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepo;
        private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IDashboardRepository dashboardRepo,
        ILogger<DashboardService> logger)
        {
            _dashboardRepo = dashboardRepo;
            _logger = logger;
        }

        // Admin Dashboard
        public async Task<AdminDashboardResponseDTO>
            GetAdminDashboardAsync()
        {
            var dashboard =
                await _dashboardRepo.GetAdminDashboardAsync();

            _logger.LogInformation(
                "Admin dashboard data retrieved successfully.");

            return dashboard;
        }

        // Officer Dashboard
        public async Task<OfficerDashboardResponseDTO>
            GetOfficerDashboardAsync()
        {
            var dashboard =
                await _dashboardRepo.GetOfficerDashboardAsync();

            _logger.LogInformation(
                "Officer dashboard data retrieved successfully.");

            return dashboard;
        }

        // Customer Dashboard
        public async Task<CustomerDashboardResponseDTO>
            GetCustomerDashboardAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new BusinessException(
                    "Invalid user ID.",
                    StatusCodes.Status400BadRequest);
            }

            try
            {
                var dashboard =
                    await _dashboardRepo.GetCustomerDashboardAsync(userId);

                _logger.LogInformation(
                    "Customer dashboard retrieved for UserId {UserId}.",
                    userId);

                return dashboard;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(
                    "Customer profile not found for UserId {UserId}.",
                    userId);

                throw new BusinessException(
                    ex.Message,
                    StatusCodes.Status404NotFound);
            }
        }
    }

}
