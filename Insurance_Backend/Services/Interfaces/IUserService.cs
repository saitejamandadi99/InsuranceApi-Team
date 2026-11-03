using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IUserService
    {
        Task<PaginatedResponseDTO<UserResponseDTO>> GetAllUsers(
             PaginationQueryDTO query);

        Task<UserResponseDTO?> GetUserById(int id);

        Task<SuccessResponseDTO<string>> AddOfficer(
            RegisterRequestDTO request);

        Task<UserResponseDTO?> ActivateUser(int id);

        Task<UserResponseDTO?> DeactivateUser(int id);

        Task<UserResponseDTO> ValidateCustomerUser(int userId);
    }
}
