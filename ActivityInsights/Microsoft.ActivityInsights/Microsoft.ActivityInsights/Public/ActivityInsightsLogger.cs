using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using Microsoft.ActivityInsights.Pipeline;

namespace Microsoft.ActivityInsights
{
    public class ActivityInsightsLogger
    {
        private const string ExceptionMsg_ActivityStartEndMismatch = "Cannot access current activity becasue there is no valid Logical Activity Execution Stack."
                                                                   + " Did you mismatch the number of calls to StartXxxActivity(..) and to Complete/FailXxxActivity(..)?";

        private const string ExceptionMsg_ActivityStacksMismatch = "The current Logical Activity Execution Stack is not the same Stack as the one where the specified Activity was created."
                                                                 + " Did you mismatch the number of calls to StartXxxActivity(..) and to Complete/FailXxxActivity(..)?";

        private IActivityPipeline _pipeline;
        private readonly AsyncLocal<LogicalExecutionStack> _logicalExecutionThread = new AsyncLocal<LogicalExecutionStack>();

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

            LogicalExecutionStack currentLogicalStack = GetCurrentLogicalStack(createIfNotExists: true);

            lock (currentLogicalStack)
            {
                Activity parent = currentLogicalStack.Peek(); // may be null

                var activity = new Activity(activityName, logLevel, Util.CreateRandomId(), parent?.RootActivity, parent, currentLogicalStack);
                currentLogicalStack.Push(activity);

                return activity;
            }
        }

        private LogicalExecutionStack GetCurrentLogicalStack(bool createIfNotExists)
        {
            LogicalExecutionStack currentLogicalStack = _logicalExecutionThread.Value;

            if (currentLogicalStack != null)
            {
                return currentLogicalStack;
            }

            LogicalExecutionStack newLogicalStack = new LogicalExecutionStack(parentStack: null);

            lock (_logicalExecutionThread)
            {
                currentLogicalStack = _logicalExecutionThread.Value;
                if (currentLogicalStack != null)
                {
                    return currentLogicalStack;
                }

                _logicalExecutionThread.Value = newLogicalStack;
                return currentLogicalStack;
            }
        }

        public Activity StartNewLogicalActivityThread(string activityName, ActivityLogLevel logLevel)
        {
            activityName = ValidateActivityName(activityName);

            LogicalExecutionStack currentLogicalStack = GetCurrentLogicalStack(createIfNotExists: false);

            object lockScope = currentLogicalStack;
            lockScope = lockScope ?? _logicalExecutionThread;

            lock(lockScope)
            {
                Activity parent = currentLogicalStack?.Peek();  // may be null

                var newLogicalStack = new LogicalExecutionStack(currentLogicalStack);

                var activity = new Activity(activityName, logLevel, Util.CreateRandomId(), parent?.RootActivity, parent, newLogicalStack);
                newLogicalStack.Push(activity);

                _logicalExecutionThread.Value = newLogicalStack;

                return activity;
            }
        }

        public void CompleteCurrentActivity()
        {
            DateTimeOffset now = DateTimeOffset.Now;

            LogicalExecutionStack logicalStack = GetCurrentLogicalStack(createIfNotExists: false);
            if (logicalStack == null)
            {
                throw new InvalidOperationException(ExceptionMsg_ActivityStartEndMismatch);
            }

            Activity activity;
            lock (logicalStack)
            { 
                activity = logicalStack.Pop();

                if (logicalStack.Count == 0)
                {
                    _logicalExecutionThread.Value = logicalStack.ParentStack;
                }
            }

            activity.TransitionToComplete(now);
            _pipeline.ProcessAndSend(activity);
        }

        public void FailActivity(Activity activity, string failureMessage)
        {
            Util.EnsureNotNull(activity, nameof(activity));
            FailActivity(activity, exception: null, failureMessage);
        }

        public void FailActivityAndSwallow(Activity activity, Exception exception)
        {
            Util.EnsureNotNull(activity, nameof(activity));
            Util.EnsureNotNull(exception, nameof(exception));

            string failureMessage = String.IsNullOrEmpty(exception.Message)
                                        ? exception.GetType().Name
                                        : $"{exception.GetType().Name}: {exception.Message}";

            FailActivity(activity, exception, failureMessage);
        }

        public Exception FailActivityAndRethrow(Activity activity, Exception exception)
        {
            Util.EnsureNotNull(activity, nameof(activity));
            Util.EnsureNotNull(exception, nameof(exception));

            string failureMessage = String.IsNullOrEmpty(exception.Message)
                                        ? exception.GetType().Name
                                        : $"{exception.GetType().Name}: {exception.Message}";

            FailActivity(activity, exception, failureMessage);

            ExceptionDispatchInfo.Capture(exception).Throw();
            return exception;   // line never reached
        }

        public void FailCurrentActivity(string failureMessage)
        {
            FailCurrentActivity(exception: null, failureMessage);
        }

        public void FailCurrentActivityAndSwallow(Exception exception)
        {
            Util.EnsureNotNull(exception, nameof(exception));

            string failureMessage = String.IsNullOrEmpty(exception.Message)
                                        ? exception.GetType().Name
                                        : $"{exception.GetType().Name}: {exception.Message}";

            FailCurrentActivity(exception, failureMessage);
        }

        public Exception FailCurrentActivityAndRethrow(Exception exception)
        {
            Util.EnsureNotNull(exception, nameof(exception));

            string failureMessage = String.IsNullOrEmpty(exception.Message)
                                        ? exception.GetType().Name
                                        : $"{exception.GetType().Name}: {exception.Message}";

            FailCurrentActivity(exception, failureMessage);

            ExceptionDispatchInfo.Capture(exception).Throw();
            return exception;   // line never reached
        }


        private void FailActivity(Activity activity, Exception exception, string failureMessage)
        {
            DateTimeOffset now = DateTimeOffset.Now;

            LogicalExecutionStack logicalStack = GetCurrentLogicalStack(createIfNotExists: false);

            if (logicalStack == null)
            {
                throw new InvalidOperationException(ExceptionMsg_ActivityStartEndMismatch);
            }

            if (logicalStack != activity.LogicalExecutionStack)
            {
                throw new InvalidOperationException(ExceptionMsg_ActivityStacksMismatch);
            }

            var faultedActivities = new List<Activity>();
            lock(logicalStack)
            {
                Activity poppedActivity;
                do
                {
                    poppedActivity = logicalStack.Pop();
                    faultedActivities.Add(poppedActivity);
                }
                while (poppedActivity != activity);

                if (logicalStack.Count == 0)
                {
                    _logicalExecutionThread.Value = logicalStack.ParentStack;
                }
            }

            string faultId = Util.CreateRandomId();

            for (int i = 0; i < faultedActivities.Count; i++)
            {
                faultedActivities[i].TransitionToFaulted(exception, now, failureMessage, activity, faultId);
                _pipeline.ProcessAndSend(faultedActivities[i]);
            }
        }


        private void FailCurrentActivity(Exception exception, string failureMessage)
        {
            DateTimeOffset now = DateTimeOffset.Now;

            LogicalExecutionStack logicalStack = GetCurrentLogicalStack(createIfNotExists: false);
            if (logicalStack == null)
            {
                throw new InvalidOperationException(ExceptionMsg_ActivityStartEndMismatch);
            }

            Activity activity;
            lock (logicalStack)
            {
                activity = logicalStack.Pop();

                if (logicalStack.Count == 0)
                {
                    _logicalExecutionThread.Value = logicalStack.ParentStack;
                }
            }

            activity.TransitionToFaulted(exception, now, failureMessage, activity, Util.CreateRandomId());
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
