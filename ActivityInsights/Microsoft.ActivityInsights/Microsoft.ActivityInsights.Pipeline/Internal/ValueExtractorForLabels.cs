using System;

namespace Microsoft.ActivityInsights.Pipeline
{
    internal class ValueExtractorForLabels
    {
        private readonly string _labelName;
        private readonly string _defaultValue;

        public ValueExtractorForLabels(string labelName, string defaultValue)
        {
            _labelName = Util.EnsureNotNull(labelName, nameof(labelName));
            _defaultValue = defaultValue;
        }

        public string ExtractValue(Activity activity)
        {
            string value = null;
            if (true == activity?.TryGetLabel(_labelName, out value))
            {
                return value ?? _defaultValue;
            }
            else
            {
                return _defaultValue;
            }
        }
    }
}
