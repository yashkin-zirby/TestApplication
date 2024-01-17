namespace TestApplication.Utils
{
    public class WorkerCombineEventArgs: WorkerEventArgs
    {
        private long deletedRows;
        public long DeletedRows { get { return deletedRows; } }
        public WorkerCombineEventArgs(long progress, long count, long deletedRows) : base(progress, count)
        {
            this.deletedRows = deletedRows;
        }
    }
}
