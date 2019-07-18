using System;

namespace Microsoft.ActivityInsights.Pipeline
{
    internal class ActivitySelectorAggregator
    {
        private readonly Func<Activity, bool>[] _matchersToCombine;

        public ActivitySelectorAggregator(Func<Activity, bool>[] matchersToCombine)
        {
            _matchersToCombine = matchersToCombine;
        }

        public bool And(Activity activity)
        {
            if (activity == null || _matchersToCombine == null || _matchersToCombine.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < _matchersToCombine.Length; i++)
            {
                if (false == _matchersToCombine[i](activity))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Or(Activity activity)
        {
            if (activity == null || _matchersToCombine == null || _matchersToCombine.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < _matchersToCombine.Length; i++)
            {
                if (true == _matchersToCombine[i](activity))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
