using System;

namespace Microsoft.ActivityInsights.Pipeline
{
    public delegate void ActivityProcessorAction(Activity activity, out bool continueProcessing);
}
