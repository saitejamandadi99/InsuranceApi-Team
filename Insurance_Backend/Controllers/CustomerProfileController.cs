using System.Security.Claims;
using Insurance_Backend.Config;
using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insurance_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerProfileController : BaseController
    {
        private readonly ICustomerProfileService _customerProfileService;

        public CustomerProfileController(
            ICustomerProfileService customerProfileService)
        {
            _customerProfileService = customerProfileService;
        }

        // GET: api/CustomerProfile
        // Admin and Agent can retrieve all customer profiles.
        [HttpGet]
        [Authorize(Roles = nameof(Role.Admin) + "," + nameof(Role.Officer))]
        public async Task<IActionResult> GetAllCustomerProfiles(
            [FromQuery] PaginationQueryDTO query)
        {
            var profiles =
                await _customerProfileService.GetAllCustomerProfiles(query);

            return Success(
                profiles,
                "Customer profiles retrieved successfully.");
        }

        // GET: api/CustomerProfile/5
        // Admin and Agent can retrieve a profile by CustomerId.
        [HttpGet("{id:int}")]
        [Authorize(Roles = nameof(Role.Admin) + "," + nameof(Role.Officer))]
        public async Task<IActionResult> GetCustomerProfileById(int id)
        {
            var profile =
                await _customerProfileService.GetCustomerProfileById(id);

            if (profile == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Customer profile not found."
                });
            }

            return Success(
                profile,
                "Customer profile retrieved successfully.");
        }

        // GET: api/CustomerProfile/my-profile
        // A customer can retrieve their own profile.
        [HttpGet("my-profile")]
        [Authorize(Roles = nameof(Role.Customer))]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.GetUserId();

            var profile =
                await _customerProfileService.GetCustomerProfileByUserId(userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Customer profile not found."
                });
            }

            return Success(
                profile,
                "Your profile was retrieved successfully.");
        }

        // GET: api/CustomerProfile/user/5
        // Admin and Agent can retrieve a profile by UserId.
        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = nameof(Role.Admin) + "," + nameof(Role.Officer))]
        public async Task<IActionResult> GetCustomerProfileByUserId(int userId)
        {
            var profile =
                await _customerProfileService.GetCustomerProfileByUserId(userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Customer profile not found."
                });
            }

            return Success(
                profile,
                "Customer profile retrieved successfully.");
        }

        // POST: api/CustomerProfile
        // A customer creates their own profile.
        [HttpPost]
        [Authorize(Roles = nameof(Role.Customer))]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateCustomerProfile(
            [FromForm] CustomerRequestDTO request)
        {
            var profile =
                await _customerProfileService.CreateCustomerProfile(
                    request,
                    User);

            return CreatedAtAction(
                nameof(GetCustomerProfileById),
                new { id = profile.CustomerId },
                new
                {
                    Success = true,
                    Message = "Customer profile created successfully.",
                    Data = profile
                });
        }

        // PUT: api/CustomerProfile/5
        // A customer updates their own profile; Admin can update profiles.
        [HttpPut]
        [Authorize(Roles = nameof(Role.Customer) + "," + nameof(Role.Admin))]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCustomerProfile(
            //int id,
            [FromForm] CustomerRequestDTO request)
        {
            var profile =
                await _customerProfileService.UpdateCustomerProfile(
                    User.GetUserId(),
                    request,
                    User);

            if (profile == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = "Customer profile not found."
                });
            }

            return Success(
                profile,
                "Customer profile updated successfully.");
        }
    }
}
