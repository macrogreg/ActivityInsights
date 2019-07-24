using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights
{
    internal class LogicalExecutionStack
    {
        private readonly List<Activity> _activities = new List<Activity>();

        public LogicalExecutionStack(LogicalExecutionStack parentStack)
        {
            this.ParentStack = parentStack;
        }

        public LogicalExecutionStack ParentStack { get; }

        public void Push(Activity activity)
        {
            Util.EnsureNotNull(activity, nameof(activity));

            _activities.Add(activity);
        }

        public Activity Pop()
        {
            if (_activities.Count == 0)
            {
                return null;
            }

            Activity activity = _activities[_activities.Count - 1];
            _activities.RemoveAt(_activities.Count - 1);
            return activity;
        }

        public Activity Peek()
        {
            if (_activities.Count == 0)
            {
                return null;
            }

            Activity activity = _activities[_activities.Count - 1];
            return activity;
        }

        public int Count { get { return _activities.Count; } }
    }
}
