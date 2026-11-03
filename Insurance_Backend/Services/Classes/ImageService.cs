using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Insurance_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Insurance_Backend.Services.Classes
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<ImageService> _logger;

        private const long MaxFileSize = 5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        public ImageService(
            Cloudinary cloudinary,
            ILogger<ImageService> logger)
        {
            _cloudinary = cloudinary;
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException(
                    "Please select an image to upload.");
            }

            if (file.Length > MaxFileSize)
            {
                throw new ArgumentException(
                    "Image size must not exceed 5 MB.");
            }

            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedExtensions.Contains(
                    extension,
                    StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Only JPG, JPEG, PNG, and WEBP images are allowed.");
            }

            if (string.IsNullOrWhiteSpace(file.ContentType) ||
                !file.ContentType.StartsWith(
                    "image/",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "The uploaded file must be an image.");
            }

            try
            {
                await using var stream = file.OpenReadStream();

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(
                        Path.GetFileName(file.FileName),
                        stream),

                    Folder = "insurance-profiles"
                };

                var uploadResult =
                    await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null ||
                    uploadResult.SecureUrl == null)
                {
                    _logger.LogError(
                        "Cloudinary image upload failed: {Error}",
                        uploadResult.Error?.Message);

                    throw new InvalidOperationException(
                        "Image upload failed. Please try again.");
                }

                _logger.LogInformation(
                    "Image uploaded successfully to Cloudinary.");

                return uploadResult.SecureUrl.ToString();
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error occurred while uploading an image.");

                throw new InvalidOperationException(
                    "Unable to upload the image at this time.",
                    ex);
            }
        }
    }
}