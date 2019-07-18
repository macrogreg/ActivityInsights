using System;

namespace Microsoft.ActivityInsights.Pipeline
{
    internal class ActivitySelectorByName
    {
        private readonly string _value;
        private readonly bool _requiredMatchResult;

        public ActivitySelectorByName(string value, bool negateResult)
        {
            _value = Util.EnsureNotNull(value, nameof(value));
            _requiredMatchResult = negateResult ? false : true;
        }

        public bool StartsWith(Activity activity)
        {
            return _requiredMatchResult == activity?.Name?.StartsWith(_value, StringComparison.Ordinal);
        }

        public bool EndsWith(Activity activity)
        {
            return _requiredMatchResult == activity?.Name?.EndsWith(_value, StringComparison.Ordinal);
        }

        public bool Contains(Activity activity)
        {
            return _requiredMatchResult == activity?.Name?.Contains(_value);
        }

        public bool Equals(Activity activity)
        {
            return _requiredMatchResult == activity?.Name?.Equals(_value, StringComparison.Ordinal);
        }
    }
}
