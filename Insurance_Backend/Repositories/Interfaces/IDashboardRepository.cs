using Insurance_Backend.DTO;
using Insurance_Backend.DTO.ResponseDTO;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<AdminDashboardResponseDTO> GetAdminDashboardAsync();
    Task<OfficerDashboardResponseDTO> GetOfficerDashboardAsync();
        Task<CustomerDashboardResponseDTO> GetCustomerDashboardAsync(
            int userId);
    }

}
