using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights.Pipeline
{
    public interface IActivityPipeline
    {
        IList<IActivityProcessor> Processors { get; }

        IList<IActivitySender> Senders { get; }

        //void ProcessAndSend(Activity activity);
    }
}
