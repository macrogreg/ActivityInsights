using System;

namespace Microsoft.ActivityInsights
{
    public class Activity
    {
        //public static class ReservedNames
        //{
        //    public static class Labels
        //    {
        //        public const string Name = "Activity.Name";
        //        public const string ActivityId = "Activity.Id";
        //        public const string RootId = "Activity.RootId";
        //        public const string ParentId = "Activity.ParentId";
        //        public const string Status = "Activity.Status";
        //        public const string StartTimeUtc = "Activity.StartTimeUtc";
        //        public const string EndTimeUtc = "Activity.EdnTimeUtc";
        //    }

        //    public static class Measurements
        //    {
        //        public const string DurationMSecs = "Activity.DutationMSecs";
        //    }
        //}
        

        private Activity _parentActivity;

        public Activity(string name) { }

        public string Name { get; }

        public string ActivityId { get; }

        public ActivityStatus Status { get; }

        public ActivityLogLevel LogLevel { get; }

        public TimeSpan Duration { get; }
        public DateTime StartTimeUtc { get; }
        public DateTime EndTimeUtc { get; }

        public Activity RootActivity { get; }
        public bool TryGetParentActivity(out Activity parentActivity) { parentActivity = null; return false; } 

        public Activity SetLabel(string labelName, string labelValue) { return this; }
        public bool TryGetLabel(string labelName, out string labelValue)
        {
            //labelName = Util.SpellNull(labelName);

            //if (ReservedNames.Labels.Name.Equals(labelName, StringComparison.Ordinal))
            //{
            //    labelValue = this.Name; return true;
            //}

            //if (ReservedNames.Labels.ActivityId.Equals(labelName, StringComparison.Ordinal))
            //{
            //    labelValue = this.ActivityId; return true;
            //}

            //if (ReservedNames.Labels.RootId.Equals(labelName, StringComparison.Ordinal))
            //{
            //    labelValue = this.RootActivity.ActivityId; return true;
            //}

            //if (ReservedNames.Labels.ParentId.Equals(labelName, StringComparison.Ordinal))
            //{
            //    Activity parentActivity;
            //    if (! this.TryGetParentActivity(out parentActivity))
            //    {
            //        labelValue = null; return false;
            //    }

            //    labelValue = parentActivity.ActivityId; return true;
            //}

            //if (ReservedNames.Labels.Status.Equals(labelName, StringComparison.Ordinal))
            //{
            //    labelValue = this.Status.ToString(); return true;
            //}

            //if (ReservedNames.Labels.StartTimeUtc.Equals(labelName, StringComparison.Ordinal))
            //{
            //    labelValue = this.StartTimeUtc.ToString("o"); return true;
            //}

            //if (ReservedNames.Labels.EndTimeUtc.Equals(labelName, StringComparison.Ordinal))
            //{
            //    labelValue = this.EndTimeUtc.ToString("o"); return true;
            //}

            throw new NotImplementedException();
            labelValue = null; return false;
        }

        public Activity SetMeasurement(string measurementName, double measurementValue) { return this; }
        public bool TryGetMeasurement(string measurementName, out double measurementValue)
        {
            //measurementName = Util.SpellNull(measurementName);

            //if (ReservedNames.Measurements.DurationMSecs.Equals(measurementName, StringComparison.Ordinal))
            //{
            //    measurementValue = this.Duration.TotalMilliseconds; return true;
            //}

            throw new NotImplementedException();
            measurementValue = 0.0; return false;
        }

        public Exception Fail(Exception exception) { return Fail(exception, rethrow: true); }
        public Exception Fail(Exception exception, bool rethrow) { return exception; }
        public void Fail(string failureMessage) { }

        public void Complete() { }

       
    }
}
