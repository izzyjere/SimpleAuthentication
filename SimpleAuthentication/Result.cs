using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuthentication
{
    public class Result
    {
        public string Message { get; }
        public bool Succeeded { get; }

        protected Result(bool success, string message)
        { 
            Message = message;
            Succeeded = success;
        }
        internal static  Result Success(string message = "")
        {
            return new Result(true,message);
        } 
        internal static Result Failure(string message = "")
        {
            return new Result(false,message);
        }
    }
    public class Result<T>
    {
        public T? Data { get; }
        public bool Succeeded { get; }
        public string Message { get; }
        private Result(T? data, bool succeeded, string message)
        {
            Data = data;
            Succeeded=succeeded;
            Message=message;
        }
        internal static Result<T> Success(T data, string message = "")
        {
            return new Result<T>(data,true, message);
        }
        internal static Result<T> Failure(string message = "")
        {
            return new Result<T>(default, false, message);
        }
    } 
}
