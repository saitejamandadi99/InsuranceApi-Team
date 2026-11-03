using Insurance_Backend.DTO.ResponseDTO;
using Microsoft.AspNetCore.Mvc;

namespace Insurance_Backend.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult Success<T>(T data, string message)
        {
            return Ok(new SuccessResponseDTO<T>
            {
                Success = true,
                Message = message,
                Data = data
            });
        }

        protected IActionResult CreatedAtSuccess<T>(T data, string message, string actionName, object routeValues)
        {
            return CreatedAtAction(actionName, routeValues, (new SuccessResponseDTO<T>
            {
                Success = true,
                Message = message,
                Data = data
            }));
        }
    }
}
