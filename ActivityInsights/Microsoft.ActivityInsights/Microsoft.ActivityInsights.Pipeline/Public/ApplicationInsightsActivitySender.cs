using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

using DistributedTracingActivity = System.Diagnostics.Activity;

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
            
            // We need to only log exceptions once per fault, togehter with the activity that was faulted explicitly.
            if (activity.Status == ActivityStatus.Faulted && activity.InitialFaultActivity == activity)
            {
                Exception exception = activity.FaultException;
                if (exception != null)
                {
                    ExceptionTelemetry exceptionTelemetry = new ExceptionTelemetry(exception);
                    exceptionTelemetry.Properties[ItemSourceLabelName] = ItemSourceLabelValue;

                    ActivitySerializer.AddActivityMetadataForExceptions(activity, exceptionTelemetry.Properties);

                    _applicationInsightsClient.TrackException(exceptionTelemetry);
                }
            }

            _applicationInsightsClient.TrackEvent(activityTelemetry);
        }

        internal IDisposable StartActivityOperation<TOperation>(
                                    string name,
                                    string operationId = null,
                                    string parentOperationId = null,
                                    string globalOperationId = null)
                            where TOperation : OperationTelemetry, new()
        {
            Util.EnsureNotNull(name, nameof(name));

            DistributedTracingActivity currentOperationContext = DistributedTracingActivity.Current;

            TOperation operation = new TOperation();
            operation.Name = name;

            operation.Id = operationId;
            if (operation.Id == null)
            {
                operation.GenerateOperationId();
            }

            operation.Context.Operation.ParentId = parentOperationId ?? currentOperationContext?.Id;
            operation.Context.Operation.Id = globalOperationId ?? currentOperationContext?.RootId;
            operation.Context.Operation.Name = name;

            IOperationHolder<TOperation> startedOperation = _applicationInsightsClient.StartOperation(operation);
            return startedOperation;
        }
    }
}
