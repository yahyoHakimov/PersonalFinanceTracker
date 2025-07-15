using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Application.Common.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? Error { get; private set; }
        public List<string> Errors { get; private set; } = new();

        private Result(bool isSuccess, T? data, string? error)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
        }

        public static Result<T> Success(T data) => new(true, data, null);
        public static Result<T> Failure(string error) => new(false, default, error);
        public static Result<T> Failure(List<string> errors)
        {
            var result = new Result<T>(false, default, errors.FirstOrDefault());
            result.Errors = errors;
            return result;
        }
    }
}
