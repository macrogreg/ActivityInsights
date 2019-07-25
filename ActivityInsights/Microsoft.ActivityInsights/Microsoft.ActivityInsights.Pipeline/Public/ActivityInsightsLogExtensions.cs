using System;

namespace Microsoft.ActivityInsights.Pipeline
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static class ActivityInsightsLogExtensions
    {
        public static IActivityPipeline GetPipeline(this ActivityInsightsLog activityInsightsLog)
        {
            Util.EnsureNotNull(activityInsightsLog, nameof(activityInsightsLog));
            return activityInsightsLog.Pipeline;
        }

        public static IActivityPipeline SetPipeline(this ActivityInsightsLog activityInsightsLog, IActivityPipeline pipeline)
        {
            Util.EnsureNotNull(activityInsightsLog, nameof(activityInsightsLog));
            Util.EnsureNotNull(pipeline, nameof(pipeline));

            return activityInsightsLog.Pipeline = pipeline;
        }
    }
}
