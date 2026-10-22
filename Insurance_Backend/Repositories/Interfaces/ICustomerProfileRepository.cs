using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface ICustomerProfileRepository
    {
        Task<CustomerProfile> AddCustomerProfile(
            CustomerProfile customerProfile);

        Task<CustomerProfile?> GetCustomerProfileById(int id);

        Task<CustomerProfile?> GetCustomerProfileByUserId(int userId);

        Task<PaginatedResponseDTO<CustomerProfile>> GetAllCustomerProfiles(
            PaginationQueryDTO query);

        Task<CustomerProfile?> UpdateCustomerProfile(
            CustomerProfile customerProfile);

        Task<bool> IsProfileOwnedByUser(int customerId, int userId);

        Task<bool> DeleteCustomerProfile(int id);

        Task<bool> UserProfileExists(int userId);
    }
}