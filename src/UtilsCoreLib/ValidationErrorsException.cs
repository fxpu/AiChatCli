namespace FxPu.Utils{
    /// <summary>
    /// Represents an exception that is thrown when there are validation errors in a process or operation.
    /// </summary>
    public class ValidationErrorsException : Exception    {
        /// <summary>
        /// Gets or sets the collection of validation errors.
        /// </summary>
        public IEnumerable<ValidationError> ValidationErrors { get; init; } = null!;

        /// <summary>
        /// Initializes a new instance of the ValidationErrorsException class with a default error message.
        /// </summary>
        /// <returns>
        /// A new instance of the ValidationErrorsException class with an empty array of validation errors.
        /// </returns>
        public ValidationErrorsException()
                    : base("One or more validation errors occured.")        {            ValidationErrors = Array.Empty<ValidationError>();        }

        /// <summary>
        /// Initializes a new instance of the ValidationErrorsException class with the specified validation errors and optional message.
        /// </summary>
        /// <param name="validationErrors">The collection of validation errors that caused the exception.</param>
        /// <param name="message">The optional message to include with the exception.</param>
        /// <returns>A new instance of the ValidationErrorsException class.</returns>
        public ValidationErrorsException(IEnumerable<ValidationError> validationErrors, string? message = null)
                    : base(message ?? "One or more validation errors occured.")        {            ArgumentNullException.ThrowIfNull(validationErrors);            ValidationErrors = validationErrors;        }

        /// <summary>
        /// Constructor for ValidationErrorsException class that takes a single validation error and an optional message parameter.
        /// </summary>
        /// <param name="validationError">The validation error that caused the exception.</param>
        /// <param name="message">Optional message to include with the exception.</param>
        /// <returns>A new instance of the ValidationErrorsException class.</returns>
        public ValidationErrorsException(ValidationError validationError, string? message = null)
                    : base(message ?? "One or more validation errors occured.")        {            ArgumentNullException.ThrowIfNull(validationError);            ValidationErrors = new ValidationError[] { validationError };        }

        /// <summary>
        /// Initializes a new instance of the ValidationErrorsException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <returns>A new instance of the ValidationErrorsException class.</returns>
        public ValidationErrorsException(string? message)
                    : base(message ?? "One or more validation errors occured.")        {        }

        /// <summary>
        /// Initializes a new instance of the ValidationErrorsException class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        /// <returns>A new instance of the ValidationErrorsException class.</returns>
        public ValidationErrorsException(string? message, Exception? innerException)
                    : base(message ?? "One or more validation errors occured.", innerException)        {        }    }}