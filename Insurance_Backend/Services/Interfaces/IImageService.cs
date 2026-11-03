using Microsoft.AspNetCore.Http;

namespace Insurance_Backend.Services.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}