using System;
using Microsoft.ApplicationInsights;

namespace Microsoft.ActivityInsights.Pipeline
{
    public interface IActivityProcessor
    {
        string Name { get; }
        void ProcessActivity(Activity activity, TelemetryClient applicationInsightsClient, out bool continueProcessing);
    }
}
