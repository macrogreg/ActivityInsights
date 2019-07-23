using System;
using Microsoft.ActivityInsights.Pipeline;

namespace Microsoft.ActivityInsights
{
    public class ActivityInsightsLogger
    {
        private IActivityPipeline _pipeline;

        public ActivityInsightsLogger()
            : this(ActivityPipelineDefaults.CreateForSendOnly())
        {
        }

        public ActivityInsightsLogger(IActivityPipeline pipeline)
        {
            _pipeline = Util.EnsureNotNull(pipeline, nameof(pipeline));
        }

        public IActivityPipeline GetPipeline()
        {
            return _pipeline;
        }

        public void SetPipeline(IActivityPipeline pipeline)
        {
            _pipeline = Util.EnsureNotNull(pipeline, nameof(pipeline));
        }
    }
}
