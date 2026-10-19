namespace Insurance_Backend.DTO.ResponseDTO
{
    public class SuccessResponseDTO<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
