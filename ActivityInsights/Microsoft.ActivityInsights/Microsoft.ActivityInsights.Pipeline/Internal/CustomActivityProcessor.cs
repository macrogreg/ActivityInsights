using System;
using Microsoft.ApplicationInsights;

namespace Microsoft.ActivityInsights.Pipeline
{
    internal sealed class CustomActivityProcessor : IActivityProcessor
    {
        private readonly string _name;
        private readonly ActivityProcessorAction _action;

        internal CustomActivityProcessor(string name, ActivityProcessorAction action)
        {
            _name = Util.SpellNull(name);
            _action = Util.EnsureNotNull(action, nameof(action));
        }

        public string Name { get { return _name; } }

        public void ProcessActivity(Activity activity, TelemetryClient applicationInsightsClient, out bool continueProcessing)
        {
            _action(activity, applicationInsightsClient, out continueProcessing);
        }
    }

}
