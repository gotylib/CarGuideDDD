

using CarGuideDDD.Core.EntityObjects.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CarGuideDDD.Core.AnswerObjects
{
    public class ServiceResultGet<T,E,R> : ServiceResult
        where T : IEntity
        where E : Exception
        where R : IEntity
    {
        public IEnumerable<T> Results { get; set; }
        public E Error { get; set; }
        public R Result { get; set; }   

        public ServiceResultGet(IEnumerable<T> results = null, E error = null, R result = default, bool success = true, string mesage = null,int statusCode = 200)
            :base(success, mesage, statusCode)
        {
            Results = results;
            Error = error;
            Result = result;
        }


        public static ServiceResultGet<T, E, R> IEnumerableResult(IEnumerable<T> results)
        {
            return new ServiceResultGet<T, E, R>(results);
        }

        public static ServiceResultGet<T, E, R>  SimpleResult(R result)
        {
            return new ServiceResultGet<T, E, R>(null,null,result);
        }

        public static ServiceResultGet<T, E, R> ErrorResult(E error)
        {
            return new ServiceResultGet<T, E, R>(null, error, default,false,null,400);
        }

        public static ServiceResultGet<T, E, R> ServerError()
        {
            return new ServiceResultGet<T, E, R>(null, null, default, false, null, 500);
        }

        public static ServiceResultGet<T, E, R> BadRequest(string message, int statusCode = 400)
        {
            return new ServiceResultGet<T, E, R>(null, null, default, false, message, statusCode);
        }

        public static ServiceResultGet<T, E, R> ServerError(string message = null)
        {
            return new ServiceResultGet<T, E, R>(null, null, default, false, message, 500);
        }
    }
}
