namespace Insurance_Backend.Services.Interfaces
{
    public interface ICloudDocumentService
    {
        Task<string> UploadClaimDocumentAsync(IFormFile file);
        Task DeleteDocumentAsync(string documentReference);
    }
}
