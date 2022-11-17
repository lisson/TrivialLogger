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

        public Queue<LogRequest> RequestQueue
        {
            get { return m_queue; }
            private set { m_queue = value; }
        }

        public LogRequestQueue()
        {
            m_queue = new Queue<LogRequest>();
        }

        public void Enqueue(LogRequest entry)
        {
            lock (m_queueLock)
            {
                m_queue.Enqueue(entry);
                OnPropertyChanged("RequestQueue");
            }
        }

        public LogRequest Dequeue()
        {
            LogRequest entry = null;
            lock (m_queueLock)
            {
                entry  = m_queue.Dequeue();
                OnPropertyChanged("RequestQueue");
            }
            return entry;
        }
    }
}
