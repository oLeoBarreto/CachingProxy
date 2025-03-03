namespace Caching_proxy;

public interface IRedisService
{
    Task<string?> GetRedisData(string key);
    Task SetRedisData(string key, string value);
    Task ClearRedisData();
}