
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
                throw (variableName == null) ? new ArgumentException("The specified string argument may not be empty.") : new ArgumentNullException($"\"{variableName}\" may not be empty.");
            }

            return value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static string SpellNull(string value)
        {
            return value ?? NullString;
        }

        public static void LogInternalError(TelemetryClient applicationInsightsClient, string message, IDictionary<string, string> detailLabels, IDictionary<string, double> detailMeasures)
        {
            EnsureNotNull(applicationInsightsClient, nameof(applicationInsightsClient));
            message = SpellNull(message);

            // Throw-Catch exception to initialize the stack and then log it:
            try
            {
                throw new ActivityInsightsInternalException(message);
            }
            catch (Exception ex)
            {
                applicationInsightsClient.TrackException(ex, detailLabels, detailMeasures);
                applicationInsightsClient.Flush();
            }
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
    }
}
