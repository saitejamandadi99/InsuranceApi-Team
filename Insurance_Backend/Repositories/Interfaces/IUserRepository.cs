using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<PaginatedResponseDTO<UserResponseDTO>> GetAllUsers( PaginationQueryDTO query);
        Task<User?> GetUserById(int id);
        Task<User?> GetUserByEmail(string email);
        Task<UserResponseDTO> AddUser(User user);
        Task<UserResponseDTO?> ActivateUser(int id);
        Task<UserResponseDTO?> DeactivateUser(int id);
        Task<bool> EmailExists(string email);
    }
}

