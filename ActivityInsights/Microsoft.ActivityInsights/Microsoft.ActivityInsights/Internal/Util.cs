
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.ApplicationInsights;

namespace Microsoft.ActivityInsights
{
    public static class Util
    {
        public const string NullString = "null";

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static T EnsureNotNull<T>(T value, string variableName) where T : class
        {
            if (value == null)
            {
                throw (variableName == null) ? new ArgumentNullException() : new ArgumentNullException(variableName);
            }

            return value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static string EnsureNotNullOrEmpty(string value, string variableName)
        {
            if (value == null)
            {
                throw (variableName == null) ? new ArgumentNullException() : new ArgumentNullException(variableName);
            }

            if (value.Length == 0)
            {
                throw (variableName == null)
                        ? new ArgumentException("The specified string may not be empty.")
                        : new ArgumentNullException($"\"{variableName}\" may not be empty.");
            }

            return value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static string SpellNull(string value)
        {
            return value ?? NullString;
        }

        public static string FormatAsArray(IEnumerable<object> values, bool includeOuterBraces = true)
        {
            if (values == null)
            {
                return NullString;
            }

            IEnumerable<object> valuesInQuotes = values.Select( (v) => '"' + (v?.ToString() ?? NullString) + '"' );
            string str = String.Join(", ", valuesInQuotes);

            if (includeOuterBraces)
            {
                str = "{" + str + "}";
            }

            return str;
        }

        public static string CreateRandomId()
        {
            // We shorten a guid becasue locally the chars we use are "unique-enough:: 
            // "708F5CC4-7F9C-468F-B21E-7153A88E24DE"
            //      "CC4-7F9C-468F-B21E-715"

            string guid = Guid.NewGuid().ToString("D");
            string shortId = guid.Substring(5, 22);
            return shortId;
        }
    }
}
