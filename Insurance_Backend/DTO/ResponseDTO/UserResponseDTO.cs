using Insurance_Backend.Enums;

namespace Insurance_Backend.DTO.ResponseDTO
{
    public class UserResponseDTO
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;
        public Role Role { get; set; }

        public ActiveStatus ActiveStatus { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
