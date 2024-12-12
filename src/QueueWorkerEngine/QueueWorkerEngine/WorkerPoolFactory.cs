using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QueueWorkerEngine
{
    public class WorkerPoolFactory : IPoolFactory {
        ILoggerFactory _loggerFactory;
        public WorkerPoolFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public Dictionary<string,IWorkerPool> GetPools(PoolSetting[] setting,PoolType poolType){
            if(poolType == PoolType.Process){

                if(setting.Any(x=>string.IsNullOrEmpty(x.FileName))){
                    throw new ArgumentException("PoolType.Process need to declare FilePath!");
                }

                return setting.ToDictionary(x=>x.Group, y => (IWorkerPool)new ProcessPool(y, _loggerFactory));
            }
            
            return setting.ToDictionary(x=>x.Group,y=> (IWorkerPool)new ThreadPool(y.WorkUnitCount,1));
        }
    }
}
