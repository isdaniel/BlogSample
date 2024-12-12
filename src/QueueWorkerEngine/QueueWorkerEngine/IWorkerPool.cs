using System.Threading.Tasks;

namespace QueueWorkerEngine
{
    public interface IWorkerPool
    {
        Task<bool> AddTaskAsync(MessageTask task);
        Task WaitFinishedAsync();
    }
}
