using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ActivityInsights;
using Microsoft.ActivityInsights.Pipeline;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;

namespace Microsoft.ActivityInsights.Examples.ConsoleFoo
{

    class TestPipeline : IActivityPipeline
    {
        public IList<IActivityProcessor> Processors { get; }  = new List<IActivityProcessor>();

        public IList<IActivitySender> Senders { get; } = new List<IActivitySender>();

        public void ProcessAndSend(Activity activity)
        {
            Console.WriteLine($"Sending activity '{activity.Name}' ({activity.ActivityId}). ParentName='{activity.ParentActivity?.Name ?? "null"}'. RootName='{activity.RootActivity?.Name ?? "null"}'");
        }
    }

    class Program
    {
        class TestActivity
        {
            public TestActivity Parent = null;
            public string Name = null;
            public TestActivity(TestActivity parent, string name)
            {
                this.Parent = parent;
                this.Name = name;
            }
            public override string ToString()
            {
                if (Parent == null)
                {
                    return $"TestActivity = (Parent=null, Name={Name??"null"})";
                }
                else
                {
                    return $"TestActivity = (Parent={Parent.Name??"null"}, Name={Name??"null"})";
                }
                
            }
        }

        static AsyncLocal<TestActivity> _al1 = new AsyncLocal<TestActivity>();

        static void PrintAsyncLocal(AsyncLocal<TestActivity> al)
        {
            Console.WriteLine(al?.Value?.ToString() ?? "null");
        }

        //public static async Task M1Async()
        //{
        //    TestActivity.Start("M1");
        //    await Task.Delay(1000);
        //    TestActivity.Finish();
        //}

        //public static async Task M2Async()
        //{
        //    TestActivity.Start("M2");
        //    await Task.Delay(1000);
        //    TestActivity.Finish();
        //}

        //public static Task M3()
        //{
        //    TestActivity.Start("M3");
        //    Task.Delay(1000).GetAwaiter().GetResult();
        //    TestActivity.Finish();
        //}

        //public static async Task M4()
        //{
        //    TestActivity.Start("M4");
        //    Task.Delay(1000).GetAwaiter().GetResult();
        //    TestActivity.Finish();
        //}

        static ActivityInsightsLogger _testLogger = new ActivityInsightsLogger(new TestPipeline());

        static Task<int> DoWorkSync(int x)
        {
            _testLogger.StartNewActivity($"Activity {x}", ActivityLogLevel.Information);
            Console.WriteLine($"Performing workload {x}");
            _testLogger.CompleteCurrentActivity();

            return Task.FromResult(x);
        }

        static async Task<string> DoWorkAsync(string s)
        {
            _testLogger.StartNewLogicalActivityThread($"Activity {s}", ActivityLogLevel.Information);

            Console.WriteLine($"About to sleep #1 in {s}");
            await Task.Delay(1000);
            Console.WriteLine($"Completed sleep #1 in {s}");

            _testLogger.StartNewActivity($"Activity {s}.A", ActivityLogLevel.Information);

            Console.WriteLine($"About to sleep #2 in {s}");
            await Task.Delay(1000);
            Console.WriteLine($"Completed sleep #2 in {s}");

            _testLogger.CompleteCurrentActivity();

            _testLogger.CompleteCurrentActivity();

            return s;
        }

        static void TestLogger1()
        {
            _testLogger.StartNewActivity("Activity 1", ActivityLogLevel.Information);

            Console.WriteLine("Performing workload 1");

            {
                _testLogger.StartNewActivity("Activity 2", ActivityLogLevel.Information);
                Console.WriteLine("Performing workload 2");

                DoWorkSync(3);

                DoWorkSync(4).GetAwaiter().GetResult();

                Task.Run(() => DoWorkSync(5)).GetAwaiter().GetResult();

                _testLogger.CompleteCurrentActivity();
            }

            _testLogger.CompleteCurrentActivity();

            Console.WriteLine();
            Console.WriteLine();

            _testLogger.StartNewActivity("Activity 10", ActivityLogLevel.Information);

            Task tA = DoWorkAsync("A");
            Task tB = DoWorkAsync("B");

            DoWorkSync(11);

            Task.WaitAll(new[] {tA, tB});

            _testLogger.CompleteCurrentActivity();
        }

        static void Thread1Main()
        {

            Console.WriteLine($"Thread1Main entered");
            PrintAsyncLocal(_al1);

            Thread.Sleep(500);

            Console.WriteLine($"Thread1Main slept #1");
            PrintAsyncLocal(_al1);

            Console.WriteLine($"Setting _al1 in Thread1Main #1");
            _al1.Value = new TestActivity(_al1.Value, "Th1");
            PrintAsyncLocal(_al1);
        }

        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello World!");

            TestLogger1();

            return;

            _al1.Value = new TestActivity(null, "0");
            PrintAsyncLocal(_al1);

            Thread thread1 = new Thread(Thread1Main);
            thread1.Start();

            Console.WriteLine($"thread1 started");
            PrintAsyncLocal(_al1);

            Thread.Sleep(1000);

            Console.WriteLine($"slept 1000ms");
            PrintAsyncLocal(_al1);

            //Task t1 = M1Async();
            //Task t2 = M2Async();

            return;

            _al1.Value = new TestActivity(null, "0");

            PrintAsyncLocal(_al1);

            Task t1 = Work1Async();

            Console.WriteLine($"Returned from {nameof(Work1Async)}");
            PrintAsyncLocal(_al1);

            Console.WriteLine($"");

            Thread.Sleep(1500);
            Console.WriteLine($"Done with Thread.Sleep #1 in main");
            PrintAsyncLocal(_al1);

            Console.WriteLine($"Updating _al1 in main");
            _al1.Value.Name = "0.2";
            PrintAsyncLocal(_al1);

            Thread.Sleep(1500);
            Console.WriteLine($"Done with Thread.Sleep #2 in main");
            PrintAsyncLocal(_al1);

            Console.WriteLine($"Updating _al1 in main");
            _al1.Value.Name = "0.3";
            PrintAsyncLocal(_al1);

            Console.WriteLine($"");

            t1.GetAwaiter().GetResult();
            Console.WriteLine($"Done waiting for t1.");

            PrintAsyncLocal(_al1);

            Console.WriteLine($"");

            Task t2 = Work2Async();
            Console.WriteLine($"Returned from {nameof(Work2Async)}");
            PrintAsyncLocal(_al1);

            Console.WriteLine($"");

            Task t3 = Work3Async();
            Console.WriteLine($"Returned from {nameof(Work3Async)}");
            PrintAsyncLocal(_al1);

            Console.WriteLine($"");

            Task.WaitAll(new[] { t2, t3 });
            Console.WriteLine($"Done waiting for t2 and t3");
            PrintAsyncLocal(_al1);

            Console.WriteLine($"");

            Task<int> t4 = Task.Run(Work4);

            Console.WriteLine($"Returned from Task.Run(Work4);");
            PrintAsyncLocal(_al1);

            _al1.Value.Name = "0.4";

            Console.WriteLine($"Modified local activity");
            PrintAsyncLocal(_al1);

            Console.WriteLine(t4.Result);

            Console.WriteLine($"Done waiting for t4");
            PrintAsyncLocal(_al1);
        }

        static async Task Work1Async()
        {
            Console.WriteLine($"Entered {nameof(Work1Async)}.");
            PrintAsyncLocal(_al1);

            await Task.Delay(1000);
            Console.WriteLine($"Completed await Task.Delay #1 in {nameof(Work1Async)}.");
            PrintAsyncLocal(_al1);

            Console.WriteLine("Updating _al1.Value.Name");
            _al1.Value.Name = "0.1";
            PrintAsyncLocal(_al1);

            await Task.Delay(1000);
            Console.WriteLine($"Completed await Task.Delay #2 in {nameof(Work1Async)}.");
            PrintAsyncLocal(_al1);

            Console.WriteLine("Updating _al1");
            _al1.Value = new TestActivity(_al1.Value, "1");

            PrintAsyncLocal(_al1);

            await Task.Delay(1000);
            Console.WriteLine($"Completed await Task.Delay #3 in {nameof(Work1Async)}.");

            PrintAsyncLocal(_al1);

            Console.WriteLine("Updating _al1");
            _al1.Value = new TestActivity(_al1.Value, "1.1");

            PrintAsyncLocal(_al1);

            await Task.Delay(1000);
            Console.WriteLine($"Completed await Task.Delay #4 in {nameof(Work1Async)}.");

            PrintAsyncLocal(_al1);
        }

        static async Task Work2Async()
        {
            Console.WriteLine($"Entered {nameof(Work2Async)}.");
            PrintAsyncLocal(_al1);

            Console.WriteLine("Updating _al1");
            _al1.Value = new TestActivity(_al1.Value, "2");

            PrintAsyncLocal(_al1);

            Task notAwaited = Task.Delay(1000);
            Console.WriteLine($"Did not wait for Task.Delay in {nameof(Work2Async)}.");

            PrintAsyncLocal(_al1);
        }

        static async Task Work3Async()
        {
            Console.WriteLine($"Entered {nameof(Work3Async)}.");
            PrintAsyncLocal(_al1);

            Console.WriteLine("Updating _al1");
            _al1.Value = new TestActivity(_al1.Value, "3");

            PrintAsyncLocal(_al1);

            await Task.Delay(1000);
            Console.WriteLine($"Completed await Task.Delay in {nameof(Work3Async)}.");

            PrintAsyncLocal(_al1);
        }

        static int Work4()
        {
            Console.WriteLine($"Entered {nameof(Work4)}.");
            PrintAsyncLocal(_al1);

            Console.WriteLine("Updating _al1");
            _al1.Value = new TestActivity(_al1.Value, "4");

            PrintAsyncLocal(_al1);

            Task.Delay(1000).GetAwaiter().GetResult();
            Console.WriteLine($"Completed wait for Task.Delay in {nameof(Work4)}.");

            PrintAsyncLocal(_al1);

            return 42;
        }


        public void ConfigurePipelineExample1()
        {
            IActivityPipeline pipeline = ActivityInsights.GetCurrentLogger().Pipeline;

            TelemetryClient appInsightClient =  pipeline.Senders.FindByName<ApplicationInsightsActivitySender>(ActivityPipelineDefaults.SenderNames.DefaultApplicationInsightsSender)?.ApplicationInsightsClient;
            appInsightClient = appInsightClient ?? new TelemetryClient();

            pipeline.Processors.Add(ActivityProcessor.CreateMetricExtractor(
                                                            "Sample Extractor",
                                                            ActivityProcessor.UseSelectors.ByName.StartsWith("Sale"),
                                                            appInsightClient,
                                                            "Price",
                                                            ActivityProcessor.UseMeasurement.Custom("Cows Sold", null),
                                                            ActivityProcessor.UseLabels.MapToDimensions(
                                                                    ActivityProcessor.UseLabels.MagicNames.ActivityName,
                                                                    ActivityProcessor.UseLabels.MagicNames.ActivityStatus,
                                                                    "Cow Gender",
                                                                    "Cow Breed",
                                                                    "Cow Color")));

            pipeline.Processors.Add(ActivityProcessor.CreateCustom("Sampling Processor", (Activity activity, out bool continueProcessing) =>
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
            ActivityInsights.GetCurrentLogger().Pipeline = pipeline; 
        }

        public void ConfigurePipelineExample3()
        {
            IActivityPipeline pipeline = ActivityPipelineDefaults.CreateForMaturePhase();

            pipeline
                    .Processors
                    .FindByName<FilterToExcludeAllSelectedActivityProcessor>(ActivityPipelineDefaults.ProcessorNames.FilterToExcludeDevelopmentLogs)
                            .ExceptActivitySelector = ActivityProcessor.UseSelectors.ByName.StartsWith("ComputePi");

            ActivityInsights.GetCurrentLogger().Pipeline = pipeline;
        }
    }


}
