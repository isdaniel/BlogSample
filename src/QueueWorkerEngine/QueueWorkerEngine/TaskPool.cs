using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace QueueWorkerEngine
{

    public class ThreadPool : IWorkerPool
    {
        private readonly int _limitThreadCount;
        private BlockingCollection<MessageTask> _taskQueue;
        public Task[] _threadPool;
        private volatile bool _finish;
        public ThreadPool(int limitThreadCount,int prefetchTaskCount)
        {
            _taskQueue = new BlockingCollection<MessageTask>(prefetchTaskCount);
            this._limitThreadCount = limitThreadCount;
            _threadPool = new Task[limitThreadCount];
            InitPool();
        }

        public Task<bool> AddTaskAsync(MessageTask task)
        {
            bool result = false;
            if (!_finish)
            {
                _taskQueue.Add(task);
                result = true;
            }
            return Task.FromResult(result);
        }

        private void InitPool()
        {
            for (int i = 0; i < _limitThreadCount; i++)
            {
                _threadPool[i] = Task.Run(() => {
                    ProcessHandler();
                });
            }
        }

        private void ProcessHandler()
        {
            while (true)
            {
                while(_taskQueue.Count > 0){
                    var task = _taskQueue.Take();
                    if(task != null)
                        task.Execute();
                }

                if (_finish && _taskQueue.Count == 0)
                {
                    break;
                }
            }
        }

        public async Task WaitFinishedAsync()
        {
            _finish = true;
            _taskQueue.CompleteAdding();

            await Task.WhenAll(_threadPool);
        }
    }
}
