using System;
using System.Collections.Generic;

namespace Microsoft.ActivityInsights.Pipeline
{
    internal class ActivityPipeline : IActivityPipeline
    {
        private readonly List<IActivityProcessor> _processors = new List<IActivityProcessor>();
        private readonly List<IActivitySender> _senders = new List<IActivitySender>();

        public ActivityPipeline()
        {
            _processors = new List<IActivityProcessor>();
            _senders = new List<IActivitySender>();
        }

        public IList<IActivityProcessor> Processors
        {
            get
            {
                // We purposefully do not make any attempt to synchronize this collection.
                // Modifications should be rare and it is the responsibility of the user to make sure that monifications of this collection
                // or of individual senders do not happen cuncurrently to each other or to processing-and-senging of any activities.
                return _processors;
            }
        }

        public IList<IActivitySender> Senders
        {
            get
            {
                // We purposefully do not make any attempt to synchronize this collection.
                // Modifications should be rare and it is the responsibility of the user to make sure that monifications of this collection
                // or of individual senders do not happen cuncurrently to each other or to processing-and-senging of any activities.
                return _senders;
            }
        }

        public void ProcessAndSend(Activity activity)
        {
            if (activity == null)
            {
                return;
            }

            // We purposefully do not make any attempt to synchronize this collection.
            // Modifications should be rare and it is the responsibility of the user to make sure that monifications of this collection
            // or of individual senders do not happen cuncurrently to each other or to processing-and-senging of any activities.

            if (_processors.Count > 0)
            {
                Process(activity);
            }

            Send(activity);
        }

        private void Process(Activity activity)
        {
            bool continueProcessing = true;
            for (int i = 0; i < _processors.Count && continueProcessing; i++)
            {

                IActivityProcessor processor = _processors[i];
                if (processor != null)
                {
                    try
                    {
                        processor.ProcessActivity(activity, out continueProcessing);
                    }
                    catch (Exception ex)
                    {
                        continueProcessing = true;
                        LogInternalError(ex);
                    }
                }
            }
        }

        private void Send(Activity activity)
        {
            for (int i = 0; i < _senders.Count; i++)
            {
                IActivitySender sender = _senders[i];
                if (sender != null)
                {
                    try
                    {
                        sender.SendActivity(activity);
                    }
                    catch (Exception ex)
                    {
                        LogInternalError(ex);
                    }
                }
            }
        }

        internal void LogInternalError(Exception exception)
        {
            LogInternalError(exception, skipSenders: null);
        }

        private void LogInternalError(Exception exception, HashSet<IActivitySender> skipSenders)
        {
            if (exception == null)
            {
                return;
            }

            List<Exception> additionalExceptions = null;

            for (int i = 0; i < _senders.Count; i++)
            {
                IActivitySender sender = _senders[i];
                if (sender != null)
                {
                    try
                    {
                        if (skipSenders == null || false == skipSenders.Contains(sender))
                        {
                            sender.LogActivityInsightsError(exception);
                        }
                    }
                    catch (Exception ex)
                    {

                        skipSenders = skipSenders ?? new HashSet<IActivitySender>();
                        skipSenders.Add(sender);

                        additionalExceptions = additionalExceptions ?? new List<Exception>();
                        additionalExceptions.Add(ex);
                    }
                }
            }

            if (additionalExceptions != null)
            {
                for (int i = 0; i < additionalExceptions.Count; i++)
                {
                    LogInternalError(additionalExceptions[i], skipSenders);
                }
            }
        }
    }
}
