using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights.Pipeline
{
    public static class ActivityProcessorListExtensions
    {
        public static IActivityProcessor FindByName(this IList<IActivityProcessor> processors, string processorName)
        {
            Util.EnsureNotNull(processors, nameof(processors));
            processorName = Util.SpellNull(processorName);

            for (int i = 0; i < processors.Count; i++)
            {
                if (processors[i] != null && processorName.Equals(processors[i].Name, StringComparison.Ordinal))
                {
                    return processors[i];
                }
            }

            return null;
        }

        public static T FindByName<T>(this IList<IActivityProcessor> processors, string processorName) where T : class, IActivityProcessor
        {
            Util.EnsureNotNull(processors, nameof(processors));
            processorName = Util.SpellNull(processorName);

            for (int i = 0; i < processors.Count; i++)
            {
                if (processors[i] != null)
                {
                    T typedProcessor = processors[i] as T;
                    if (typedProcessor != null && processorName.Equals(typedProcessor.Name, StringComparison.Ordinal))
                    {
                        return typedProcessor;
                    }
                }
            }

            return null;
        }

        public static ICollection<IActivityProcessor> FindAllByName(this IList<IActivityProcessor> processors, string processorName)
        {
            Util.EnsureNotNull(processors, nameof(processors));
            processorName = Util.SpellNull(processorName);

            var found = new List<IActivityProcessor>();
            for (int i = 0; i < processors.Count; i++)
            {
                if (processors[i] != null && processorName.Equals(processors[i].Name, StringComparison.Ordinal))
                {
                    found.Add(processors[i]);
                }
            }

            return found;
        }

        public static ICollection<T> FindAllByName<T>(this IList<IActivityProcessor> processors, string processorName) where T : class, IActivityProcessor
        {
            Util.EnsureNotNull(processors, nameof(processors));
            processorName = Util.SpellNull(processorName);

            var found = new List<T>();
            for (int i = 0; i < processors.Count; i++)
            {
                if (processors[i] != null)
                {
                    T typedProcessor = processors[i] as T;
                    if (typedProcessor != null && processorName.Equals(typedProcessor.Name, StringComparison.Ordinal))
                    {
                        found.Add(typedProcessor);
                    }
                }
            }

            return found;
        }

    }
}
