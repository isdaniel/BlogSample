using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QueueWorkerEngine.RabbitMq;

namespace QueueWorkerEngine
{
    public class BackgroundWorker : BackgroundService
    {
        private readonly ILogger<BackgroundWorker> _logger;
        private readonly RabbitMqWorkerBase _worker;

        public BackgroundWorker(ILogger<BackgroundWorker> logger, RabbitMqWorkerBase worker)
        {
            this._worker = worker;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken token)
        {
            _worker.CreateWorkUnit(token);
            token.WaitHandle.WaitOne();
            _logger.LogInformation("ExecuteAsync Finish!");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start Stop...Wait for queue to comsume stop.");
            await _worker.GracefulShutDown(cancellationToken);
            _logger.LogInformation("Stop Service.");
            await base.StopAsync(cancellationToken);
        }
    }
}
