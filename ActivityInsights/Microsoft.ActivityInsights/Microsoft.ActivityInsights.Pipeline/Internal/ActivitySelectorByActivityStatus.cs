using System;

namespace Microsoft.ActivityInsights.Pipeline
{
    internal class ActivitySelectorByActivityStatus
    {
        private readonly ActivityStatus _activityStatus;

        public ActivitySelectorByActivityStatus(ActivityStatus activityStatus)
        {
            _activityStatus = activityStatus;
        }

        public bool IsEqual(Activity activity)
        {
            return activity?.Status == _activityStatus;
        }

        public bool IsNotEqual(Activity activity)
        {
            return activity?.Status != _activityStatus;
        }
    }
}
