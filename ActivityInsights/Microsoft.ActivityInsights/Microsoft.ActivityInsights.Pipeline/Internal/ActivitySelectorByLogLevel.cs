using System;

namespace Microsoft.ActivityInsights.Pipeline
{
    internal class ActivitySelectorByLogLevel
    {
        private readonly ActivityLogLevel _logLevel;

        public ActivitySelectorByLogLevel(ActivityLogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public bool IsSmaller(Activity activity)
        {
            if (activity == null)
            {
                return false;
            }

            return ((int)(activity.LogLevel)) < ((int)_logLevel);
        }

        public bool IsSmallerOrEqual(Activity activity)
        {
            if (activity == null)
            {
                return false;
            }

            return ((int)(activity.LogLevel)) <= ((int)_logLevel);
        }

        public bool IsEqual(Activity activity)
        {
            if (activity == null)
            {
                return false;
            }

            return ((int)(activity.LogLevel)) == ((int)_logLevel);
        }

        public bool IsLargerOrEqual(Activity activity)
        {
            if (activity == null)
            {
                return false;
            }

            return ((int)(activity.LogLevel)) >= ((int)_logLevel);
        }

        public bool IsLarger(Activity activity)
        {
            if (activity == null)
            {
                return false;
            }

            return ((int)(activity.LogLevel)) > ((int)_logLevel);
        }
    }
}
