using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Insurance_Backend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authServie;
        public AuthController(IAuthService authServie)
        {
            _authServie = authServie;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDTO request)
        {
            var response = await _authServie.Register(request);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            var response = await _authServie.Login(request);
            return Ok(response);
        }
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDTO request)
        {
            var response = await _authServie.VerifyEmail(request);


            return Ok(response);

        }
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequestDTO request)
        {
            var response = await _authServie.ResendVerification(request);


            return Ok(response);
        }
        // FORGOT PASSWORD

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordRequestDTO request)
        {
            var response = await _authServie.ForgotPassword(request);

            return Ok(response);
        }

        // RESET PASSWORD

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequestDTO request)
        {
            var response = await _authServie.ResetPassword(request);

            return Ok(response);
        }
    }
}
