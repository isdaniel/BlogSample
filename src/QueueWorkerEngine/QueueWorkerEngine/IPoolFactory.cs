using System.Collections.Generic;

namespace QueueWorkerEngine
{
    public interface IPoolFactory
    {
        Dictionary<string, IWorkerPool> GetPools(PoolSetting[] setting, PoolType poolType);
    }
}
