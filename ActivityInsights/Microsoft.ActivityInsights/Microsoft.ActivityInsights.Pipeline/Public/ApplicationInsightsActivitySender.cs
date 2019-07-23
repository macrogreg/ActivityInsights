using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights.Pipeline
{
    public class ApplicationInsightsActivitySender : IActivitySender
    {
        public const string ItemSourceLabelName = "ItemSource";
        public const string ItemSourceLabelValue = "Microsoft.ActivityInsights";

        public const string ExceptionIdLabel = "Activity.ExceptionId";

        private readonly TelemetryClient _applicationInsightsClient;

        public string Name { get; }

        public TelemetryClient ApplicationInsightsClient { get { return _applicationInsightsClient; } }

        public ApplicationInsightsActivitySender(string senderName, TelemetryClient applicationInsightsClient)
        {
            this.Name = Util.SpellNull(senderName);
            _applicationInsightsClient = Util.EnsureNotNull(applicationInsightsClient, nameof(applicationInsightsClient));
        }

        public void LogActivityInsightsError(Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            Dictionary<string, string> detailLabels = null;
            Dictionary<string, double> detailMeasures = null;

            ActivityInsightsException insightsException = exception as ActivityInsightsException;
            if (insightsException != null)
            {
                detailLabels = insightsException.DetailLabels;
                detailMeasures = insightsException.DetailMeasures;
            }

            detailLabels = detailLabels ?? new Dictionary<string, string>();
            detailLabels[ItemSourceLabelName] = ItemSourceLabelValue;

            _applicationInsightsClient.TrackException(insightsException ?? exception, detailLabels, detailMeasures);
        }

        public void SendActivity(Activity activity)
        {
            if (activity == null)
            {
                return;
            }

            EventTelemetry activityTelemetry = new EventTelemetry(activity.Name);
            activityTelemetry.Properties[ItemSourceLabelName] = ItemSourceLabelValue;

            activityTelemetry.Timestamp = activity.IsStatusFinal ? activity.EndTime.ToUniversalTime() : DateTimeOffset.UtcNow;
            ActivitySerializer.AddActivityData(activity, activityTelemetry.Properties, activityTelemetry.Metrics);
            
            if (activity.Status == ActivityStatus.Faulted)
            {
                string exceptionRefId = Util.NullString;
                Exception exception = activity.FaultException;

                if (exception != null)
                {
                    ExceptionTelemetry exceptionTelemetry = new ExceptionTelemetry(exception);
                    exceptionTelemetry.Properties[ItemSourceLabelName] = ItemSourceLabelValue;

                    ActivitySerializer.AddActivityFailureRelatedMetadata(activity, exceptionTelemetry.Properties);

                    exceptionRefId = Util.CreateRandomId();
                    exceptionTelemetry.Properties[ExceptionIdLabel] = exceptionRefId;

                    _applicationInsightsClient.TrackException(exceptionTelemetry);
                }

                activityTelemetry.Properties[ExceptionIdLabel] = exceptionRefId;
            }

            _applicationInsightsClient.TrackEvent(activityTelemetry);
        }
    }
}
