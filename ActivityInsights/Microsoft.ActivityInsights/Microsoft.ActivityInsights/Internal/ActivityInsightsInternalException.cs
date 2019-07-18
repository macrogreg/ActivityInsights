using System;

namespace Microsoft.ActivityInsights
{
    internal class ActivityInsightsInternalException : Exception
    {
        public ActivityInsightsInternalException(string message)
            : base(message)
        { }
    }
}
