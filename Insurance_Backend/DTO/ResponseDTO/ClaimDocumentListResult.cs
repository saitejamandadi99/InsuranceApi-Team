namespace Insurance_Backend.DTO.ResponseDTO
{
    public class ClaimDocumentListResult
    {
        public int DocumentId { get; set; }

        public string DocumentName { get; set; } = string.Empty;

        public string ClaimNumber { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;

        public string? DocumentReference { get; set; }

        public DateTime UploadedDate { get; set; }

        public int TotalRecords { get; set; }
    }
}
