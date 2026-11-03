using Insurance_Backend.DTO.RequestDTO;
using Insurance_Backend.Enums;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Insurance_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginationQueryDTO query)
        {
            var users = await _userService.GetAllUsers(query);
            return Success(users, "Retrieved users successfully");
        }
        [Authorize(Roles = nameof(Role.Admin))]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            var user = await _userService.GetUserById(id);
            return Success(user, "Retrieved user successfully");
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] RegisterRequestDTO request)
        {
            var result = await _userService.AddOfficer(request);
            return Success(result, "User(officer) created successfully");
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPut("activate/{id}")]
        public async Task<IActionResult> ActivateUser([FromRoute] int id)
        {
            var user = await _userService.ActivateUser(id);
            return Success(user, "Activated User successfully");
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateUser([FromRoute] int id)
        {
            var user = await _userService.DeactivateUser(id);
            return Success(user, "Deactivated user successfully");
        }
    }
}
