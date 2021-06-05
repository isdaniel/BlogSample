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

        public ReadWriteLockBase(string name)
        {
            _redisDb = ConnectionMultiplexer.Connect("127.0.0.1:6999,password=cpredis").GetDatabase();
            _name = name;
            _hashKey = Guid.NewGuid().ToString();
            _threadID = Thread.CurrentThread.ManagedThreadId;
        }

        protected abstract bool TryInnerUnLock();
        protected abstract long? TryInnerLock(int threadID,long timeout);

        private long? tryAcquire(int threadId, long timeout)
        {
            internalLockLeaseTime = timeout;
            return TryInnerLock(threadId, timeout);
        }

        public bool TryLock(long timeout) 
        {
            long? ttl = tryAcquire(_threadID, timeout);
            // lock acquired
            if (!ttl.HasValue) {
                return true;
            }

            while (ttl.HasValue) {
                Thread.Sleep(new TimeSpan(ttl.Value));
                ttl = tryAcquire(_threadID, timeout);
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

    public class WriteLock : ReadWriteLockBase
    {
        public WriteLock(string name) : base(name)
        {
           
        }

        private static string lockScript = @"local mode = redis.call('hget', KEYS[1], 'mode');  
        if (mode == false) then  
          redis.call('hset', KEYS[1], 'mode', 'write');  
          redis.call('hset', KEYS[1], ARGV[2], 1);  
          redis.call('pexpire', KEYS[1], ARGV[1]);  
          return nil;  
        end;  
        if (mode == 'write') then  
          if (redis.call('hexists', KEYS[1], ARGV[2]) == 1) then  
	          redis.call('hincrby', KEYS[1], ARGV[2], 1);   
	          local currentExpire = redis.call('pttl', KEYS[1]);  
	          redis.call('pexpire', KEYS[1], currentExpire + ARGV[1]);  
	          return nil;  
          end;  
        end; 
        return redis.call('pttl', KEYS[1]);";
        protected override long? TryInnerLock(int threadID, long timeout)
        {
            var result = _redisDb.ScriptEvaluate(lockScript,
                new RedisKey[] { _name },
                new RedisValue[] { ToMillisecond(timeout), $"{GetLockName()}:write" });
            return result.IsNull ? default(long?) : (long)result;
        }


        private static string unLockscript = @"local mode = redis.call('hget', KEYS[1], 'mode');  
if (mode == false) then  
	return 1;  
end; 
if (mode == 'write') then  
	local lockExists = redis.call('hexists', KEYS[1], ARGV[3]);  
	if (lockExists == 0) then  
		return nil; 
	else  
		local counter = redis.call('hincrby', KEYS[1], ARGV[3], -1);  
		if (counter > 0) then  
			redis.call('pexpire', KEYS[1], ARGV[2]);  
			return 0;  
		else  
			redis.call('hdel', KEYS[1], ARGV[3]);  
			if (redis.call('hlen', KEYS[1]) == 1) then  
				redis.call('del', KEYS[1]);    
			else  
				--has unlocked read-locks
				redis.call('hset', KEYS[1], 'mode', 'read');  
			end;  
			return 1; 
		end;  
	end;  
end; 
return nil;";

        protected override bool TryInnerUnLock()
        {
            var result = _redisDb.ScriptEvaluate(unLockscript,
                new RedisKey[] { _name },
                new RedisValue[] { 0, internalLockLeaseTime, $"{GetLockName()}:write" });

            return !result.IsNull && (bool)result;

        }
    }

    public class ReadLock : ReadWriteLockBase
    {
        public ReadLock(string name) : base(name)
        {
       
        }

        private static string lockScript = @"local mode = redis.call('hget', KEYS[1], 'mode');  
        if (mode == false) then
            redis.call('hset', KEYS[1], 'mode', 'read');
            redis.call('hset', KEYS[1], ARGV[2], 1);  
            redis.call('set', KEYS[2] .. ':1', 1);  
            redis.call('pexpire', KEYS[2] .. ':1', ARGV[1]);  
            redis.call('pexpire', KEYS[1], ARGV[1]);  
            return nil;  
        end;  
        if (mode == 'read') or(mode == 'write' and redis.call('hexists', KEYS[1], ARGV[3]) == 1) then
            local ind = redis.call('hincrby', KEYS[1], ARGV[2], 1);   
            local key = KEYS[2].. ':' .. ind; 
            redis.call('set', key, 1);  
            redis.call('pexpire', key, ARGV[1]);  
            local remainTime = redis.call('pttl', KEYS[1]);
            redis.call('pexpire', KEYS[1], math.max(remainTime, ARGV[1]));  
            return nil;  
        end; 
        return redis.call('pttl', KEYS[1]);";

        protected override long? TryInnerLock(int threadID, long timeout)
        {
            var result = _redisDb.ScriptEvaluate(lockScript,
                new RedisKey[] { _name, GetReadWriteTimeOutPrefix() },
                new RedisValue[] { ToMillisecond(timeout), GetLockName(), $"{GetLockName()}:write" });
            return result.IsNull ? default(long?) : (long)result;
        }

        private static string unLockScript = @"local mode = redis.call('hget', KEYS[1], 'mode');  
--如果沒有鎖
if (mode == false) then  
	return 1;  
end; 
local lockExists = redis.call('hexists', KEYS[1], ARGV[2]);  
if (lockExists == 0) then  
	return nil; 
end;  
	
local counter = redis.call('hincrby', KEYS[1], ARGV[2], -1);   
if (counter == 0) then  
	redis.call('hdel', KEYS[1], ARGV[2]);   
end; 
redis.call('del', KEYS[2] .. ':' .. (counter+1));  

if (redis.call('hlen', KEYS[1]) > 1) then  
	local maxRemainTime = -3;   
	local keys = redis.call('hkeys', KEYS[1]);   
	for n, key in ipairs(keys) do   
		counter = tonumber(redis.call('hget', KEYS[1], key));   
		if type(counter) == 'number' then   
			for i=counter, 1, -1 do   
				local remainTime = redis.call('pttl', KEYS[3] .. ':' .. key .. ':rwlock_timeout:' .. i);   
				maxRemainTime = math.max(remainTime, maxRemainTime);  
			end;   
		end;   
	end;  
			
	if maxRemainTime > 0 then  
		redis.call('pexpire', KEYS[1], maxRemainTime);  
		return 0;  
	end;  
		
	if mode == 'write' then   
		return 0;  
	end;  
end;  

if  (redis.call('hlen', KEYS[1]) <= 1) then
   redis.call('del', KEYS[1]);  
   return 1;
end;

return 0;";

        protected override bool TryInnerUnLock()
        {
            var result = _redisDb.ScriptEvaluate(unLockScript,
                new RedisKey[] { _name, GetReadWriteTimeOutPrefix(), Prefix },
                new RedisValue[] { 0, GetLockName() });

            return !result.IsNull && (bool) result;

        }
    }
}