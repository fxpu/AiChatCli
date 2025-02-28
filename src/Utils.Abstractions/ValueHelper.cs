#nullable enable

using System.Collections;
using System.Text.RegularExpressions;

namespace FxPu.Utils
{
    /// <summary>
    /// Provides helper methods for value operations.
    /// </summary>
    public static class ValueHelper
    {
        /// <summary>
        /// Determines whether the specified value is empty.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value is empty; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// This method handles different types of values:
        /// - If the value is null, it returns true.
        /// - If the value is a string, it checks if the string is null or empty.
        /// - If the value is an IEnumerable, it checks if the collection is empty.
        /// </remarks>
        public static bool IsEmpty(object? value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is string str)
            {
                return string.IsNullOrEmpty(str);
            }

            if (value is IEnumerable enumerable)
            {
                if (value is ICollection collection)
                {
                    return collection.Count == 0;
                }

                return !enumerable.Cast<object>().Any();
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified nullable boolean value is true.
        /// </summary>
        /// <param name="value">The nullable boolean value to check.</param>
        /// <returns><c>true</c> if the value is true; otherwise, <c>false</c>.</returns>
        public static bool IsTrue(bool? value)
            => value ?? false;

        /// <summary>
        /// Gets the value associated with the specified key from the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to search.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="notFoundValue">The value to return if the key is not found.</param>
        /// <returns>The value associated with the specified key, or <paramref name="notFoundValue"/> if the key is not found.</returns>
        public static TValue GetValue<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue notFoundValue = default)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            return notFoundValue;
        }

        /// <summary>
        /// Gets the value associated with the specified key from the read-only dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The read-only dictionary to search.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="notFoundValue">The value to return if the key is not found.</param>
        /// <returns>The value associated with the specified key, or <paramref name="notFoundValue"/> if the key is not found.</returns>
        public static TValue GetValue<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue notFoundValue = default)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }

            return notFoundValue;
        }

        public static bool IsEqual<TValue>(TValue? value, TValue? otherValue)
        {
            if (value == null)
            {
                return otherValue == null;
            }

            return value.Equals(otherValue);
        }

        public static bool IsInMaxLength(string? value, int maxLength)
        {
            return IsEmpty(value) || value!.Length <= maxLength;
        }



        public static bool IsBetween(IComparable? value, IComparable minValue, IComparable maxValue)
        {
            if (value == null)
            {
                return false;
            }

            return value.CompareTo(minValue) >= 0 && value.CompareTo(maxValue) <= 0;
        }

        public static bool IsBetweenOrIsNull(IComparable? value, IComparable minValue, IComparable maxValue)
        {
            if (value == null)
            {
                return true;
            }

            return value.CompareTo(minValue) >= 0 && value.CompareTo(maxValue) <= 0;
        }

        public static bool IsRegexMatch(string? value, string pattern, int regexTimeoutMilliseconds = 200)
        {
            if (IsEmpty(value))
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(value, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(regexTimeoutMilliseconds));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static bool IsRegexMatchOrIsNull(string? value, string pattern, int regexTimeoutMilliseconds = 200)
        {
            if (value == null)
            {
                return true;
            }

            try
            {
                return Regex.IsMatch(value, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(regexTimeoutMilliseconds));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static bool IsNoInvalidChars(string? value, char[] invalidChars)
        {
            return IsEmpty(value) || value.IndexOfAny(invalidChars) == -1;
        }

        public static bool IsIn<TValue>(TValue? value, IEnumerable<TValue> items)
        => value != null && items.Contains(value);



        public static bool IsNotIn<TValue>(TValue? value, IEnumerable<TValue> items)
            => !IsIn(value, items);

    }
}
