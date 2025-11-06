namespace FormReporting.Models.Common
{
    /// <summary>
    /// Represents the result of an operation (success or failure)
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message describing the result
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// List of error messages (if operation failed)
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static OperationResult SuccessResult(string message = "Operation completed successfully")
        {
            return new OperationResult
            {
                Success = true,
                Message = message
            };
        }

        /// <summary>
        /// Creates a failure result
        /// </summary>
        public static OperationResult FailureResult(string message, List<string>? errors = null)
        {
            return new OperationResult
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    /// <summary>
    /// Represents the result of an operation with data
    /// </summary>
    /// <typeparam name="T">The type of data returned</typeparam>
    public class OperationResult<T> : OperationResult
    {
        /// <summary>
        /// The data returned by the operation
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Creates a successful result with data
        /// </summary>
        public static OperationResult<T> SuccessResult(T data, string message = "Operation completed successfully")
        {
            return new OperationResult<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Creates a failure result
        /// </summary>
        public new static OperationResult<T> FailureResult(string message, List<string>? errors = null)
        {
            return new OperationResult<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
