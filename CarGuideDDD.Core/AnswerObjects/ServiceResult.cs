

namespace CarGuideDDD.Core.AnswerObjects
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public int StatusCode { get; set; }

        public ServiceResult(bool success = true, string message = null, int statusCode = 200)
        { 
            Success = success;
            Message = message;
            StatusCode = statusCode;
        }

        public static ServiceResult Ok(string message = null)
        {
            return new ServiceResult(true, message);
        }

        public static ServiceResult BadRequest(string message, int statusCode = 400)
        {
            return new ServiceResult(false, message, statusCode);
        }

        public static ServiceResult ServerError(string message = null)
        {
            return new ServiceResult(false, message,500);
        }

    }
}
