using System.Security.Claims;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;

namespace Insurance_Backend.Services.Interfaces
{
    public interface ICustomerProfileService
    {
        Task<PaginatedResponseDTO<CustomerResponseDTO>> GetAllCustomerProfiles(
            PaginationQueryDTO query);

        Task<CustomerResponseDTO?> GetCustomerProfileById(int id);

        Task<CustomerResponseDTO?> GetCustomerProfileByUserId(int userId);

        Task<CustomerResponseDTO> CreateCustomerProfile(
            CustomerRequestDTO request,
            ClaimsPrincipal user);

        Task<CustomerResponseDTO?> UpdateCustomerProfile(
            int id,
            CustomerRequestDTO request,
            ClaimsPrincipal user);

        Task<bool> DeleteCustomerProfile(
            int id,
            ClaimsPrincipal user);
    }
}