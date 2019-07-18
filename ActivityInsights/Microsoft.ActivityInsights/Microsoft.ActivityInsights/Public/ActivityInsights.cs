using Microsoft.ActivityInsights;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ActivityInsights
{
    public static class ActivityInsights
    {
        public static Activity StartNewActivity(string activityName) { return null; }
        public static Activity StartNewActivity(string activityNamePrefix, string activityNamePostfix) { return null; }

        public static ActivityInsightsLogger GetCurrentLogger()
        {
            return null;
        }
    }
}
