using Insurance_Backend.DTO;
using Insurance_Backend.DTO.ResponseDTO;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardResponseDTO> GetAdminDashboardAsync();

    Task<OfficerDashboardResponseDTO> GetOfficerDashboardAsync();

        Task<CustomerDashboardResponseDTO> GetCustomerDashboardAsync(int userId);
    }

}
