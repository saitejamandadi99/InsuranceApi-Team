namespace Insurance_Backend.DTO.ResponseDTO
{
    public class ClaimDocumentResponseDTO
    {
        public int DocumentId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentReference { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
    }
}
