namespace Insurance_Backend.DTO.ResponseDTO
{
    public class ErrorResponseDTO
    {
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public int StatusCode { get; set; } //codes like 404, 401, 403

        public string ErrorType { get; set; } = string.Empty; //like Not Found, UnAuthorized etc
        public string Message { get; set; } = string.Empty;
        public string RequestPath { get; set; } = string.Empty;
    }
}
