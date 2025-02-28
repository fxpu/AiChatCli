using System.Text;

namespace FxPu.Utils
{
    public static class ValidationErrorsExtensions
    {
        public static string? ValidationErrorsAsString(this IEnumerable<ValidationError> validationErrors)
        {
            StringBuilder sb = new StringBuilder("Validation errors:");
            foreach (var validationError in validationErrors)
            {
                sb.Append($"\n{validationError.Source}: {validationError.Message}");
            }

            return sb.ToString();
        }


        public static void Throw(this ICollection<ValidationError> validationErrors)
        {
            if (validationErrors.Count > 0)
            {
                throw new ValidationErrorsException(validationErrors);
            }
        }

    }
}
