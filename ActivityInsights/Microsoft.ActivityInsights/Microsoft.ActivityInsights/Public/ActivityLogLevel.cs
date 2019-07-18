using System;

namespace Microsoft.ActivityInsights
{
    public enum ActivityLogLevel
    {
        Debug = 10,
        Information = 20,
        Critical = 30
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static class ActivityLogLevelExtensions
    {
        public static string ToCategoryString(this ActivityLogLevel activityLogLevel)
        {
            const string otherStr = "Other";
            const string categorySuffix = "XX";

            switch (activityLogLevel)
            {
                case ActivityLogLevel.Debug:
                case ActivityLogLevel.Information:
                case ActivityLogLevel.Critical:
                    return activityLogLevel.ToString();
            }

            int logLevelInt = (int) activityLogLevel;

            if ((logLevelInt - (int) ActivityLogLevel.Debug) < 10)
            {
                return ActivityLogLevel.Debug.ToString() + categorySuffix;
            }

            if ((logLevelInt - (int) ActivityLogLevel.Information) < 10)
            {
                return ActivityLogLevel.Information.ToString() + categorySuffix;
            }

            if ((logLevelInt - (int) ActivityLogLevel.Critical) < 10)
            {
                return ActivityLogLevel.Critical.ToString() + categorySuffix;
            }

            return otherStr;
        }
    }
}
