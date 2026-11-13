using AutoMapper;

using Insurance_Backend.Config;

using Insurance_Backend.Custom;

using Insurance_Backend.DTO.RequestDTO;

using Insurance_Backend.DTO.ResponseDTO;

using Insurance_Backend.Enums;

using Insurance_Backend.Models;

using Insurance_Backend.Repositories.Interfaces;

using Insurance_Backend.Services.Interfaces;

using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Options;

using System.Security.Cryptography;

using System.Text;

using System.Text.RegularExpressions;

namespace Insurance_Backend.Services.Classes

{

    public class AuthService : IAuthService

    {

        private readonly IUserRepository _userRepo;

        private readonly ITokenService _tokenService;

        private readonly IEmailService _emailService;

        private readonly IMapper _mapper;

        private readonly JWTSettings _jwtSettings;

        private readonly ILogger<AuthService> _logger;

        public AuthService(

            IUserRepository userRepo,

            ITokenService tokenService,

            IEmailService emailService,

            IMapper mapper,

            IOptions<JWTSettings> options,

            ILogger<AuthService> logger)

        {

            _userRepo = userRepo;

            _tokenService = tokenService;

            _emailService = emailService;

            _mapper = mapper;

            _jwtSettings = options.Value;

            _logger = logger;

        }

        // REGISTER CUSTOMER

        public async Task<SuccessResponseDTO<string>> Register(

            RegisterRequestDTO request)

        {

            var email = request.Email.Trim();

            var emailExists = await _userRepo.EmailExists(email);

            if (emailExists)

            {

                throw new BusinessException(

                    "Email already exists.",

                    StatusCodes.Status409Conflict);

            }

            var user = _mapper.Map<User>(request);

            user.Email = email;

            user.Password =

                BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Public registration always creates a Customer.

            user.Role = Role.Customer;

            user.ActiveStatus = ActiveStatus.Active;

            // Email verification is required before customer login.

            user.IsEmailVerified = false;

            var otp = GenerateOtp();

            // Store only the OTP hash.

            user.EmailVerificationCodeHash = HashOtp(otp);

            // OTP expires after 10 minutes.

            user.EmailVerificationCodeExpiry =

                DateTime.UtcNow.AddMinutes(10);

            // Save customer before attempting email delivery.

            await _userRepo.AddUser(user);

            try
            {
                string emailSubject = "Welcome to Insurance Management System";


                string emailBody = $@"Hello {user.FullName}!  
                    Welcome to Insurance Management System.
                    Thank you for registering with us.
                    Your email verification OTP is: {otp}
                    This OTP is valid for 10 minutes.
                    Please do not share it with anyone.
                    Best regards,
                    Insurance Management Team";

                await _emailService.SendEmailAsync(user.Email,

                    emailSubject,

                    emailBody);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration email delivery failed for user {UserId}.", user.UserId);


                throw new BusinessException("Registration was saved, but the verification email could not be sent. Please request a new verification code.",

                    StatusCodes.Status503ServiceUnavailable);

            }

            _logger.LogInformation("Customer registered. Email verification is pending.");


            return new SuccessResponseDTO<string>
            {
                Success = true,

                Message = "Registration successful. Please verify your email.",

                Data = "A verification code has been generated."
            };
        }

        // VERIFY EMAIL USING OTP ONLY

        public async Task<SuccessResponseDTO<string>> VerifyEmail(

    VerifyEmailRequestDTO request)

        {

            if (request == null ||

                string.IsNullOrWhiteSpace(request.Email) ||

                string.IsNullOrWhiteSpace(request.OTP))

            {

                throw new BusinessException(

                    "Email and verification code are required.",

                    StatusCodes.Status400BadRequest);

            }

            if (!Regex.IsMatch(request.OTP, @"^\d{6}$"))

            {

                throw new BusinessException(

                    "OTP must contain exactly six digits.",

                    StatusCodes.Status400BadRequest);

            }

            // 1. Fetch the user by email.

            var email = request.Email.Trim();

            var user = await _userRepo.GetUserByEmail(email);

            if (user == null || user.Role != Role.Customer)

            {

                throw new BusinessException(

                    "Invalid email or verification code.",

                    StatusCodes.Status400BadRequest);

            }

            // 2. Check whether the email is already verified.

            if (user.IsEmailVerified)

            {

                throw new BusinessException(

                    "Email is already verified.",

                    StatusCodes.Status400BadRequest);

            }

            // 3. Check whether a verification code exists.

            if (string.IsNullOrWhiteSpace(user.EmailVerificationCodeHash) ||

                user.EmailVerificationCodeExpiry == null)

            {

                throw new BusinessException(

                    "No valid verification code exists. Please request a new code.",

                    StatusCodes.Status400BadRequest);

            }

            // 4. Check whether the code has expired.

            if (user.EmailVerificationCodeExpiry.Value <= DateTime.UtcNow)

            {

                throw new BusinessException(

                    "Verification code has expired. Please request a new code.",

                    StatusCodes.Status400BadRequest);

            }

            // 5. Compare the submitted OTP hash with the stored hash.

            var suppliedHash = HashOtp(request.OTP);

            var storedHash = Convert.FromHexString(

                user.EmailVerificationCodeHash);

            var providedHash = Convert.FromHexString(suppliedHash);

            if (!CryptographicOperations.FixedTimeEquals(

                    providedHash, storedHash))

            {

                throw new BusinessException(

                    "Invalid email or verification code.",

                    StatusCodes.Status400BadRequest);

            }

            // 6. Mark the customer's email as verified.

            user.IsEmailVerified = true;

            // 7. Clear the OTP to prevent reuse.

            user.EmailVerificationCodeHash = null;

            user.EmailVerificationCodeExpiry = null;

            await _userRepo.UpdateUser(user);

            _logger.LogInformation(

                "Email verification completed for user {UserId}.",

                user.UserId);

            return new SuccessResponseDTO<string>

            {

                Success = true,

                Message = "Email verified successfully.",

                Data = "You can now login."

            };

        }

        // RESEND VERIFICATION CODE

        public async Task<SuccessResponseDTO<string>> ResendVerification(

            ResendVerificationRequestDTO request)

        {

            var email = request.Email.Trim();

            var user = await _userRepo.GetUserByEmail(email);

            if (user == null || user.Role != Role.Customer)

            {

                throw new BusinessException(

                    "Invalid email or email Does not exists .",

                    StatusCodes.Status400BadRequest);

            }

            if (user.IsEmailVerified)

            {

                throw new BusinessException(

                    "Email is already verified.",

                    StatusCodes.Status400BadRequest);

            }

            var otp = GenerateOtp();

            // Generate a new OTP and invalidate the previous one.

            user.EmailVerificationCodeHash = HashOtp(otp);

            user.EmailVerificationCodeExpiry =

                DateTime.UtcNow.AddMinutes(10);

            await _userRepo.UpdateUser(user);

            try
            {
                string emailSubject = "Your New OTP - Insurance Management System";


                string emailBody = $@"Hello {user.FullName}!
                    Here is your new email verification OTP: {otp}
                    This OTP is valid for 10 minutes.
                    Please do not share it with anyone. 
                    Best regards, 
                    Insurance Management Team";

                await _emailService.SendEmailAsync(user.Email,

                    emailSubject,

                    emailBody);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,

                    "Unable to resend verification email for user {UserId}.",

                    user.UserId);


                throw new BusinessException("Unable to send the verification email. Please try again later.",

                    StatusCodes.Status503ServiceUnavailable);

            }
            return new SuccessResponseDTO<string>
            {
                Success = true,

                Message = "Verification code sent successfully.",

                Data = "A new verification code has been generated."
            };
        }

        // LOGIN

        public async Task<LoginResponseDTO> Login(

            LoginRequestDTO request)

        {

            var user = await _userRepo.GetUserByEmail(

                request.Email.Trim());

            if (user == null)

            {

                throw new UnauthorizedAccessException(

                    "Invalid email or password.");

            }

            var passwordMatched = BCrypt.Net.BCrypt.Verify(

                request.Password,

                user.Password);

            if (!passwordMatched)

            {

                throw new UnauthorizedAccessException(

                    "Invalid email or password.");

            }

            if (user.ActiveStatus == ActiveStatus.Inactive)

            {

                throw new BusinessException(

                    "User is not active.",

                    StatusCodes.Status403Forbidden);

            }

            // Email verification is mandatory for Customers only.

            if (user.Role == Role.Customer &&

                !user.IsEmailVerified)

            {

                throw new BusinessException(

                    "Please verify your email before logging in.",

                    StatusCodes.Status403Forbidden);

            }

            _logger.LogInformation(

                "User logged in successfully.");

            return new LoginResponseDTO

            {

                JwtToken = _tokenService.GenerateToken(user),

                UserEmail = user.Email,

                UserRole = user.Role.ToString(),

                ExpiresAt = DateTime.UtcNow.AddMinutes(

                    _jwtSettings.ExpiryMinutes)

            };

        }

        // GENERATE A RANDOM SIX-DIGIT OTP

        private static string GenerateOtp()

        {

            return RandomNumberGenerator

                .GetInt32(100000, 1000000)

                .ToString();

        }

        // HASH THE OTP BEFORE STORING IT

        private static string HashOtp(string otp)

        {

            var bytes = Encoding.UTF8.GetBytes(otp);

            var hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash);

        }
        // FORGOT PASSWORD

        public async Task<SuccessResponseDTO<string>> ForgotPassword(
            ForgotPasswordRequestDTO request)
        {
            var email = request.Email.Trim();

            var user = await _userRepo.GetUserByEmail(email);

            // Do not reveal whether the email exists.
            if (user == null)
            {
                return new SuccessResponseDTO<string>
                {
                    Success = true,
                    Message = "If the email is registered, a password reset OTP will be sent.",
                    Data = "Please check your email."
                };
            }

            var otp = GenerateOtp();

            user.PasswordResetOtpHash = HashOtp(otp);

            // OTP expires after 5 minutes.
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(5);

            await _userRepo.UpdateUser(user);

            try
            {
                string emailSubject = "Password Reset OTP - Insurance Management System";

                string emailBody = $@"
            <h3>Hello {System.Net.WebUtility.HtmlEncode(user.FullName)}!</h3>
            <p>We received a request to reset your password.</p>
            <p>Your password reset OTP is:</p>
            <h2><b>{otp}</b></h2>
            <p>This OTP is valid for 5 minutes.</p>
            <p>Please do not share this OTP with anyone.</p>
            <br/>
            <p>Best regards,</p>
            <p>Insurance Management Team</p>";

                await _emailService.SendEmailAsync(
                    user.Email,
                    emailSubject,
                    emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Password reset email delivery failed for user {UserId}.",
                    user.UserId);

                user.PasswordResetOtpHash = null;
                user.PasswordResetOtpExpiry = null;

                await _userRepo.UpdateUser(user);

                throw new BusinessException(
                    "Unable to send password reset OTP. Please try again later.",
                    StatusCodes.Status503ServiceUnavailable);
            }

            return new SuccessResponseDTO<string>
            {
                Success = true,
                Message = "If the email is registered, a password reset OTP will be sent.",
                Data = "Please check your email."
            };
        }
        // RESET PASSWORD

        public async Task<SuccessResponseDTO<string>> ResetPassword(
            ResetPasswordRequestDTO request)
        {
            var email = request.Email.Trim();

            var user = await _userRepo.GetUserByEmail(email);

            if (user == null)
            {
                throw new BusinessException(
                    "Invalid email or OTP.",
                    StatusCodes.Status400BadRequest);
            }

            // Check whether OTP exists.
            if (string.IsNullOrWhiteSpace(user.PasswordResetOtpHash) ||
                user.PasswordResetOtpExpiry == null)
            {
                throw new BusinessException(
                    "Please request a new password reset OTP.",
                    StatusCodes.Status400BadRequest);
            }

            // Check OTP expiry.
            if (user.PasswordResetOtpExpiry.Value <= DateTime.UtcNow)
            {
                throw new BusinessException(
                    "OTP has expired. Please request a new OTP.",
                    StatusCodes.Status400BadRequest);
            }

            // Compare OTP hashes.
            var suppliedHash = HashOtp(request.OTP);

            var storedHash = Convert.FromHexString(
                user.PasswordResetOtpHash);

            var providedHash = Convert.FromHexString(suppliedHash);

            if (!CryptographicOperations.FixedTimeEquals(
                providedHash, storedHash))
            {
                throw new BusinessException(
                    "Invalid email or OTP.",
                    StatusCodes.Status400BadRequest);
            }

            // Hash the new password.
            user.Password = BCrypt.Net.BCrypt.HashPassword(
                request.NewPassword);

            // Clear OTP after successful password reset.
            user.PasswordResetOtpHash = null;

            user.PasswordResetOtpExpiry = null;

            user.UpdatedDate = DateTime.UtcNow;

            await _userRepo.UpdateUser(user);

            _logger.LogInformation(
                "Password reset successfully for user {UserId}.",
                user.UserId);

            return new SuccessResponseDTO<string>
            {
                Success = true,
                Message = "Password reset successfully.",
                Data = "You can now login using your new password."
            };
        }
    }

}

