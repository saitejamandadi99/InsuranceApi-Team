
using AutoMapper;
using Insurance_Backend.Custom;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Insurance_Backend.Services.Interfaces;

namespace Insurance_Backend.Services.Classes
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepo,
            IMapper mapper,
            ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserResponseDTO?> ActivateUser(int id)
        {
            var result = await _userRepo.ActivateUser(id);

            if (result == null)
            {
                throw new BusinessException(
                    "User not Found",
                    StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(
                "{UserId}-{FullName} is activated",
                result.UserId,
                result.FullName);

            return _mapper.Map<UserResponseDTO>(result);
        }

        public async Task<UserResponseDTO?> DeactivateUser(int id)
        {
            var result = await _userRepo.DeactivateUser(id);

            if (result == null)
            {
                throw new BusinessException(
                    "User not Found",
                    StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(
                "{UserId}-{FullName} is Deactivated",
                result.UserId,
                result.FullName);

            return _mapper.Map<UserResponseDTO>(result);
        }

        public async Task<PaginatedResponseDTO<UserResponseDTO>> GetAllUsers(
            PaginationQueryDTO query)
        {
            var result = await _userRepo.GetAllUsers(query);

            return new PaginatedResponseDTO<UserResponseDTO>
            {
                Records = _mapper.Map<IEnumerable<UserResponseDTO>>(
                    result.Records),
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = result.TotalPages,
                IsLastPage = result.IsLastPage,
                SortBy = result.SortBy,
                SortDirection = result.SortDirection
            };
        }

        public async Task<UserResponseDTO?> GetUserById(int id)
        {
            var user = await _userRepo.GetUserById(id);

            if (user == null)
            {
                throw new BusinessException(
                    "User not Found",
                    StatusCodes.Status404NotFound);
            }

            return _mapper.Map<UserResponseDTO>(user);
        }

        public async Task<SuccessResponseDTO<string>> AddOfficer(
            RegisterRequestDTO request)
        {
            if (await _userRepo.EmailExists(request.Email))
            {
                throw new BusinessException(
                    "Email already Exists",
                    StatusCodes.Status409Conflict);
            }

            var user = _mapper.Map<User>(request);

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.Role = Role.Officer;
            user.ActiveStatus = ActiveStatus.Active;
            user.IsEmailVerified = true;
            var createdUser = await _userRepo.AddUser(user);

            _logger.LogInformation(
                "{UserId}-{FullName} added as an Officer",
                createdUser.UserId,
                createdUser.FullName);

            return new SuccessResponseDTO<string>
            {
                Success = true,
                Message = "User registered successfully",
                Data = "Registration Successful"
            };
        }

        public async Task<UserResponseDTO> ValidateCustomerUser(int userId)
        {
            var user = await _userRepo.GetUserById(userId);

            if (user == null)
            {
                throw new BusinessException(
                    "User not found.",
                    StatusCodes.Status404NotFound);
            }

            if (user.ActiveStatus != ActiveStatus.Active)
            {
                throw new BusinessException(
                    "User is inactive.",
                    StatusCodes.Status403Forbidden);
            }

            if (user.Role != Role.Customer)
            {
                throw new BusinessException(
                    "Only customers are allowed.",
                    StatusCodes.Status403Forbidden);
            }

            return _mapper.Map<UserResponseDTO>(user);
        }
    }
}
