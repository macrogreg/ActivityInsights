using System;
using System.Threading;
using Microsoft.ActivityInsights.Pipeline;

namespace Microsoft.ActivityInsights
{
    public class ActivityInsightsLogger
    {
        private IActivityPipeline _pipeline;
        private readonly AsyncLocal<LogicalExecutionStack> _logicalExecutionThread = new AsyncLocal<LogicalExecutionStack>();

        // Opertions protected by this lock are expeected to be rarely invoked concurrently.
        // We do not expect any contention on this lock, it should be very fast.
        private readonly object _lock = new object();

        public ActivityInsightsLogger()
            : this(ActivityPipelineDefaults.CreateForSendOnly())
        {
        }

        public ActivityInsightsLogger(IActivityPipeline pipeline)
        {
            _pipeline = Util.EnsureNotNull(pipeline, nameof(pipeline));
        }

        public IActivityPipeline Pipeline
        {
            get { return _pipeline; }
            set { _pipeline = Util.EnsureNotNull(value, nameof(value)); }
        }

        public Activity StartNewActivity(string activityName, ActivityLogLevel logLevel)
        {
            activityName = ValidateActivityName(activityName);

            lock (_lock)
            {
                LogicalExecutionStack currentLogicalStack = _logicalExecutionThread.Value;

                if (currentLogicalStack == null)
                {
                    currentLogicalStack = new LogicalExecutionStack(parentStack: null);
                    _logicalExecutionThread.Value = currentLogicalStack;
                }

                Activity parent = currentLogicalStack.Peek(); // may be null

                var activity = new Activity(activityName, logLevel, Util.CreateRandomId(), parent?.RootActivity, parent);
                currentLogicalStack.Push(activity);

                return activity;
            }
        }

        public Activity StartNewLogicalActivityThread(string activityName, ActivityLogLevel logLevel)
        {
            activityName = ValidateActivityName(activityName);

            lock (_lock)
            {
                LogicalExecutionStack currentLogicalStack = _logicalExecutionThread.Value;

                var newLogicalStack = new LogicalExecutionStack(currentLogicalStack);
                _logicalExecutionThread.Value = newLogicalStack;

                Activity parent = currentLogicalStack.Peek(); // may be null

                var activity = new Activity(activityName, logLevel, Util.CreateRandomId(), parent?.RootActivity, parent);
                newLogicalStack.Push(activity);

                return activity;
            }
        }

        public void CompleteActivity()
        {
            Activity activity;
            lock (_lock)
            {
                LogicalExecutionStack logicalStack = _logicalExecutionThread.Value;

                activity = logicalStack.Pop();
                if (activity == null)
                {
                    // log mismatch between start/complete
                }

                activity.Complete();

                if (logicalStack.Count == 0)
                {
                    _logicalExecutionThread.Value = logicalStack.ParentStack;
                }
            }

            _pipeline.ProcessAndSend(activity);
        }

        private static string ValidateActivityName(string activityName)
        {
            Util.EnsureNotNull(activityName, nameof(activityName));
            activityName = activityName.Trim();

            return (activityName.Length == 0) ? "_" : activityName;
        }
    }
}
