using System;
using Microsoft.ApplicationInsights;

namespace Microsoft.ActivityInsights.Pipeline
{
    public static class ActivityPipelineDefaults
    {
        public static class ProcessorNames
        {
            public const string MetricExtractorForCountsAndDurations = "Extract Activity Counts and Durations";
            public const string FilterToExcludeDevelopmentLogs = "Exclude Development Logs";
        }

        public static class SenderNames
        {
            public const string DefaultApplicationInsightsSender = "Default Application Insights Activity Sender";
        }

        public static class MetricNames
        {
            public const string ActivityCountsAndDurations = "Activity Durations and Counts";
            public const string ActivityCountsAndDurationsByName = "Activity Durations and Counts by Name";
        }

        public static IActivityPipeline CreateForDevelopmentPhase()
        {
            TelemetryClient applicationInsightsClient = new TelemetryClient();
            return CreateForDevelopmentPhase(applicationInsightsClient);
        }

        public static IActivityPipeline CreateForDevelopmentPhase(TelemetryClient applicationInsightsClient)
        {
            Util.EnsureNotNull(applicationInsightsClient, nameof(applicationInsightsClient));

            ActivityPipeline pipeline = new ActivityPipeline();

            // In development/debug phase we log all activities.
            // We also extract metrics from all activities.
            // The metric includes the activity duration as Value and the number of activities as Count.
            // The metric dimensions are the LogLevel category and the Status of the activities.

            pipeline.Processors.Add(ActivityProcessor.CreateMetricExtractor(
                extractorName:
                    ProcessorNames.MetricExtractorForCountsAndDurations,
                activitySelector:
                    ActivityProcessor.UseSelectors.SelectAll(),
                applicationInsightsClient:
                    applicationInsightsClient,
                outputMetricName:
                    MetricNames.ActivityCountsAndDurations,
                measurementValueExtractor:
                    ActivityProcessor.UseMeasurement.ActivityDuration(),
                activityLabelsToMetricDimensionsMap:
                    ActivityProcessor.UseLabels.MapToDimensions(
                            ActivityProcessor.UseLabels.MagicNames.ActivityStatus,
                            ActivityProcessor.UseLabels.MagicNames.ActivityLogLevel)
            ));

            pipeline.Senders.Add(new ApplicationInsightsActivitySender(SenderNames.DefaultApplicationInsightsSender, applicationInsightsClient));

            return pipeline;
        }

        public static IActivityPipeline CreateForMaturePhase()
        {
            TelemetryClient applicationInsightsClient = new TelemetryClient();
            return CreateForMaturePhase(applicationInsightsClient);
        }

        public static IActivityPipeline CreateForMaturePhase(TelemetryClient applicationInsightsClient)
        {
            Util.EnsureNotNull(applicationInsightsClient, nameof(applicationInsightsClient));

            ActivityPipeline pipeline = new ActivityPipeline();

            pipeline.Processors.Add(ActivityProcessor.CreateMetricExtractor(
                extractorName:
                    ProcessorNames.MetricExtractorForCountsAndDurations,
                activitySelector:
                    ActivityProcessor.UseSelectors.SelectAll(),
                applicationInsightsClient:
                    applicationInsightsClient,
                outputMetricName:
                    MetricNames.ActivityCountsAndDurations,
                measurementValueExtractor:
                    ActivityProcessor.UseMeasurement.ActivityDuration(),
                activityLabelsToMetricDimensionsMap:
                    ActivityProcessor.UseLabels.MapToDimensions(
                            ActivityProcessor.UseLabels.MagicNames.ActivityStatus,
                            ActivityProcessor.UseLabels.MagicNames.ActivityLogLevel)
            ));

            pipeline.Processors.Add(ActivityProcessor.CreateMetricExtractor(
                extractorName:
                    ProcessorNames.MetricExtractorForCountsAndDurations,
                activitySelector:
                    ActivityProcessor.UseSelectors.SelectAll(),
                applicationInsightsClient:
                    applicationInsightsClient,
                outputMetricName:
                    MetricNames.ActivityCountsAndDurationsByName,
                measurementValueExtractor:
                    ActivityProcessor.UseMeasurement.ActivityDuration(),
                activityLabelsToMetricDimensionsMap:
                    new[] {
                            ActivityProcessor.UseLabels.LabelExtractorToDimensionMapEntry(
                                    (a) => a.LogLevel.ToCategoryString(),
                                    ActivityProcessor.UseLabels.MagicNames.ActivityLogLevel),
                            ActivityProcessor.UseLabels.LabelExtractorToDimensionMapEntry(
                                    (a) => a.Status.ToString(),
                                    ActivityProcessor.UseLabels.MagicNames.ActivityStatus),
                            ActivityProcessor.UseLabels.LabelExtractorToDimensionMapEntry(
                                    (new ValueExtractorForLabelsWithCardinalityLimit( (a) => a.Name , maxValueCount: 100)).ExtractValue,
                                    ActivityProcessor.UseLabels.MagicNames.ActivityName)
                    }
            ));


            pipeline.Processors.Add(ActivityProcessor.CreateFilterToExcludeAllSelected(
                filterName:
                    ProcessorNames.FilterToExcludeDevelopmentLogs,
                applyToActivitySelector:
                    ActivityProcessor.UseSelectors.Combine.And(
                            ActivityProcessor.UseSelectors.ByLogLevel.SmallerOrEqual(ActivityLogLevel.Information),
                            ActivityProcessor.UseSelectors.ByStatus.NotEqual(ActivityStatus.Faulted))
            ));

            pipeline.Senders.Add(new ApplicationInsightsActivitySender(SenderNames.DefaultApplicationInsightsSender, applicationInsightsClient));

            return pipeline;
        }

        public static IActivityPipeline CreateForSendOnly()
        {
            TelemetryClient applicationInsightsClient = new TelemetryClient();
            return CreateForSendOnly(applicationInsightsClient);
        }

        public static IActivityPipeline CreateForSendOnly(TelemetryClient applicationInsightsClient)
        {
            Util.EnsureNotNull(applicationInsightsClient, nameof(applicationInsightsClient));

            ActivityPipeline pipeline = new ActivityPipeline();
            pipeline.Senders.Add(new ApplicationInsightsActivitySender(SenderNames.DefaultApplicationInsightsSender, applicationInsightsClient));
            return pipeline;
        }
    }

}
