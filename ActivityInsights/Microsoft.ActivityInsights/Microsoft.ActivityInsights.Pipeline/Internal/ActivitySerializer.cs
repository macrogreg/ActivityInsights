using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights.Pipeline
{
    internal class ActivitySerializer
    {
        public static void AddActivityCoreMetadata(Activity activity, IDictionary<string, string> labels, IDictionary<string, double> measurements)
        {
            Util.EnsureNotNull(activity, nameof(activity));
            Util.EnsureNotNull(labels, nameof(labels));
            Util.EnsureNotNull(measurements, nameof(measurements));

            labels["Activity.Name"] = activity.Name;
            labels["Activity.Id"] = activity.ActivityId;
            labels["Activity.RootId"] = activity.RootActivity.ActivityId;

            Activity parentActivity;
            bool hasParent = activity.TryGetParentActivity(out parentActivity);
            labels["Activity.ParentId"] = hasParent ? parentActivity.ActivityId : Util.NullString;

            labels["Activity.LogLevel"] = activity.LogLevel.ToString();

            labels["Activity.Status"] = activity.Status.ToString();
            labels["Activity.StartTimeUtc"] = activity.StartTime.UtcDateTime.ToString("o");
            labels["Activity.EndTimeUtc"] = activity.IsStatusFinal ? activity.EndTime.UtcDateTime.ToString("o") : Util.NullString;

            if (activity.Status == ActivityStatus.Faulted)
            {
                labels["Activity.Status"] = Util.SpellNull(activity.FaultMessage);
            }

            measurements["Activity.DurationMSecs"] = activity.Duration.TotalMilliseconds;
        }

        public static void AddActivityFailureRelatedMetadata(Activity activity, IDictionary<string, string> labels)
        {
            Util.EnsureNotNull(activity, nameof(activity));
            Util.EnsureNotNull(labels, nameof(labels));

            labels["Activity.Name"] = activity.Name;

            labels["Activity.Id"] = activity.ActivityId;
            labels["Activity.RootId"] = activity.RootActivity.ActivityId;
            labels["Activity.LogLevel"] = activity.LogLevel.ToString();

            labels["Activity.StartTimeUtc"] = activity.StartTime.UtcDateTime.ToString("o");
            labels["Activity.EndTimeUtc"] = activity.EndTime.UtcDateTime.ToString("o");
        }

        public static void AddActivityLabels(Activity activity, IDictionary<string, string> serializedLabels)
        {
            Util.EnsureNotNull(activity, nameof(activity));
            Util.EnsureNotNull(serializedLabels, nameof(serializedLabels));

            IDictionary<string, string> activityLabels = activity.Labels;

            if (activityLabels != null)
            {
                foreach (KeyValuePair<string, string> activityLabel in activityLabels)
                {
                    serializedLabels[Util.SpellNull(activityLabel.Key)] = Util.SpellNull(activityLabel.Value);
                }
            }
        }

        public static void AddActivityMeasurements(Activity activity, IDictionary<string, double> serializedMeasurements)
        {
            Util.EnsureNotNull(activity, nameof(activity));
            Util.EnsureNotNull(serializedMeasurements, nameof(serializedMeasurements));

            IDictionary<string, double> activityMeasurements = activity.Measurements;

            if (activityMeasurements != null)
            {
                foreach (KeyValuePair<string, double> activityMeasurement in activityMeasurements)
                {
                    serializedMeasurements[Util.SpellNull(activityMeasurement.Key)] = activityMeasurement.Value;
                }
            }
        }

        public static void AddActivityData(Activity activity, IDictionary<string, string> labels, IDictionary<string, double> measurements)
        {
            AddActivityCoreMetadata(activity, labels, measurements);
            AddActivityLabels(activity, labels);
            AddActivityMeasurements(activity, measurements);
        }
    }
}
