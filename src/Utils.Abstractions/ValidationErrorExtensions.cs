using FxPu.Utils.Resources;

namespace FxPu.Utils
{
    public static class ValidationErrorExtensions
    {
        private static TException CreateException<TException>(string? source, string message)
            where TException : Exception
        {
            // ArgumentNullException
            if (typeof(TException) == typeof(ArgumentNullException))
            {
                message = message.Replace("{{source}}", Messages.ValidateValue);
                return new ArgumentNullException(source, message) as TException;
            }

            // ArgumentException
            if (typeof(TException) == typeof(ArgumentException))
            {
                message = message.Replace("{{source}}", Messages.ValidateValue);
                return new ArgumentException(message, source) as TException;
            }

            // ValidationErrorsException
            if (typeof(TException) == typeof(ValidationErrorsException))
            {
                message = message.Replace("{{source}}", Messages.ValidateValue);
                return new ValidationErrorsException(new[] { new ValidationError(source, message) }) as TException;
            }

            // build message with source
            message = message.Replace("{{source}}", source == null ? Messages.ValidateValue : $"\"{source}\"");

            return (TException) Activator.CreateInstance(typeof(TException), message);
        }

        public static void Throw(this ValidationError? validationError)
            => validationError?.Throw<ValidationErrorsException>();

        public static void Throw<TException>(this ValidationError? validationError) where TException : Exception
        {
            if (validationError != null)
            {
                throw CreateException<TException>(validationError.Source, validationError.Message);
            }
        }

        public static void AddToCollection(this ValidationError? validationError, ICollection<ValidationError> validationErrors)
        {
            if (validationError != null)
            {
                validationErrors?.Add(validationError);
            }
        }

    }
}
