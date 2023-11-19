using StackExchange.Redis;

namespace RReadLockPOC
{
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
	local lockExists = redis.call('hexists', KEYS[1], ARGV[2]);  
	if (lockExists == 0) then  
		return nil; 
	else  
		local counter = redis.call('hincrby', KEYS[1], ARGV[2], -1);  
		if (counter > 0) then  
			redis.call('pexpire', KEYS[1], ARGV[1]);  
			return 0;  
		else  
			redis.call('hdel', KEYS[1], ARGV[2]);  
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
                new RedisValue[] { internalLockLeaseTime, $"{GetLockName()}:write" });

            return !result.IsNull && (bool)result;

        }
    }
}