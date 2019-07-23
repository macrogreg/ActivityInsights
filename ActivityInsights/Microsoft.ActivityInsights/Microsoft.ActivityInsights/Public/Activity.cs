using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights
{
    public class Activity
    {
        private Activity _parentActivity;

        public Activity(string name) { }

        public string Name { get; }

        public string ActivityId { get; }

        public Activity RootActivity { get; }
        public bool TryGetParentActivity(out Activity parentActivity) { parentActivity = null; return false; }

        public ActivityLogLevel LogLevel { get; }

        public ActivityStatus Status { get; }

        

        public TimeSpan Duration { get; }
        public DateTimeOffset StartTime { get; }
        public DateTimeOffset EndTime { get; }

        public bool IsStatusFinal { get;  }
        

        public Activity SetLabel(string labelName, string labelValue) { return this; }
        public bool TryGetLabel(string labelName, out string labelValue)
        {
            
            throw new NotImplementedException();
            labelValue = null; return false;
        }

        public Activity SetMeasurement(string measurementName, double measurementValue) { return this; }
        public bool TryGetMeasurement(string measurementName, out double measurementValue)
        {
            

            throw new NotImplementedException();
            measurementValue = 0.0; return false;
        }

        internal IDictionary<string, string> Labels { get; }

        internal IDictionary<string, double> Measurements { get; }
        public Exception FaultException { get; internal set; }
        public string FaultMessage { get; internal set; }

        public Exception Fail(Exception exception) { return Fail(exception, rethrow: true); }
        public Exception Fail(Exception exception, bool rethrow) { return exception; }
        public void Fail(string failureMessage) { }

        public void Complete() { }

       
    }
}
