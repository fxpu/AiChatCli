using System.Collections;
using System.Runtime.CompilerServices;

namespace FxPu.Utils
{
    public static class ThrowIf
    {
        private static void ConditionalThrow<TException>(bool throwCondition, FormattableString message)
            where TException : Exception
        {
            if (throwCondition)
            {
                throw (TException) Activator.CreateInstance(typeof(TException), message.ToString());
            }
        }


        public static void IsNull<TException>(object? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
            => ConditionalThrow<TException>(Value.IsNull(argument), message ?? $"{source} is null.");

        public static void IsNotNull<TException>(object? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
            => ConditionalThrow<TException>(Value.IsNotNull(argument), message ?? $"{source} is not null.");

        public static void IsTrue<TException>(bool? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
    => ConditionalThrow<TException>(Value.IsTrue(argument), message ?? $"{source} is true.");

        public static void IsFalse<TException>(bool? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
            => ConditionalThrow<TException>(Value.IsFalse(argument), message ?? $"{source} is false.");

        public static void IsEmpty<TException>(string? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
            => ConditionalThrow<TException>(Value.IsEmpty(argument), message ?? $"{source} is empty.");

        public static void IsNotEmpty<TException>(string? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
            => ConditionalThrow<TException>(Value.IsNotEmpty(argument), message ?? $"{source} is not empty.");


        public static void IsEmpty<TException>(IEnumerable? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
    => ConditionalThrow<TException>(Value.IsEmpty(argument), message ?? $"{source} is empty.");

        public static void IsNotEmpty<TException>(IEnumerable? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
            => ConditionalThrow<TException>(Value.IsNotEmpty(argument), message ?? $"{source} is not empty.");


        public static void IsEmpty<TException, TValue>(IEnumerable<TValue>? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
    => ConditionalThrow<TException>(Value.IsEmpty(argument), message ?? $"{source} is empty.");


        public static void IsNotEmpty<TException, TValue>(IEnumerable<TValue>? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
            => ConditionalThrow<TException>(Value.IsNotEmpty(argument), message ?? $"{source} is not empty.");


        public static void IsEmpty<TException>(IQueryable<object>? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
=> ConditionalThrow<TException>(Value.IsEmpty(argument), message ?? $"{source} is empty.");

        public static void ThrowIfIsNotEmpty<TException>(IQueryable<object>? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null)
            where TException : Exception
            => ConditionalThrow<TException>(Value.IsNotEmpty(argument), message ?? $"{source} is not empty.");

    }
}