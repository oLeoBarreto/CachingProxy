using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Caching_proxy;

public class RedisService(IDistributedCache cache, ConnectionMultiplexer redis) : IRedisService
{
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public async Task<string?> GetRedisData(string key)
    {
        try
        {
            var result = await cache.GetStringAsync(key);
            
            return result != null ? JsonSerializer.Deserialize<string>(result) : null;
        }
        catch (Exception e)
        {
            throw new Exception($"Something went wrong: {e}");
        }
    }

    public async Task SetRedisData(string key, string value)
    {
        try
        {
            await cache.SetStringAsync(
                key,
                JsonSerializer.Serialize(value),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheExpiry }
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Something went wrong: {e}");
        }
    }

    public async Task ClearRedisData()
    {
        try
        {
            var server = redis.GetServer(redis.GetEndPoints()[0]);
            var keys = server.Keys();

            foreach (var key in keys)
            {
                await cache.RemoveAsync(key);
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Something went wrong: {e}");
        }
    }
}