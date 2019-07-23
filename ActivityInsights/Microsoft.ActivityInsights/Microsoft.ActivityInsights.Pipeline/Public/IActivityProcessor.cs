using System;
using Microsoft.ApplicationInsights;

namespace Microsoft.ActivityInsights.Pipeline
{
    public interface IActivityProcessor
    {
        string Name { get; }
        void ProcessActivity(Activity activity, out bool continueProcessing);
    }
}
