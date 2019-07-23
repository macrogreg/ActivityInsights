using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Microsoft.ActivityInsights
{
    public class Activity
    {
        private Dictionary<string, string> _labels = null;
        private Dictionary<string, double> _measurements = null;

        private int _status;

        public Activity(string activityName, ActivityLogLevel logLevel, string activityId, Activity root, Activity parent)
        {
            this.Name = Util.EnsureNotNullOrEmpty(activityName, nameof(activityName));
            this.LogLevel = logLevel;
            _status = (int) ActivityStatus.Running;

            this.ActivityId = Util.EnsureNotNullOrEmpty(activityId, nameof(activityId));
            this.RootActivity = Util.EnsureNotNull(root, nameof(root));
            this.ParentActivity = parent;

            this.StartTime = DateTimeOffset.Now;
            this.EndTime = default(DateTimeOffset);

            this.FaultException = null;
            this.FaultMessage = null;
        }

        public string Name { get; }

        public ActivityLogLevel LogLevel { get; }

        public ActivityStatus Status { get { return (ActivityStatus) _status; } }

        public bool IsStatusFinal
        {
            get { return (this.Status == ActivityStatus.Completed) || (this.Status == ActivityStatus.Faulted); }
        }

        public string ActivityId { get; }

        public Activity RootActivity { get; }

        public Activity ParentActivity { get; }
        
        public DateTimeOffset StartTime { get; }

        public DateTimeOffset EndTime { get; private set; }

        public TimeSpan Duration
        {
            get
            {
                if (this.IsStatusFinal)
                {
                    return this.EndTime - this.StartTime;
                }

                DateTimeOffset now = DateTimeOffset.Now;
                return now - this.StartTime;
            }
        }

        public Activity SetLabel(string labelName, string labelValue)
        {
            labelName = Util.SpellNull(labelName);
            labelValue = Util.SpellNull(labelValue);

            Dictionary<string, string> labels = EnsureHasLabels();

            // Accessing labels concurrently is expected to be very rare. It's cheaper to take a lock.
            lock(_labels)
            {
                _labels[labelName] = labelValue;
            }

            return this;
        }

        public bool TryGetLabel(string labelName, out string labelValue)
        {
            Dictionary<string, string> labels = _labels;

            if (labels == null)
            {
                labelValue = null;
                return false;
            }

            labelName = Util.SpellNull(labelName);

            // Accessing labels concurrently is expected to be very rare. It's cheaper to take a lock.
            lock (labels)
            {
                return labels.TryGetValue(labelName, out labelValue);
            }
        }

        public Activity SetMeasurement(string measurementName, double measurementValue)
        {
            measurementName = Util.SpellNull(measurementName);

            Dictionary<string, double> measurements = EnsureHasMeasurements();

            // Accessing measurements concurrently is expected to be very rare. It's cheaper to take a lock.
            lock (_measurements)
            {
                _measurements[measurementName] = measurementValue;
            }

            return this;
        }

        public bool TryGetMeasurement(string measurementName, out double measurementValue)
        {
            {
                Dictionary<string, double> measurements = _measurements;

                if (measurements == null)
                {
                    measurementValue = default(double);
                    return false;
                }

                measurementName = Util.SpellNull(measurementName);

                // Accessing labels concurrently is expected to be very rare. It's cheaper to take a lock.
                lock (measurements)
                {
                    return measurements.TryGetValue(measurementName, out measurementValue);
                }
            }
        }

        internal IDictionary<string, string> Labels { get { return _labels; } }

        internal IDictionary<string, double> Measurements { get { return _measurements; } }

        public Exception FaultException { get; private set; }

        public string FaultMessage { get; private set; }

        public Exception FailAndRethrow(Exception exception)
        {
            Util.EnsureNotNull(exception, nameof(exception));

            string faultMsg = $"{exception.GetType().Name}: {exception.Message}";
            return TransitionToFail(exception, faultMsg, nameof(FailAndRethrow), rethrow: true);
        }

        public void FailAndSwallow(Exception exception)
        {
            Util.EnsureNotNull(exception, nameof(exception));

            string faultMsg = $"{exception.GetType().Name}: {exception.Message}";
            TransitionToFail(exception, faultMsg, nameof(FailAndSwallow), rethrow: false);
        }

        public void Fail(string failureMessage)
        {
            failureMessage = Util.SpellNull(failureMessage);
            TransitionToFail(null, failureMessage, nameof(Fail), rethrow: false);
        }

        private Exception TransitionToFail(Exception exception, string failureMessage, string transitionMethodName, bool rethrow)
        {
            int prevStatus = Interlocked.CompareExchange(ref _status, (int) ActivityStatus.Faulted, (int) ActivityStatus.Running);
            if (prevStatus != (int) ActivityStatus.Running)
            {
                ThrowCannotTransitionToFinalStatus(transitionMethodName, (ActivityStatus) prevStatus);
            }

            this.EndTime = DateTimeOffset.Now;

            this.FaultException = exception;
            this.FaultMessage = failureMessage;

            if (rethrow)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }

            return exception;
        }

        public void Complete()
        {
            int prevStatus = Interlocked.CompareExchange(ref _status, (int) ActivityStatus.Completed, (int) ActivityStatus.Running);
            if (prevStatus != (int) ActivityStatus.Running)
            {
                ThrowCannotTransitionToFinalStatus(nameof(Complete), (ActivityStatus) prevStatus);
            }

            this.EndTime = DateTimeOffset.Now;
        }

        private Dictionary<string, string> EnsureHasLabels()
        {
            Dictionary<string, string> labels = _labels;

            if (labels == null)
            {
                var newLabels = new Dictionary<string, string>();
                labels = Interlocked.CompareExchange(ref _labels, newLabels, null);
                labels = labels ?? newLabels;
            }

            return labels;
        }

        private Dictionary<string, double> EnsureHasMeasurements()
        {
            Dictionary<string, double> measurements = _measurements;

            if (measurements == null)
            {
                var newMeasurements = new Dictionary<string, double>();
                measurements = Interlocked.CompareExchange(ref _measurements, newMeasurements, null);
                measurements = measurements ?? newMeasurements;
            }

            return measurements;
        }

        private static void ThrowCannotTransitionToFinalStatus(string transitionMethodName, ActivityStatus actualStatus)
        {
            throw new InvalidOperationException($"'{transitionMethodName}(..)' may only be invoked on an {nameof(Activity)} with"
                                                + $" the {nameof(Status)} == {ActivityStatus.Running}."
                                                + $" However {nameof(Status)} of this {nameof(Activity)} is '{actualStatus}'.");
        }
       
    }
}
