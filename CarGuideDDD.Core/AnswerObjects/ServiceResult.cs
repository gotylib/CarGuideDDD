using CarGuideDDD.Core.EntityObjects.Interfaces;

public enum Errors
{
    InvalidUserOrRole,
    CarNotFound,
    InvalidRole,
    ServerError,
    BadRequest
}


namespace CarGuideDDD.Core.AnswerObjects
{
    public class ServiceResult<T, E, R>
        where T : IEntity
        where E : Exception
        where R : IEntity
    {
        public IEnumerable<T> Results { get; set; }
        public E Error { get; set; }
        public R Result { get; set; }
        public bool Success { get; set; }
        public Errors? ErrorCode { get; set; }

        public ServiceResult(IEnumerable<T> results = null, E error = null, R result = default, bool success = true, Errors? errorCode = null)
        {
            Results = results;
            Error = error;
            Result = result;
            Success = success;
            ErrorCode = errorCode;
        }

        public static ServiceResult<T, E, R> IEnumerableResult(IEnumerable<T> results)
        {
            return new ServiceResult<T, E, R>(results);
        }

        public static ServiceResult<T, E, R> SimpleResult(R result)
        {
            return new ServiceResult<T, E, R>(null, null, result);
        }

        public static ServiceResult<T, E, R> ErrorResult(E error)
        {
            return new ServiceResult<T, E, R>(null, error, default, false);
        }

        public static ServiceResult<T, E, R> ServerError()
        {
            return new ServiceResult<T, E, R>(null, null, default, false, Errors.ServerError);
        }

        public static ServiceResult<T, E, R> BadRequest()
        {
            return new ServiceResult<T, E, R>(null, null, default, false, Errors.BadRequest);
        }

        public static ServiceResult<T, E, R> Ok()
        {
            return new ServiceResult<T, E, R>(null, null, default, true);
        }

    }
}
