namespace Insurance_Backend.DTO.ResponseDTO
{
    public class LoginResponseDTO
    {
        public string JwtToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public string UserEmail { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
