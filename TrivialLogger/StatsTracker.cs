using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrivialLogger
{
    class StatsTracker
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private StatsTracker() {}

        private static CancellationTokenSource m_cancelTokenSrc;
        private static Task m_collectTask;

        private static long m_RequestsServiced = 0;
        private static long m_ServiceTimeMs = 0;
        private static Object m_serviceTimeLock = new object();

        public long RequestsServiced
        {
            get
            {
                return m_RequestsServiced;
            }
            private set
            {
                m_RequestsServiced = value;
            }
        }
        public long ServiceTimeMs
        {
            get
            {
                return m_ServiceTimeMs;
            }
            private set
            {
                m_ServiceTimeMs = value;
            }
        }

        private static bool m_StartLoop()
        {
            long lastCount = 0;
            long lastTime = 0;
            int delayMs = 10000;
            while(true)
            {
                Task.Delay(delayMs).GetAwaiter().GetResult();
                long delta = m_RequestsServiced - lastCount;
                long deltaMs = m_ServiceTimeMs - lastTime;
                double average = delta / deltaMs;
                lastCount = m_RequestsServiced;
                if (delta > 0)
                {
                    log.Debug($"Serviced {delta} requests in {deltaMs} ms. Average: {average} ms / request");
                    log.Debug($"Total serviced {m_RequestsServiced} requests.");
                    log.Debug($"Total service time {m_ServiceTimeMs} ms.");
                }
            }
        }

        public static void IncrementRequestsServiced()
        {
            Interlocked.Increment(ref m_RequestsServiced);
        }

        public static void AddServiceTime(long duration)
        {
            lock (m_serviceTimeLock)
            {
                m_ServiceTimeMs = m_ServiceTimeMs + duration;
            }
        }

        public static bool Collect()
        {
            m_cancelTokenSrc = new CancellationTokenSource();
            var token = m_cancelTokenSrc.Token;
            m_collectTask = Task.Factory.StartNew(() => m_StartLoop(), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            return true;
        }

        public static void Stop()
        {
            if(m_cancelTokenSrc == null)
            {
                return;
            }

            m_cancelTokenSrc.Cancel();
        }
    }
}
