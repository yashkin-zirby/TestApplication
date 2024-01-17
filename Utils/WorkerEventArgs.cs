using System;

namespace TestApplication.Utils
{
    public delegate void WorkerDoTaskHandler(WorkerEventArgs args);
    public class WorkerEventArgs : EventArgs
    {
        private long progress;
        private long count;
        public long Count { get { return count; } }
        public long Progress { get { return progress; } }
        public WorkerEventArgs(long progress, long count) : base() {
            this.progress = progress;
            this.count = count;
        }
    }
}
