using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights
{
    internal class ActivityInsightsException : Exception
    {
        public Dictionary<string, string> DetailLabels { get; }
        public Dictionary<string, double> DetailMeasures { get; }

        public ActivityInsightsException(string message, Dictionary<string, string> detailLabels, Dictionary<string, double> detailMeasures)
            : base(message)
        {
            this.DetailLabels = detailLabels;
            this.DetailMeasures = detailMeasures;
        }
    }
}
