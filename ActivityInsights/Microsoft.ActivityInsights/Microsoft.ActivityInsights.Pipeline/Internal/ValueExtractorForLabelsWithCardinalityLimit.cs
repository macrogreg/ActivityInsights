using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights.Pipeline
{
    internal class ValueExtractorForLabelsWithCardinalityLimit
    {
        public const string OtherValueMonikerDefault = "Other";

        private readonly Func<Activity, string> _labelValueExtractor;
        private readonly int _maxValueCount;
        private readonly string _otherValueMoniker;
        private readonly HashSet<string> _previousValues;

        public ValueExtractorForLabelsWithCardinalityLimit(Func<Activity, string> labelValueExtractor, int maxValueCount)
            : this(labelValueExtractor, maxValueCount, otherValueMoniker: OtherValueMonikerDefault)
        {
        }

        public ValueExtractorForLabelsWithCardinalityLimit(Func<Activity, string> labelValueExtractor, int maxValueCount, string otherValueMoniker)
        {
            _labelValueExtractor = Util.EnsureNotNull(labelValueExtractor, nameof(labelValueExtractor));

            if (maxValueCount < 2)
            {
                throw new ArgumentException($"{nameof(maxValueCount)} must be 2 or larger.");
            }

            _maxValueCount = maxValueCount;
            _otherValueMoniker = String.IsNullOrWhiteSpace(otherValueMoniker) ? OtherValueMonikerDefault : otherValueMoniker;

            _previousValues = new HashSet<string>();
            _previousValues.Add(_otherValueMoniker);
        }

        public string ExtractValue(Activity activity)
        {
            string value = _labelValueExtractor(activity);
            value = Util.SpellNull(value);

            if (this.CanUseValue(value))
            {
                return value;
            }
            else
            {
                return _otherValueMoniker;
            }
        }

        private bool CanUseValue(string value)
        {
            if (_previousValues.Contains(value))
            {
                return true;
            }

            if (_previousValues.Count >= _maxValueCount)
            {
                return false;
            }

            lock (_previousValues)
            {
                if (_previousValues.Count >= _maxValueCount)
                {
                    return false;
                }

                _previousValues.Add(value);
                return true;
            }
        }
    }
}
