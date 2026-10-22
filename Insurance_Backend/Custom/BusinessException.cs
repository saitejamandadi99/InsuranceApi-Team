namespace Insurance_Backend.Custom
{
    public class BusinessException:Exception
    {
        public int StatusCode { get; }

        public BusinessException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
