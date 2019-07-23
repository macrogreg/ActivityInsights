using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
        private readonly TelemetryClient _applicationInsightsClient;
        private readonly Metric _outputMetric;

        private static string NullLabelExtractor(Activity activity)
        {
            return Util.NullString;
        }

        internal MetricExtractorActivityProcessor(
                                            string processorName,
                                            Func<Activity, bool> activitySelector,
                                            TelemetryClient applicationInsightsClient,
                                            string outputMetricName,
                                            Func<Activity, double?> measurementValueExtractor,
                                            IEnumerable<Tuple<Func<Activity, string>, string>> activityLabelsToMetricDimensionsMap)
        {
            _name = Util.SpellNull(processorName);
            _activitySelector = Util.EnsureNotNull(activitySelector, nameof(activitySelector));
            _applicationInsightsClient = Util.EnsureNotNull(applicationInsightsClient, nameof(applicationInsightsClient));
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

            _outputMetric = applicationInsightsClient.GetMetric(_metricId, MetricConfigurations.Common.Measurement(), MetricAggregationScope.TelemetryClient);
        }

        public string Name { get { return _name; } }

        public void ProcessActivity(Activity activity, out bool continueProcessing)
        {
            Util.EnsureNotNull(activity, nameof(activity));
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

            MetricSeries dataSeries;
            bool canGetSeries = _outputMetric.TryGetDataSeries(out dataSeries, true, dimensionValues);

            if (! canGetSeries)
            {
                // Cannot get series. Probably reached dimension cap.

                // Consider attempting to use the dimensionless time series (like below) instead of the following error log:
                // canGetSeries = metric.TryGetDataSeries(out dataSeries);
                // if (! canGetSeries)
                // {
                //     create detailLabels and detailMeasures and throw ActivityInsightsException exception like below
                // }

                var detailLabels = new Dictionary<string, string>();
                var detailMeasures = new Dictionary<string, double>();
                ActivitySerializer.AddActivityCoreMetadata(activity, detailLabels, detailMeasures);

                detailLabels["ActivityProcessor.Name"] = this.Name;
                detailLabels["ActivityProcessor.Type"] = this.GetType().Name;
                detailLabels["OutputMetric.Namespace"] = _outputMetric.Identifier.MetricNamespace;
                detailLabels["OutputMetric.Name"] = _outputMetric.Identifier.MetricId;
                detailLabels["OutputMetric.DimensionNames"] = Util.FormatAsArray(_outputMetric.Identifier.GetDimensionNames());
                detailLabels["OutputMetric.LookupDimensionValues"] = Util.FormatAsArray(dimensionValues);

                detailMeasures["OutputMetric.Value"] = metricValue.Value;

                throw new ActivityInsightsException("Activity processor failed to get Get Data Series for output metric (see details).", detailLabels, detailMeasures);
            }

            dataSeries.TrackValue(metricValue);
        }
    }

}
