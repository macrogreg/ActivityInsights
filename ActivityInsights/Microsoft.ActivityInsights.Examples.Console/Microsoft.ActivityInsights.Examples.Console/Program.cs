using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.ActivityInsights;
using Microsoft.ActivityInsights.Pipeline;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;

namespace Microsoft.ActivityInsights.Examples.Console
{


    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello World!");
        }

        public void ConfigurePipelineExample1()
        {
            IActivityPipeline pipeline = ActivityInsights.GetCurrentLogger().GetPipeline();

            pipeline.Processors.Add(ActivityProcessor.CreateMetricExtractor(
                                                            "Sample Extractor",
                                                            ActivityProcessor.UseSelectors.ByName.StartsWith("Sale"),
                                                            "Price",
                                                            ActivityProcessor.UseMeasurement.Custom("Cows Sold", null),
                                                            ActivityProcessor.UseLabels.MapToDimensions(
                                                                    ActivityProcessor.UseLabels.MagicNames.ActivityName,
                                                                    ActivityProcessor.UseLabels.MagicNames.ActivityStatus,
                                                                    "Cow Gender",
                                                                    "Cow Breed",
                                                                    "Cow Color")));

            pipeline.Processors.Add(ActivityProcessor.CreateCustom("Sampling Processor", (Activity activity, TelemetryClient telemetryClient, out bool continueProcessing) =>
                {
                    const double samplingRate = 0.2; // 20%
                    const int pivot = (int)(Int32.MaxValue * samplingRate);

                    int hash = activity.RootActivity.ActivityId.GetHashCode();
                    continueProcessing = (hash <= pivot);
                }));

            pipeline.Processors.Add(ActivityProcessor.CreateFilterToExcludeAllSelected(
                                                            "Sample Filter",
                                                            ActivityProcessor.UseSelectors.Combine.And(
                                                                ActivityProcessor.UseSelectors.ByName.StartsWith("FooBar"),
                                                                ActivityProcessor.UseSelectors.ByLogLevel.SmallerOrEqual(ActivityLogLevel.Debug),
                                                                ActivityProcessor.UseSelectors.ByStatus.NotEqual(ActivityStatus.Faulted)
                                                            )));
        }

        public void ConfigurePipelineExample2()
        {
            IActivityPipeline pipeline = ActivityPipelineDefaults.CreateForDevelopmentPhase();
            ActivityInsights.GetCurrentLogger().SetPipeline(pipeline); 
        }

        public void ConfigurePipelineExample3()
        {
            IActivityPipeline pipeline = ActivityPipelineDefaults.CreateForMaturePhase();

            pipeline
                    .Processors
                    .FindByName<FilterToExcludeAllSelectedActivityProcessor>(ActivityPipelineDefaults.ProcessorNames.FilterToExcludeDevelopmentLogs)
                            .ExceptActivitySelector = ActivityProcessor.UseSelectors.ByName.StartsWith("ComputePi");

            ActivityInsights.GetCurrentLogger().SetPipeline(pipeline);
        }
    }


}
