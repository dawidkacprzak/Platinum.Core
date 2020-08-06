namespace Platinum.Core.Types
{
    public interface IBufforUrlQueueService
    {
        public void Run(IDal db, IBufforUrlQueueTask task);
    }
}