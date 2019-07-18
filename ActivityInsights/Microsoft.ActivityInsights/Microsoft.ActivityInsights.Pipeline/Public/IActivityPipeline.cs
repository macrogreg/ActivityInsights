using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights.Pipeline
{
    public interface IActivityPipeline
    {
        IList<IActivityProcessor> Processors { get; }
    }
}
