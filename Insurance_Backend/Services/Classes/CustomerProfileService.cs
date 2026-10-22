using System.Security.Claims;
using AutoMapper;
using Insurance_Backend.Exceptions;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.DTO.ResponseDTO;
using Insurance_Backend.Models;
using Insurance_Backend.Repositories.Interfaces;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Insurance_Backend.Services.Classes
{
    public class CustomerProfileService : ICustomerProfileService
    {
        private readonly ICustomerProfileRepository _customerRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerProfileService> _logger;

    public CustomerProfileService(
        ICustomerProfileRepository customerRepo,
        IMapper mapper,
        ILogger<CustomerProfileService> logger)
        {
            _customerRepo = customerRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaginatedResponseDTO<CustomerResponseDTO>>
            GetAllCustomerProfiles(PaginationQueryDTO query)
        {
            return await _customerRepo.GetAllCustomerProfiles(query);
        }

        public async Task<CustomerResponseDTO?> GetCustomerProfileById(int id)
        {
            var customer = await _customerRepo.GetCustomerProfileById(id);

            if (customer == null)
            {
                throw new BusinessException(
                    "Customer profile not found.",
                    StatusCodes.Status404NotFound);
            }

            return customer;
        }

        public async Task<CustomerResponseDTO?> GetCustomerProfileByUserId(
            int userId)
        {
            var customer =
                await _customerRepo.GetCustomerProfileByUserId(userId);

            if (customer == null)
            {
                throw new BusinessException(
                    "Customer profile not found.",
                    StatusCodes.Status404NotFound);
            }

            return customer;
        }

        public async Task<CustomerResponseDTO> CreateCustomerProfile(
            CustomerRequestDTO request,
            ClaimsPrincipal user)
        {
            int userId = GetLoggedInUserId(user);

            if (await _customerRepo.UserProfileExists(userId))
            {
                throw new BusinessException(
                    "A customer profile already exists for this user.",
                    StatusCodes.Status409Conflict);
            }

            var customerProfile = _mapper.Map<CustomerProfile>(request);

            // Assign the authenticated user's ID.
            customerProfile.UserId = userId;

            // Profile image upload can be handled separately.
            var result =
                await _customerRepo.AddCustomerProfile(customerProfile);

            _logger.LogInformation(
                "{UserId} created customer profile {CustomerId}",
                userId,
                result.CustomerId);

            return result;
        }

        public async Task<CustomerResponseDTO?> UpdateCustomerProfile(
            int id,
            CustomerRequestDTO request,
            ClaimsPrincipal user)
        {
            int userId = GetLoggedInUserId(user);

            var existingProfile =
                await _customerRepo.GetCustomerProfileById(id);

            if (existingProfile == null)
            {
                throw new BusinessException(
                    "Customer profile not found.",
                    StatusCodes.Status404NotFound);
            }

            if (!IsAdminOrOfficer(user) &&
                !await _customerRepo.IsProfileOwnedByUser(id, userId))
            {
                throw new BusinessException(
                    "You are not allowed to update this customer profile.",
                    StatusCodes.Status403Forbidden);
            }

            var customerProfile = _mapper.Map<CustomerProfile>(request);

            customerProfile.CustomerId = id;

            // Preserve the original profile owner.
            // The repository response DTO does not expose UserId,
            // so do not assign UserId from the response.
            // The repository updates existing fields by CustomerId.

            customerProfile.ProfileImageUrl =
                existingProfile.ProfileImageUrl;

            var result =
                await _customerRepo.UpdateCustomerProfile(customerProfile);

            if (result == null)
            {
                throw new BusinessException(
                    "Unable to update customer profile.",
                    StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(
                "{UserId} updated customer profile {CustomerId}",
                userId,
                id);

            return result;
        }

        public async Task<bool> DeleteCustomerProfile(
            int id,
            ClaimsPrincipal user)
        {
            int userId = GetLoggedInUserId(user);

            var existingProfile =
                await _customerRepo.GetCustomerProfileById(id);

            if (existingProfile == null)
            {
                throw new BusinessException(
                    "Customer profile not found.",
                    StatusCodes.Status404NotFound);
            }

            if (!IsAdminOrOfficer(user) &&
                !await _customerRepo.IsProfileOwnedByUser(id, userId))
            {
                throw new BusinessException(
                    "You are not allowed to delete this customer profile.",
                    StatusCodes.Status403Forbidden);
            }

            bool deleted = await _customerRepo.DeleteCustomerProfile(id);

            if (!deleted)
            {
                throw new BusinessException(
                    "Unable to delete customer profile.",
                    StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(
                "{UserId} deleted customer profile {CustomerId}",
                userId,
                id);

            return true;
        }

        private static int GetLoggedInUserId(ClaimsPrincipal user)
        {
            var userIdValue =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value
                ?? user.FindFirst("UserId")?.Value;

            if (!int.TryParse(userIdValue, out int userId) || userId <= 0)
            {
                throw new BusinessException(
                    "A valid authenticated user ID was not found.",
                    StatusCodes.Status401Unauthorized);
            }

            return userId;
        }

        private static bool IsAdminOrOfficer(ClaimsPrincipal user)
        {
            return user.IsInRole("Admin") ||
                   user.IsInRole("Officer");
        }
    }

}
