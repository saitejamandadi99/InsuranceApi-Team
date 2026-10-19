namespace Insurance_Backend.DTO.ResponseDTO
{
    public class CustomerResponseDTO
    {
        public int CustomerId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;


        public string State { get; set; } = string.Empty;

        public string Pincode { get; set; } = string.Empty;

        public string NomineeName { get; set; } = string.Empty;

        public string NomineeRelationship { get; set; } = string.Empty;

        public string? ProfileImageUrl { get; set; } 
    }
}
