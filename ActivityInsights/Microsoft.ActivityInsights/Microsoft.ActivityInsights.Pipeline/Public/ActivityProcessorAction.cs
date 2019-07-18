using System;
using Microsoft.ApplicationInsights;

namespace Microsoft.ActivityInsights.Pipeline
{
    public delegate void ActivityProcessorAction(Activity activity, TelemetryClient applicationInsightsClient, out bool continueProcessing);
}
