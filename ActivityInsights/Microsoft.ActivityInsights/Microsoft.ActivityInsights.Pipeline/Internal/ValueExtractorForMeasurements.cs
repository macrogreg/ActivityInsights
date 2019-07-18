using System;

namespace Microsoft.ActivityInsights.Pipeline
{
    internal class ValueExtractorForMeasurements
    {
        private readonly string _measurementName;
        private readonly double? _defaultValue;

        public ValueExtractorForMeasurements(string measurementName, double? defaultValue)
        {
            _measurementName = Util.EnsureNotNull(measurementName, nameof(measurementName));
            _defaultValue = defaultValue;
        }

        public double? ExtractValue(Activity activity)
        {
            double value = default(double);
            if (true == activity?.TryGetMeasurement(_measurementName, out value))
            {
                return value;
            }
            else
            {
                return _defaultValue;
            }
        }
    }
}
