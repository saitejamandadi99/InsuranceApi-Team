using System.Security.Claims;
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

namespace Insurance_Backend.Services.Classes
{
    public class CustomerProfileService : ICustomerProfileService
    {
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly ILogger<CustomerProfileService> _logger;

        public CustomerProfileService(
            ICustomerProfileRepository customerProfileRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IImageService imageService,
            ILogger<CustomerProfileService> logger)
        {
            _customerProfileRepository = customerProfileRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task<PaginatedResponseDTO<CustomerResponseDTO>>
            GetAllCustomerProfiles(PaginationQueryDTO query)
        {
            var result =
                await _customerProfileRepository.GetAllCustomerProfiles(query);

            var records = new List<CustomerResponseDTO>();

            foreach (var profile in result.Records)
            {
                var user = await _userRepository.GetUserById(profile.UserId);

                profile.User = user;

                records.Add(_mapper.Map<CustomerResponseDTO>(profile));
            }

            return new PaginatedResponseDTO<CustomerResponseDTO>
            {
                Records = records,
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = result.TotalPages,
                IsLastPage = result.IsLastPage,
                SortBy = result.SortBy,
                SortDirection = result.SortDirection
            };
        }

        public async Task<CustomerResponseDTO?> GetCustomerProfileById(int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid customer profile ID.",
                    StatusCodes.Status400BadRequest);
            }

            var profile =
                await _customerProfileRepository.GetCustomerProfileById(id);

            if (profile == null)
            {
                throw new BusinessException(
                    "Customer profile not found.",
                    StatusCodes.Status404NotFound);
            }

            var user =
                await _userRepository.GetUserById(profile.UserId);

            if (user == null)
            {
                throw new BusinessException(
                    "Associated customer account not found.",
                    StatusCodes.Status404NotFound);
            }

            profile.User = user;

            return _mapper.Map<CustomerResponseDTO>(profile);
        }

        public async Task<CustomerResponseDTO?> GetCustomerProfileByUserId(
            int userId)
        {
            if (userId <= 0)
            {
                throw new BusinessException(
                    "Invalid user ID.",
                    StatusCodes.Status400BadRequest);
            }

            var profile =
                await _customerProfileRepository
                    .GetCustomerProfileByUserId(userId);

            if (profile == null)
            {
                throw new BusinessException(
                    "Customer profile not found.",
                    StatusCodes.Status404NotFound);
            }

            var user =
                await _userRepository.GetUserById(profile.UserId);

            if (user == null)
            {
                throw new BusinessException(
                    "Associated customer account not found.",
                    StatusCodes.Status404NotFound);
            }

            profile.User = user;

            return _mapper.Map<CustomerResponseDTO>(profile);
        }


        public async Task<CustomerResponseDTO> CreateCustomerProfile(
CustomerRequestDTO request,
ClaimsPrincipal user)
        {
            if (request == null)
            {
                throw new BusinessException(
                    "Customer profile request is required.",
                    StatusCodes.Status400BadRequest);
            }
            if (user?.Identity?.IsAuthenticated != true)
            {
                throw new BusinessException(
                    "Authentication is required.",
                    StatusCodes.Status401Unauthorized);
            }
            if (!user.IsInRole(Role.Customer.ToString()))
            {
                throw new BusinessException(
                    "Only customers can create their own profiles.",
                    StatusCodes.Status403Forbidden);
            }
            int userId = user.GetUserId();
            var customerUser =
                await _userRepository.GetUserById(userId);
            if (customerUser == null)
            {
                throw new BusinessException(
                    "Customer account not found.",
                    StatusCodes.Status404NotFound);
            }
            if (customerUser.Role != Role.Customer)
            {
                throw new BusinessException(
                    "Only customers can create customer profiles.",
                    StatusCodes.Status403Forbidden);
            }
            if (customerUser.ActiveStatus != ActiveStatus.Active)
            {
                throw new BusinessException(
                    "Inactive users cannot create customer profiles.",
                    StatusCodes.Status403Forbidden);
            }
            bool profileExists =
                await _customerProfileRepository.UserProfileExists(userId);
            if (profileExists)
            {
                throw new BusinessException(
                    "Customer profile already exists.",
                    StatusCodes.Status409Conflict);
            }

            // Validate customer age
            ValidateCustomerAge(request.DateOfBirth);

            var profile = _mapper.Map<CustomerProfile>(request);
            profile.UserId = userId;
            profile.User = null;
            profile.CreatedDate = DateTime.UtcNow;
            profile.UpdatedDate = DateTime.UtcNow;
            if (request.ProfileImage != null)
            {
                profile.ProfileImageUrl =
                    await UploadProfileImage(request.ProfileImage);
            }
            var createdProfile =
                await _customerProfileRepository.AddCustomerProfile(profile);
            createdProfile.User = customerUser;
            _logger.LogInformation(
                "Customer profile {CustomerId} created for UserId {UserId}.",
                createdProfile.CustomerId,
                userId);
            return _mapper.Map<CustomerResponseDTO>(createdProfile);
        }

        private static void ValidateCustomerAge(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Reject dates in the future.
            if (dateOfBirth > today)
            {
                throw new BusinessException(
                    "Date of birth cannot be in the future.",
                    StatusCodes.Status400BadRequest);
            }

            // Calculate the customer's actual age.
            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth > today.AddYears(-age))
            {
                age--;
            }

            // Only customers aged 18 or older are permitted.
            if (age < 18)
            {
                throw new BusinessException(
                    "Only customers aged 18 years or older can register a profile.",
                    StatusCodes.Status400BadRequest);
            }
        }


        public async Task<CustomerResponseDTO?> UpdateCustomerProfile(
            int id,
            CustomerRequestDTO request,
            ClaimsPrincipal user)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid customer profile ID.",
                    StatusCodes.Status400BadRequest);
            }

            if (request == null)
            {
                throw new BusinessException(
                    "Customer profile request is required.",
                    StatusCodes.Status400BadRequest);
            }

            if (!user.IsInRole(Role.Customer.ToString()))
            {
                throw new BusinessException(
                    "Only customers can update their own profiles.",
                    StatusCodes.Status403Forbidden);
            }

            int userId = user.GetUserId();

            var existingProfile =
                await _customerProfileRepository.GetCustomerProfileByUserId(userId);

            if (existingProfile == null)
            {
                throw new BusinessException(
                    "Customer profile not found.",
                    StatusCodes.Status404NotFound);
            }

            if (existingProfile.UserId != userId)
            {
                throw new BusinessException(
                    "You are not authorized to update this profile.",
                    StatusCodes.Status403Forbidden);
            }

            var customerUser =
                await _userRepository.GetUserById(userId);

            if (customerUser == null)
            {
                throw new BusinessException(
                    "Customer account not found.",
                    StatusCodes.Status404NotFound);
            }

            if (customerUser.Role != Role.Customer ||
                customerUser.ActiveStatus != ActiveStatus.Active)
            {
                throw new BusinessException(
                    "Your account is not permitted to update this profile.",
                    StatusCodes.Status403Forbidden);
            }
            // Validate customer age
            ValidateCustomerAge(request.DateOfBirth);
            var profile = _mapper.Map<CustomerProfile>(request);

            profile.CustomerId = existingProfile.CustomerId;
            profile.UserId = existingProfile.UserId;
            profile.User = null;
            profile.CreatedDate = existingProfile.CreatedDate;
            profile.UpdatedDate = DateTime.UtcNow;
            profile.ProfileImageUrl = existingProfile.ProfileImageUrl;

            if (request.ProfileImage != null)
            {
                profile.ProfileImageUrl =
                    await UploadProfileImage(request.ProfileImage);
            }

            var updatedProfile =
                await _customerProfileRepository.UpdateCustomerProfile(profile);

            if (updatedProfile == null)
            {
                throw new BusinessException(
                    "Customer profile could not be updated.",
                    StatusCodes.Status404NotFound);
            }

            updatedProfile.User = customerUser;

            _logger.LogInformation(
                "Customer profile {CustomerId} updated by UserId {UserId}.",
                id,
                userId);

            return _mapper.Map<CustomerResponseDTO>(updatedProfile);
        }

        

        private async Task<string> UploadProfileImage(IFormFile file)
        {
            try
            {
                return await _imageService.UploadImageAsync(file);
            }
            catch (ArgumentException ex)
            {
                throw new BusinessException(
                    ex.Message,
                    StatusCodes.Status400BadRequest);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to upload customer profile image.");

                throw new BusinessException(
                    "Unable to upload the profile image.",
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
