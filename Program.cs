using System.Net;
using Caching_proxy;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;

class CachingProxy
{
    private static IRedisService _redisService;
    private static readonly HttpClient _httpClient = new();

    static async Task Main(string[] args)
    {
        InitializeRedis();
        
        // Starting variables
        var port = 0;
        string originUrl = null;
        var clearCache = false;

        // Loop to get parameters
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--port": 
                    port = Int32.Parse(args[++i]);
                    break;
                case "--origin":
                    originUrl = args[++i];
                    break;
                case "--clear-cache":
                    clearCache = true;
                    break;
            }
        }

        // If the user informed --clear-cache just clear all cache
        if (clearCache)
        {
            await _redisService.ClearRedisData();
            Console.WriteLine("Cleared cache");
            return;
        }

        // Verify if the user provided a valid port and origin 
        if (port == 0 || originUrl == null)
        {
            Console.WriteLine("You need to provide a valid port and origin!");
        }
        
        // Start listening the request from localhost
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();
        
        Console.WriteLine($"Caching proxy started on {port}");

        while (true)
        {
            var context = await listener.GetContextAsync();
            _ = HandleRequests(context, originUrl);
        }
    }

    private static Task InitializeRedis()
    {
        var redisConnection = ConnectionMultiplexer.Connect("localhost:6379");

        var cacheOptions = new RedisCacheOptions
        {
            Configuration = "localhost:6379"
        };

        var distributedCache = new RedisCache(cacheOptions);

        _redisService = new RedisService(distributedCache, redisConnection);
        return Task.CompletedTask;
    }

    private static async Task HandleRequests(HttpListenerContext context, string origin)
    {
        var requestPath = context.Request.Url.PathAndQuery;
        var requestUrl = $"{origin}{requestPath}";
        
        Console.WriteLine($"Response from {requestUrl}");

        // Verify if already cached response
        var cachedResponse = await _redisService.GetRedisData(requestUrl);
        if (cachedResponse != null)
        {
            Console.WriteLine($"Cached response found!");
            context.Response.Headers.Add("X-Cache", "HIT");
            await HandleResponse(context, cachedResponse);
        }
        else
        {
            try
            {
                context.Response.Headers.Add("X-Cache", "MISS");
            
                Console.WriteLine($"Sending request to {requestUrl}");
            
                // If the response is not cached will send a request to origin
                var forwardRequest = new HttpRequestMessage(new HttpMethod(context.Request.HttpMethod), requestUrl);
                var forwardResponse = await _httpClient.SendAsync(forwardRequest);
            
                var response = await forwardResponse.Content.ReadAsStringAsync();
            
                // Will write the response in cache
                await _redisService.SetRedisData(requestUrl, response);

                await HandleResponse(context, response); 
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error to send forwarding request: {e}");
                await HandleResponse(context, "Internal Server Error!"); 
            }      
        }
    }

    private static async Task HandleResponse(HttpListenerContext context, string response)
    {
        context.Response.ContentType = "application/json";
        await using (var writer = new StreamWriter(context.Response.OutputStream))
        {
            await writer.WriteAsync(response);
        }
        context.Response.Close();
    }
}