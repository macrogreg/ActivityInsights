
using System;
using Microsoft.ActivityInsights.Pipeline;

namespace Microsoft.ActivityInsights
{
    public static class ActivityInsights
    {
        private static ActivityInsightsLog s_log;

        static ActivityInsights()
        {
            IActivityPipeline defaultPipeline = ActivityPipelineDefaults.CreateForDevelopmentPhase();
            ActivityInsightsLog defaultLog = new ActivityInsightsLog(defaultPipeline);
            s_log = defaultLog;
        }


        public static ActivityInsightsLog Log { get { return s_log; } }

        public static IActivityPipeline Pipeline { get { return s_log.Pipeline; } }

        public static void SetLog(ActivityInsightsLog log)
        {
            s_log = Util.EnsureNotNull(log, nameof(log));
        }

        public static void SetPipeline(IActivityPipeline pipeline)
        {
            s_log.Pipeline = Util.EnsureNotNull(pipeline, nameof(pipeline));
        }

    }
}
