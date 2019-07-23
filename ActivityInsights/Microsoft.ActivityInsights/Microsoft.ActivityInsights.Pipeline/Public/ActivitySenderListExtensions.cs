using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights.Pipeline
{
    public static class ActivitySenderListExtensions
    {
        public static IActivitySender FindByName(this IList<IActivitySender> senders, string senderName)
        {
            Util.EnsureNotNull(senders, nameof(senders));
            senderName = Util.SpellNull(senderName);

            for (int i = 0; i < senders.Count; i++)
            {
                if (senders[i] != null && senderName.Equals(senders[i].Name, StringComparison.Ordinal))
                {
                    return senders[i];
                }
            }

            return null;
        }

        public static T FindByName<T>(this IList<IActivitySender> senders, string senderName) where T : class, IActivitySender
        {
            Util.EnsureNotNull(senders, nameof(senders));
            senderName = Util.SpellNull(senderName);

            for (int i = 0; i < senders.Count; i++)
            {
                if (senders[i] != null)
                {
                    T typedProcessor = senders[i] as T;
                    if (typedProcessor != null && senderName.Equals(typedProcessor.Name, StringComparison.Ordinal))
                    {
                        return typedProcessor;
                    }
                }
            }

            return null;
        }

        public static ICollection<IActivitySender> FindAllByName(this IList<IActivitySender> senders, string senderName)
        {
            Util.EnsureNotNull(senders, nameof(senders));
            senderName = Util.SpellNull(senderName);

            var found = new List<IActivitySender>();
            for (int i = 0; i < senders.Count; i++)
            {
                if (senders[i] != null && senderName.Equals(senders[i].Name, StringComparison.Ordinal))
                {
                    found.Add(senders[i]);
                }
            }

            return found;
        }

        public static ICollection<T> FindAllByName<T>(this IList<IActivitySender> senders, string senderName) where T : class, IActivitySender
        {
            Util.EnsureNotNull(senders, nameof(senders));
            senderName = Util.SpellNull(senderName);

            var found = new List<T>();
            for (int i = 0; i < senders.Count; i++)
            {
                if (senders[i] != null)
                {
                    T typedProcessor = senders[i] as T;
                    if (typedProcessor != null && senderName.Equals(typedProcessor.Name, StringComparison.Ordinal))
                    {
                        found.Add(typedProcessor);
                    }
                }
            }

            return found;
        }

    }
}
