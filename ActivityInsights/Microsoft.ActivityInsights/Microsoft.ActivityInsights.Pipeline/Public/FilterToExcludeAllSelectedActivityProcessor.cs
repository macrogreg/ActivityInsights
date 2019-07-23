using System;
using Microsoft.ApplicationInsights;

namespace Microsoft.ActivityInsights.Pipeline
{
    public sealed class FilterToExcludeAllSelectedActivityProcessor : IActivityProcessor
    {
        private Func<Activity, bool> _applyToActivitySelector;
        private Func<Activity, bool> _exceptActivitySelector;

        internal FilterToExcludeAllSelectedActivityProcessor(
                                            string filterName,
                                            Func<Activity, bool> applyToActivitySelector,
                                            Func<Activity, bool> exceptActivitySelector)
        {
            this.Name = Util.SpellNull(filterName);
            _applyToActivitySelector = Util.EnsureNotNull(applyToActivitySelector, nameof(applyToActivitySelector));
            _exceptActivitySelector = Util.EnsureNotNull(exceptActivitySelector, nameof(exceptActivitySelector));
        }

        public string Name { get; }

        public Func<Activity, bool> ApplyToActivitySelector
        {
            get { return _applyToActivitySelector; }
            set { _applyToActivitySelector = Util.EnsureNotNull(value, nameof(value)); }
        }

        public Func<Activity, bool> ExceptActivitySelector
        {
            get { return _exceptActivitySelector; }
            set { _exceptActivitySelector = Util.EnsureNotNull(value, nameof(value)); }
        }

        public void ProcessActivity(Activity activity, out bool continueProcessing)
        {
            Func<Activity, bool> exceptActivitySelector = _exceptActivitySelector;
            bool selected = (true == _applyToActivitySelector(activity)) && (exceptActivitySelector == null || false == exceptActivitySelector(activity));

            continueProcessing = (false == selected);
        }
    }

}
