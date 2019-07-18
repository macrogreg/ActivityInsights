using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights.Pipeline
{
    public sealed class ActivityPipeline : IActivityPipeline
    {
        public IList<IActivityProcessor> Processors
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
