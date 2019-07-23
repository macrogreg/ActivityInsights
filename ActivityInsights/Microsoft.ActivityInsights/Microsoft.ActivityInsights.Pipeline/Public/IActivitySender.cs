using System;

namespace Microsoft.ActivityInsights.Pipeline
{
    public interface IActivitySender
    {
        string Name { get; }

        void SendActivity(Activity activity);

        void LogActivityInsightsError(Exception exception);
    }
}
