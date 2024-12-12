using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace QueueWorkerEngine
{
    public interface IWorker
    {
        Task<bool> ExecuteAsync(BasicDeliverEventArgs args, CancellationToken token);
        Task GracefulReleaseAsync(CancellationToken token);
    }
}
