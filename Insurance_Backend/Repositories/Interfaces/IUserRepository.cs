using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;

namespace Insurance_Backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<PaginatedResponseDTO<User>> GetAllUsers(PaginationQueryDTO query);

        Task<User?> GetUserById(int id);

        Task<User?> GetUserByEmail(string email);

        Task<User> AddUser(User user);

        Task<User?> ActivateUser(int id);

        Task<User?> DeactivateUser(int id);

        Task<bool> EmailExists(string email);
    }
}

