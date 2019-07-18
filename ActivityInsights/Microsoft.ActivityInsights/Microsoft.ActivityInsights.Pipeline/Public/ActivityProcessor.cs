using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights.Pipeline
{
    public static class ActivityProcessor
    {
        public static IActivityProcessor CreateCustom(string name, ActivityProcessorAction action)
        {
            return new CustomActivityProcessor(name, action);
        }

        public static IActivityProcessor CreateMetricExtractor(
                                                    string extractorName,
                                                    Func<Activity, bool> activitySelector,
                                                    string outputMetricName,
                                                    Func<Activity, double?> measurementValueExtractor,
                                                    IEnumerable<Tuple<Func<Activity, string>, string>> activityLabelsToMetricDimensionsMap)
        {
            return new MetricExtractorActivityProcessor(extractorName, activitySelector, outputMetricName, measurementValueExtractor, activityLabelsToMetricDimensionsMap);
        }

        public static IActivityProcessor CreateFilterToExcludeAllSelected(
                                                    string filterName,
                                                    Func<Activity, bool> applyToActivitySelector)
        {
            return CreateFilterToExcludeAllSelected(filterName, applyToActivitySelector, exceptActivitySelector: null);
        }

        public static IActivityProcessor CreateFilterToExcludeAllSelected(
                                                    string filterName,
                                                    Func<Activity, bool> applyToActivitySelector,
                                                    Func<Activity, bool> exceptActivitySelector)
        {
            return new FilterToExcludeAllSelectedActivityProcessor(filterName, applyToActivitySelector, exceptActivitySelector);
        }

        public static IActivityProcessor CreateFilterToIncludeOnlySelected(
                                                    string filterName,
                                                    Func<Activity, bool> applyToActivitySelector)
        {
            return CreateFilterToIncludeOnlySelected(filterName, applyToActivitySelector, exceptActivitySelector: null);
        }

        public static IActivityProcessor CreateFilterToIncludeOnlySelected(
                                                    string filterName,
                                                    Func<Activity, bool> applyToActivitySelector,
                                                    Func<Activity, bool> exceptActivitySelector)
        {
            return new FilterToIncludeOnlySelectedActivityProcessor(filterName, applyToActivitySelector, exceptActivitySelector);
        }

        public static class UseMeasurement
        {
            public static Func<Activity, double?> ActivityDuration()
            {
                return ( (Activity activity) => activity?.Duration.TotalMilliseconds );
            }

            public static Func<Activity, double?> Custom(string measurementName, double? defaultValue)
            {
                return (new ValueExtractorForMeasurements(measurementName, defaultValue)).ExtractValue;
            }
        }

        public static class UseLabels
        {
            public static class MagicNames
            {
                public const string ActivityName = "Activity.Name";
                public const string ActivityStatus = "Activity.Status";
                public const string ActivityLogLevel = "Activity.LogLevel";
            }

            public static Tuple<Func<Activity, string>, string> LabelExtractorToDimensionMapEntry(Func<Activity, string> labelExtractor, string dimensionName)
            {
                return Tuple.Create(labelExtractor, dimensionName);
            }

            public static IEnumerable<Tuple<Func<Activity, string>, string>> MapToDimensions(params string[] dimensionLabelNames)
            {
                if (dimensionLabelNames == null)
                {
                    return null;
                }

                var map = new List<Tuple<Func<Activity, string>, string>>();
                for (int i = 0; i < dimensionLabelNames.Length; i++)
                {
                    if (dimensionLabelNames[i] == null)
                    {
                        continue;
                    }

                    Func<Activity, string> labelValueExtractor;

                    if (MagicNames.ActivityName.Equals(dimensionLabelNames[i], StringComparison.Ordinal))
                    {
                        labelValueExtractor = (a) => a.Name;
                    }
                    else if(MagicNames.ActivityStatus.Equals(dimensionLabelNames[i], StringComparison.Ordinal))
                    {
                        labelValueExtractor = (a) => a.Status.ToString();
                    }
                    else if (MagicNames.ActivityLogLevel.Equals(dimensionLabelNames[i], StringComparison.Ordinal))
                    {
                        labelValueExtractor = (a) => a.LogLevel.ToCategoryString();
                    }
                    else
                    {
                        labelValueExtractor = (new ValueExtractorForLabels(dimensionLabelNames[i], Util.NullString)).ExtractValue;
                    }

                    map.Add(LabelExtractorToDimensionMapEntry(labelValueExtractor, dimensionLabelNames[i]));
                }

                return map;
            }
        }

        public static class UseSelectors
        {
            public static class Combine
            {
                public static Func<Activity, bool> And(params Func<Activity, bool>[] selectorsToCombine)
                {
                    return (new ActivitySelectorAggregator(selectorsToCombine)).And;
                }

                public static Func<Activity, bool> Or(params Func<Activity, bool>[] selectorsToCombine)
                {
                    return (new ActivitySelectorAggregator(selectorsToCombine)).Or;
                }
            }

            public static class ByName
            {
                public static Func<Activity, bool> StartsWith(string searchValue)
                {
                    return (new ActivitySelectorByName(searchValue, negateResult: false)).StartsWith;
                }

                public static Func<Activity, bool> StartsNotWith(string searchValue)
                {
                    return (new ActivitySelectorByName(searchValue, negateResult: true)).StartsWith;
                }

                public static Func<Activity, bool> EndsWith(string searchValue)
                {
                    return (new ActivitySelectorByName(searchValue, negateResult: false)).EndsWith;
                }

                public static Func<Activity, bool> EndsNotWith(string searchValue)
                {
                    return (new ActivitySelectorByName(searchValue, negateResult: true)).EndsWith;
                }

                public static Func<Activity, bool> Contains(string searchValue)
                {
                    return (new ActivitySelectorByName(searchValue, negateResult: false)).Contains;
                }

                public static Func<Activity, bool> NotContains(string searchValue)
                {
                    return (new ActivitySelectorByName(searchValue, negateResult: true)).Contains;
                }

                public static Func<Activity, bool> Equal(string searchValue)
                {
                    return (new ActivitySelectorByName(searchValue, negateResult: false)).Equals;
                }

                public static Func<Activity, bool> NotEqual(string searchValue)
                {
                    return (new ActivitySelectorByName(searchValue, negateResult: true)).Equals;
                }
            }

            public static class ByLogLevel
            {
                public static Func<Activity, bool> Smaller(ActivityLogLevel logLevel)
                {
                    return (new ActivitySelectorByLogLevel(logLevel)).IsSmaller;
                }

                public static Func<Activity, bool> SmallerOrEqual(ActivityLogLevel logLevel)
                {
                    return (new ActivitySelectorByLogLevel(logLevel)).IsSmallerOrEqual;
                }

                public static Func<Activity, bool> Equal(ActivityLogLevel logLevel)
                {
                    return (new ActivitySelectorByLogLevel(logLevel)).IsEqual;
                }

                public static Func<Activity, bool> LargerOrEqual(ActivityLogLevel logLevel)
                {
                    return (new ActivitySelectorByLogLevel(logLevel)).IsLargerOrEqual;
                }

                public static Func<Activity, bool> Larger(ActivityLogLevel logLevel)
                {
                    return (new ActivitySelectorByLogLevel(logLevel)).IsLarger;
                }
            }

            public static class ByStatus
            {
                public static Func<Activity, bool> Equal(ActivityStatus activityStatus)
                {
                    return (new ActivitySelectorByActivityStatus(activityStatus)).IsEqual;
                }

                public static Func<Activity, bool> NotEqual(ActivityStatus activityStatus)
                {
                    return (new ActivitySelectorByActivityStatus(activityStatus)).IsNotEqual;
                }
            }

            private static Func<Activity, bool> SelectAllFunc = (a) => true;
            private static Func<Activity, bool> SelectNoneFunc = (a) => false;

            public static Func<Activity, bool> SelectAll()
            {
                return SelectAllFunc;
            }

            public static Func<Activity, bool> SelectNone()
            {
                return SelectNoneFunc;
            }

        }  // public static class Selectors
    }  // public static class ActivityProcessor
}
