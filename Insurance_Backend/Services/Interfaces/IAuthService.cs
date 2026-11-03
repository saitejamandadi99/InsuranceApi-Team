using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<SuccessResponseDTO<string>> Register(RegisterRequestDTO request);
        Task<LoginResponseDTO> Login(LoginRequestDTO request);
        Task<SuccessResponseDTO<string>> VerifyEmail(VerifyEmailRequestDTO request);

        Task<SuccessResponseDTO<string>> ResendVerification(ResendVerificationRequestDTO request);
    }

}
