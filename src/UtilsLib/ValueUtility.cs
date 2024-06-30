using System.Collections;

namespace FxPu.UtilityLib
{
    public static class ValueUtility
    {
        public static bool IsNull(object? value)
            => value == null;

        public static bool IsNotNull(object? value)
            => !IsNull(value);

        public static bool IsTrue(bool? value)
            => (value ?? false);

        public static bool IsFalse(bool? value)
            => !IsTrue(value);

        public static bool IsEmpty(string? value)
            => string.IsNullOrEmpty(value);

        public static bool IsNotEmpty(string? value)
            => !IsEmpty(value);

        public static bool IsEmpty(IEnumerable? value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is ICollection c)
            {
                return c.Count == 0;
            }

            foreach (var item in value)
            {
                return false;
            }

            return true;
        }

        public static bool IsNotEmpty(IEnumerable? value)
            => !IsEmpty(value);

        public static bool IsEmpty(IEnumerable<object>? value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is ICollection<object> c)
            {
                return c.Count == 0;
            }

            foreach (var item in value)
            {
                return false;
            }

            return true;
        }

        public static bool IsNotEmpty(IEnumerable<object>? value)
            => !IsEmpty(value);

        public static bool IsEmpty(IQueryable<object>? value)
        {
            if (value == null)
            {
                return true;
            }

            return !value.Any();
        }

        public static bool IsNotEmpty(IQueryable<object> value)
            => !IsEmpty(value);

    }
}