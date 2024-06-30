using System.Collections;
using System.Runtime.CompilerServices;

namespace FxPu.UtilityLib
{
    public static class ThrowUtility
    {
        public static void Throw<TException>(FormattableString message) where TException : Exception
            => throw (TException) Activator.CreateInstance(typeof(TException), message.ToString());

        public static void ThrowIf<TException>(bool throwCondition, FormattableString message) where TException : Exception
        {
            if (throwCondition)
            {
                Throw<TException>(message);
            }
        }


        public static void ThrowIfIsNull<TException>(object? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
            => ThrowIf<TException>(ValueUtility.IsNull(argument), message ?? $"{source} is null.");

        public static void ThrowIfIsNotNull<TException>(object? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
            => ThrowIf<TException>(ValueUtility.IsNotNull(argument), message ?? $"{source} is not null.");

        public static void ThrowIfIsTrue<TException>(bool? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
    => ThrowIf<TException>(ValueUtility.IsTrue(argument), message ?? $"{source} is true.");

        public static void ThrowIfIsFalse<TException>(bool? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
            => ThrowIf<TException>(ValueUtility.IsFalse(argument), message ?? $"{source} is false.");

        public static void ThrowIfIsEmpty<TException>(string? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
            => ThrowIf<TException>(ValueUtility.IsEmpty(argument), message ?? $"{source} is empty.");

        public static void ThrowIfIsNotEmpty<TException>(string? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
            => ThrowIf<TException>(ValueUtility.IsNotEmpty(argument), message ?? $"{source} is not empty.");



        public static void ThrowIfIsEmpty<TException>(IEnumerable? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
    => ThrowIf<TException>(ValueUtility.IsEmpty(argument), message ?? $"{source} is empty.");

        public static void ThrowIfIsNotEmpty<TException>(IEnumerable? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
            => ThrowIf<TException>(ValueUtility.IsNotEmpty(argument), message ?? $"{source} is not empty.");


        public static void ThrowIfIsEmpty<TException, TValue>(IEnumerable<TValue>? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
    => ThrowIf<TException>(ValueUtility.IsEmpty(argument), message ?? $"{source} is empty.");

        public static void ThrowIfIsNotEmpty<TException, TValue>(IEnumerable<TValue>? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
            => ThrowIf<TException>(ValueUtility.IsNotEmpty(argument), message ?? $"{source} is not empty.");




        public static void ThrowIfIsEmpty<TException>(IQueryable<object>? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
=> ThrowIf<TException>(ValueUtility.IsEmpty(argument), message ?? $"{source} is empty.");

        public static void ThrowIfIsNotEmpty<TException>(IQueryable<object>? argument, [CallerArgumentExpression(nameof(argument))] string? source = null, FormattableString? message = null) where TException : Exception
            => ThrowIf<TException>(ValueUtility.IsNotEmpty(argument), message ?? $"{source} is not empty.");

    }
}