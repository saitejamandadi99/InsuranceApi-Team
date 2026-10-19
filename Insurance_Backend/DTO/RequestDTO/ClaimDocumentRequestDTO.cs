namespace Insurance_Backend.DTO.RequestDTO
{
    public class ClaimDocumentRequestDTO
    {
        public int ClaimId { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public List<IFormFile> Files { get; set; } = new();
    }
}
