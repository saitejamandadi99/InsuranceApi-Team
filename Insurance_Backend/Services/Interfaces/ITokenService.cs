using Insurance_Backend.Models;

namespace Insurance_Backend.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
