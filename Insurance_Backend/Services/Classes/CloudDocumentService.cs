using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Insurance_Backend.Services.Interfaces;

namespace Insurance_Backend.Services.Classes
{
    public class CloudDocumentService:ICloudDocumentService
    {

        private readonly Cloudinary _cloudinary;
        public CloudDocumentService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task DeleteDocumentAsync(string documentReference)
        {
            if (string.IsNullOrWhiteSpace(documentReference))
            {
                return;
            }

            var publicId = GetPublicId(documentReference);
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Raw
            };
            var result = await _cloudinary.DestroyAsync(deleteParams);
            if (result.Error != null)
            {
                throw new Exception(result.Error.Message);
            }
        }

        private static string GetPublicId(string documentReference)
        {
            var uri = new Uri(documentReference);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var uploadIndex = Array.IndexOf(segments, "upload");
            var publicIdSegments = segments.Skip(uploadIndex + 2).ToArray();
            var publicId = string.Join('/', publicIdSegments);
            return Path.ChangeExtension(publicId, null);

        }

        //https://res.cloudinary.com/demo/raw/upload/v1754000000/insurance/claim-documents/bill123.pdf
        //GetPublicId() extracts: insurance/claim-documents/bill123
        public async Task<string> UploadClaimDocumentAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("Claim Document Required");
            }


            await using var stream = file.OpenReadStream();
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "insurance/claim-documents"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception(uploadResult.Error.Message);
            }



            if (uploadResult.SecureUrl == null)
            {
                throw new Exception("Document upload failed");
            }
            return uploadResult.SecureUrl.AbsoluteUri;
        }
    }

}
