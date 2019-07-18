using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights.Pipeline
{
    public class ActivitySerializer
    {
        public static void AddCoreActivityMetadata(Activity activity, IDictionary<string, string> labels, IDictionary<string, double> measurements)
        {
            Util.EnsureNotNull(activity, nameof(activity));
            Util.EnsureNotNull(labels, nameof(labels));
            Util.EnsureNotNull(measurements, nameof(measurements));

            Activity parentActivity;
            if (!activity.TryGetParentActivity(out parentActivity))
            {
                parentActivity = null;
            }

            labels["Activity.Name"] = activity.Name;
            labels["Activity.Id"] = activity.ActivityId;
            labels["Activity.RootId"] = activity.RootActivity.ActivityId;
            labels["Activity.ParentId"] = Util.SpellNull(parentActivity?.ActivityId);
            labels["Activity.Status"] = activity.Status.ToString();
            labels["Activity.StartTimeUtc"] = activity.EndTimeUtc.ToString("o");
            labels["Activity.EndTimeUtc"] = activity.EndTimeUtc.ToString("o");

            measurements["Activity.DurationMSecs"] = activity.Duration.TotalMilliseconds;

            //bool ok;
            //string str;

            //ok = activity.TryGetLabel(Activity.ReservedNames.Labels.Name, out str);
            //labels[Activity.ReservedNames.Labels.Name] = ok ? str : Util.NullString;

            //ok = activity.TryGetLabel(Activity.ReservedNames.Labels.ActivityId, out str);
            //labels[Activity.ReservedNames.Labels.ActivityId] = ok ? str : Util.NullString;

            //ok = activity.TryGetLabel(Activity.ReservedNames.Labels.RootId, out str);
            //labels[Activity.ReservedNames.Labels.RootId] = ok ? str : Util.NullString;

            //ok = activity.TryGetLabel(Activity.ReservedNames.Labels.ParentId, out str);
            //labels[Activity.ReservedNames.Labels.ParentId] = ok ? str : Util.NullString;

            //ok = activity.TryGetLabel(Activity.ReservedNames.Labels.Status, out str);
            //labels[Activity.ReservedNames.Labels.Status] = ok ? str : Util.NullString;

            //ok = activity.TryGetLabel(Activity.ReservedNames.Labels.StartTimeUtc, out str);
            //labels[Activity.ReservedNames.Labels.StartTimeUtc] = ok ? str : Util.NullString;

            //ok = activity.TryGetLabel(Activity.ReservedNames.Labels.EndTimeUtc, out str);
            //labels[Activity.ReservedNames.Labels.EndTimeUtc] = ok ? str : Util.NullString;

            //double num;

            //ok = activity.TryGetMeasurement(Activity.ReservedNames.Measurements.DurationMSecs, out num);
            //measurements[Activity.ReservedNames.Measurements.DurationMSecs] = ok ? num : Double.NaN;
        }
    }
}
