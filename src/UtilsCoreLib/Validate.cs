using System.Runtime.CompilerServices;
using System.Text;
using FxPu.Utils.Resources;

namespace FxPu.Utils
{
    public static class Validate
    {
        public static ValidationError? IsNull(object? value, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (value == null)
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? Messages.ValidateIsNull);
        }

        public static ValidationError? IsNotNull(object? value, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (value != null)
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? Messages.ValidateIsNotNull);
        }

        public static ValidationError? IsEmpty(object? value, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (ValueHelper.IsEmpty(value))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? Messages.ValidateIsEmpty);
        }

        public static ValidationError? IsNotEmpty(object? value, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (!ValueHelper.IsEmpty(value))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? Messages.ValidateIsNotEmpty);
        }


        public static ValidationError? IsTrue(bool? value, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (ValueHelper.IsTrue(value))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? Messages.ValidateIsTrue);
        }

        public static ValidationError? IsFalse(bool? value, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (!ValueHelper.IsTrue(value))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? Messages.ValidateIsFalse);
        }

        public static ValidationError? IsInMaxLength(string? value, int maxLength, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (ValueHelper.IsInMaxLength(value, maxLength))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? string.Format(Messages.ValidateIsInMaxLength, maxLength));
        }



        public static ValidationError? IsBetween(IComparable? value, IComparable minValue, IComparable maxValue, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (ValueHelper.IsBetween(value, minValue, maxValue))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? string.Format(Messages.ValidateIsBetween, minValue, maxValue));
        }

        public static ValidationError? IsBetweenOrIsNull(IComparable? value, IComparable minValue, IComparable maxValue, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (ValueHelper.IsBetweenOrIsNull(value, minValue, maxValue))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? string.Format(Messages.ValidateIsBetweenOrIsNull, minValue, maxValue));
        }


        public static ValidationError? IsRegexMatch(string? value, string pattern, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null, int regexTimeoutMilliseconds = 200)
        {
            if (ValueHelper.IsRegexMatch(value, pattern, regexTimeoutMilliseconds))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? string.Format(Messages.ValidateIsRegexMatch, pattern));
        }

        public static ValidationError? IsRegexMatchOrIsNull(string? value, string pattern, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null, int regexTimeoutMilliseconds = 200)
        {
            if (ValueHelper.IsRegexMatchOrIsNull(value, pattern, regexTimeoutMilliseconds))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? string.Format(Messages.ValidateIsRegexMatchOrIsNull, pattern));
        }

        public static ValidationError? IsNoInvalidChars(string? value, char[] invalidChars, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (ValueHelper.IsNoInvalidChars(value, invalidChars))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? string.Format(Messages.ValidateIsNoInvalidChars, invalidChars));
        }


        public static ValidationError? IsIn<TValue>(TValue? value, IEnumerable<TValue> items, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (ValueHelper.IsIn(value, items))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? string.Format(Messages.ValidateIsIn, EnumerableToString(items)));
        }


        public static ValidationError? IsNotIn<TValue>(TValue? value, IEnumerable<TValue> items, string? message = null, Func<string?>? messageFunc = null, [CallerArgumentExpression("value")] string? source = null)
        {
            if (ValueHelper.IsNotIn(value, items))
            {
                return null;
            }

            return new ValidationError(source, message ?? messageFunc?.Invoke() ?? string.Format(Messages.ValidateIsNotIn, EnumerableToString(items)));
        }
        private static string EnumerableToString<TValue>(IEnumerable<TValue> items)
        {
            StringBuilder sb = new StringBuilder();
            var i = 0;
            foreach (var item in items)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                if (i > 4)
                {
                    sb.Append("...");
                    break;
                }
                sb.Append(item?.ToString() ?? "<null>");
            }
            return sb.ToString();
        }

    }
}
