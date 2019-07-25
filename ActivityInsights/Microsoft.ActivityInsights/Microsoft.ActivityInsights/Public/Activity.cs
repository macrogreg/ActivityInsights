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

        internal Activity(string activityName, ActivityLogLevel logLevel, string activityId, Activity root, Activity parent, LogicalExecutionStack logicalExecutionStack)
        {
            this.LogicalExecutionStack = Util.EnsureNotNull(logicalExecutionStack, nameof(logicalExecutionStack));

            this.Name = Util.EnsureNotNullOrEmpty(activityName, nameof(activityName));
            this.LogLevel = logLevel;
            this.Status = ActivityStatus.Running;

            this.ActivityId = Util.EnsureNotNullOrEmpty(activityId, nameof(activityId));
            this.RootActivity = root ?? this;
            this.ParentActivity = parent;

            this.StartTime = DateTimeOffset.Now;
            this.EndTime = default(DateTimeOffset);

            this.FaultException = null;
            this.FaultMessage = null;
            this.InitialFaultActivity = null;
            this.FaultId = null;
        }

        internal LogicalExecutionStack LogicalExecutionStack { get; }

        public string Name { get; }

        public ActivityLogLevel LogLevel { get; }

        public ActivityStatus Status { get; private set; }

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

        public Activity InitialFaultActivity { get; private set; }

        public string FaultId { get; private set; }

        internal void TransitionToFaulted(Exception exception, DateTimeOffset activityEndTime, string faultMessage, Activity initialFaultActivity, string faultId)
        {
            Util.EnsureNotNull(initialFaultActivity, nameof(initialFaultActivity));
            Util.EnsureNotNull(faultId, nameof(faultId));

            if (this.Status != ActivityStatus.Running)
            {
                ThrowCannotTransitionToFinalStatus(ActivityStatus.Faulted, this.Status);
            }

            this.Status = ActivityStatus.Faulted;
            this.EndTime = activityEndTime;

            this.FaultException = exception;
            this.FaultMessage = Util.SpellNull(faultMessage);
            this.InitialFaultActivity = initialFaultActivity;
            this.FaultId = faultId;
        }

        internal void TransitionToComplete(DateTimeOffset activityEndTime)
        {
            if (this.Status != ActivityStatus.Running)
            {
                ThrowCannotTransitionToFinalStatus(ActivityStatus.Completed, this.Status);
            }

            this.Status = ActivityStatus.Completed;
            this.EndTime = activityEndTime;
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

        private static void ThrowCannotTransitionToFinalStatus(ActivityStatus targetStatus, ActivityStatus currentStatus)
        {
            throw new InvalidOperationException($"Cannot transition {nameof(Activity)} to the '{targetStatus}'-status:"
                                              + $" Transition may only occur for an {nameof(Activity)} with"
                                              + $" {nameof(Status)} == {ActivityStatus.Running}."
                                              + $" However, the current {nameof(Status)} of this {nameof(Activity)} is '{currentStatus}'.");
        }
       
    }
}
