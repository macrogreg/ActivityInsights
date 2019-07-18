using System;

namespace Microsoft.ActivityInsights.Pipeline
{
    public static class ActivityPipelineDefaults
    {
        public static class ProcessorNames
        {
            public const string MetricExtractorForCountsAndDurations = "Extract Activity Counts and Durations";
            public const string FilterToExcludeDevelopmentLogs = "Exclude Development Logs";
        }

        public static class MetricNames
        {
            public const string ActivityCountsAndDurations = "Activity Durations and Counts";
            public const string ActivityCountsAndDurationsByName = "Activity Durations and Counts by Name";
        }

        public static IActivityPipeline CreateForDevelopmentPhase()
        {
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
                outputMetricName:
                    MetricNames.ActivityCountsAndDurations,
                measurementValueExtractor:
                    ActivityProcessor.UseMeasurement.ActivityDuration(),
                activityLabelsToMetricDimensionsMap:
                    ActivityProcessor.UseLabels.MapToDimensions(
                            ActivityProcessor.UseLabels.MagicNames.ActivityStatus,
                            ActivityProcessor.UseLabels.MagicNames.ActivityLogLevel)
            ));

            return pipeline;
        }

        public static IActivityPipeline CreateForMaturePhase()
        {
            ActivityPipeline pipeline = new ActivityPipeline();

            pipeline.Processors.Add(ActivityProcessor.CreateMetricExtractor(
                extractorName:
                    ProcessorNames.MetricExtractorForCountsAndDurations,
                activitySelector:
                    ActivityProcessor.UseSelectors.SelectAll(),
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

            return pipeline;
        }
    }
}
