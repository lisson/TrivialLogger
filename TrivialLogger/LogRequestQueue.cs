using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TrivialLogger
{
    class LogRequestQueue : ObservableObject
    {
        private Queue<LogRequest> m_queue;
        private Object m_queueLock = new object();

        public int TotalQueued
        {
            get;
            private set;
        }

        public int TotalDequeued
        {
            get;
            private set;
        }

        public Queue<LogRequest> RequestQueue
        {
            get { return m_queue; }
            private set { m_queue = value; }
        }

        public LogRequestQueue()
        {
            m_queue = new Queue<LogRequest>();
            TotalQueued = 0;
            TotalDequeued = 0;
        }

        public void Enqueue(LogRequest entry)
        {
            lock (m_queueLock)
            {
                m_queue.Enqueue(entry);
                TotalQueued++;
                OnPropertyChanged("RequestQueue");
            }
        }

        public LogRequest Dequeue()
        {
            LogRequest entry = null;
            lock (m_queueLock)
            {
                if(m_queue.Count == 0)
                {
                    return null;
                }
                entry  = m_queue.Dequeue();
                TotalDequeued = 0;
                OnPropertyChanged("RequestQueue");
            }
            return entry;
        }
    }
}
