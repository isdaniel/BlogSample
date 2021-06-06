using StackExchange.Redis;

namespace RReadLockPOC
{
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
local lockExists = redis.call('hexists', KEYS[1], ARGV[1]);  
if (lockExists == 0) then  
	return nil; 
end;  
	
local counter = redis.call('hincrby', KEYS[1], ARGV[1], -1);   
if (counter == 0) then  
	redis.call('hdel', KEYS[1], ARGV[1]);   
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
                new RedisValue[] { GetLockName() });

            return !result.IsNull && (bool) result;

        }
    }
}