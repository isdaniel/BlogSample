using System;
using System.Threading;
using StackExchange.Redis;

namespace RReadLockPOC
{
    public abstract class ReadWriteLockBase
    {
        protected IDatabase _redisDb;
        protected long internalLockLeaseTime;
        protected readonly string _name;
        private int _threadID;
        protected string _hashKey;

        protected ReadWriteLockBase(string name)
        {
            _redisDb = ConnectionMultiplexer.Connect("127.0.0.1:6999,password=cpredis").GetDatabase();
            _name = name;
            _hashKey = Guid.NewGuid().ToString();
            _threadID = Thread.CurrentThread.ManagedThreadId;
        }

        protected abstract bool TryInnerUnLock();
        protected abstract long? TryInnerLock(int threadID,long timeout);

        private long? Acquire(int threadId, long timeout)
        {
            internalLockLeaseTime = timeout;
            return TryInnerLock(threadId, timeout);
        }

        public bool TryLock(long timeout) 
        {
            long? ttl = Acquire(_threadID, timeout);
            // lock acquired
            if (!ttl.HasValue) {
                return true;
            }

            while (ttl.HasValue) {
                Thread.Sleep(new TimeSpan(ttl.Value));
                ttl = Acquire(_threadID, timeout);
            }
            return true;
        }

        public bool UnLock()
        {
            return TryInnerUnLock();
        }

        protected string Prefix => "{anyLock}";
        protected string GetReadWriteTimeOutPrefix()
        {
            return $"{Prefix}:{_hashKey}:{_threadID.ToString()}";
        }

        protected string GetLockName()
        {
            return $"{_hashKey}:{_threadID}";
        }

        protected long ToMillisecond(long timeout)
        {
            return timeout * 1000;
        }
    }
}