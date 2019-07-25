
using System;
using System.Collections.Generic;
using System.Linq;

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

        private static readonly char[] HexChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        public static string CreateRandomId()
        {
            // We shorten a guid becasue locally the chars we use are "unique-enough:: 
            // "708F5CC4-7F9C-468F-B21E-7153A88E24DE"
            //      "CC4-7F9C-468F-B21E-715"
            //
            //string guid = Guid.NewGuid().ToString("D");
            //string shortId = guid.Substring(5, 22);
            //return shortId;

            // 10 hex digits (like a phone number) are easier to read than the above and are sufficient:

            // XXX-XXX-XXXX
            // 012345678901

            const long mask1111 = 0xF;
            const int shiftLen = 4; 

            long rndNum = RandomXorShift.Next64();
            char[] rndChars = new char[12];

            rndChars[11] = HexChars[rndNum & mask1111];

            rndNum >>= shiftLen;
            rndChars[10] = HexChars[rndNum & mask1111];

            rndNum >>= shiftLen;
            rndChars[9] = HexChars[rndNum & mask1111];

            rndNum >>= shiftLen;
            rndChars[8] = HexChars[rndNum & mask1111];

            rndChars[7] = '-';

            rndNum >>= shiftLen;
            rndChars[6] = HexChars[rndNum & mask1111];

            rndNum >>= shiftLen;
            rndChars[5] = HexChars[rndNum & mask1111];

            rndNum >>= shiftLen;
            rndChars[4] = HexChars[rndNum & mask1111];

            rndChars[3] = '-';

            rndNum >>= shiftLen;
            rndChars[2] = HexChars[rndNum & mask1111];

            rndNum >>= shiftLen;
            rndChars[1] = HexChars[rndNum & mask1111];

            rndNum >>= shiftLen;
            rndChars[0] = HexChars[rndNum & mask1111];

            string rndStr = new String(rndChars);
            return rndStr;
        }
    }
}
