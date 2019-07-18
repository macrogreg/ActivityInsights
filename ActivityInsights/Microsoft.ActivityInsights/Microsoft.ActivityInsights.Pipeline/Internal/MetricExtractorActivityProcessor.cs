using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;

namespace Microsoft.ActivityInsights.Pipeline
{
    internal sealed class MetricExtractorActivityProcessor : IActivityProcessor
    {
        public const string MetricNamespace = "ActivityInsights";

        private readonly string _name;
        private readonly Func<Activity, bool> _activitySelector;
        private readonly MetricIdentifier _metricId;
        private readonly Func<Activity, double?> _measurementValueExtractor;
        private readonly Func<Activity, string>[] _dimensionLabelExtractors; 
        private MetricHandle _metricHandle;

        private class MetricHandle
        {
            public MetricHandle(Metric metric, TelemetryClient telemetryClient)
            {
                this.Metric = metric;
                this.TelemetryClient = telemetryClient;
            }

            public Metric Metric { get; }
            public TelemetryClient TelemetryClient { get; }
        }

        private static string NullLabelExtractor(Activity activity)
        {
            return Util.NullString;
        }

        internal MetricExtractorActivityProcessor(
                                            string processorName,
                                            Func<Activity, bool> activitySelector,
                                            string outputMetricName,
                                            Func<Activity, double?> measurementValueExtractor,
                                            IEnumerable<Tuple<Func<Activity, string>, string>> activityLabelsToMetricDimensionsMap)
        {
            _name = Util.SpellNull(processorName);
            _activitySelector = Util.EnsureNotNull(activitySelector, nameof(activitySelector));
            outputMetricName = Util.EnsureNotNullOrEmpty(outputMetricName, nameof(outputMetricName));
            _measurementValueExtractor = Util.EnsureNotNull(measurementValueExtractor, nameof(measurementValueExtractor));

            if (activityLabelsToMetricDimensionsMap == null)
            {
                _metricId = new MetricIdentifier(MetricNamespace, outputMetricName);
                _dimensionLabelExtractors = new Func<Activity, string>[0];
            }
            else
            {
                _metricId = new MetricIdentifier(MetricNamespace, outputMetricName, activityLabelsToMetricDimensionsMap.Select( (ln) => Util.SpellNull(ln.Item2) ).ToList());
                _dimensionLabelExtractors = activityLabelsToMetricDimensionsMap.Select( (ln) => (ln.Item1 ?? NullLabelExtractor) ).ToArray();
            }

            _metricHandle = null;
        }

        public string Name { get { return _name; } }

        public void ProcessActivity(Activity activity, TelemetryClient applicationInsightsClient, out bool continueProcessing)
        {
            Util.EnsureNotNull(activity, nameof(activity));
            Util.EnsureNotNull(applicationInsightsClient, nameof(applicationInsightsClient));
            continueProcessing = true;

            if (! _activitySelector(activity))
            {
                return;
            }

            double? metricValue = _measurementValueExtractor(activity);
            if (metricValue == null)
            {
                return;
            }

            string[] dimensionValues = new string[_dimensionLabelExtractors.Length];
            for (int i = 0; i < _dimensionLabelExtractors.Length; i++)
            {
                string labelValue = _dimensionLabelExtractors[i](activity);
                dimensionValues[i] = Util.SpellNull(labelValue);
            }

            Metric metric = GetMetric(applicationInsightsClient);

            MetricSeries dataSeries;
            bool canGetSeries = metric.TryGetDataSeries(out dataSeries, true, dimensionValues);

            if (! canGetSeries)
            {
                // Cannot get series. Probably reached dimension cap.

                // Consider attempting to use the dimensionless time series (like below) instead of the following error log:
                //canGetSeries = metric.TryGetDataSeries(out dataSeries);

                if (! canGetSeries)
                {
                    var detailLabels = new Dictionary<string, string>();
                    var detailMeasures = new Dictionary<string, double>();
                    ActivitySerializer.AddCoreActivityMetadata(activity, detailLabels, detailMeasures);

                    detailLabels["ActivityProcessor.Name"] = this.Name;
                    detailLabels["ActivityProcessor.Type"] = this.GetType().Name;
                    detailLabels["OutputMetric.Namespace"] = metric.Identifier.MetricNamespace;
                    detailLabels["OutputMetric.Name"] = metric.Identifier.MetricId;
                    detailLabels["OutputMetric.DimensionNames"] = Util.FormatAsArray(metric.Identifier.GetDimensionNames());
                    detailLabels["OutputMetric.LookupDimensionValues"] = Util.FormatAsArray(dimensionValues);

                    detailMeasures["OutputMetric.Value"] = metricValue.Value;

                    Util.LogInternalError(applicationInsightsClient, "Activity processor failed to get Get Data Series for output metric (see details).", detailLabels, detailMeasures);
                }
            }

            if (canGetSeries)
            {
                dataSeries.TrackValue(metricValue);
            }
        }

        private Metric GetMetric(TelemetryClient applicationInsightsClient)
        {
            MetricHandle handle = _metricHandle;

            if (handle != null && handle.TelemetryClient == applicationInsightsClient)
            {
                return handle.Metric;
            }

            Metric metric = applicationInsightsClient.GetMetric(_metricId, MetricConfigurations.Common.Measurement(), MetricAggregationScope.TelemetryClient);
            MetricHandle newHandle = new MetricHandle(metric, applicationInsightsClient);

            // We may or may not win a cincurrency here, but it does not matter - this cache is just an optimization.
            Interlocked.Exchange(ref _metricHandle, newHandle);

            return newHandle.Metric;
        }
    }

}
