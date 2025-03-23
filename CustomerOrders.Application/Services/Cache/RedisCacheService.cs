using CustomerOrders.Core.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

/// <summary>
/// Manages caching for product data using Redis.
/// </summary>
public class RedisCacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    private IDatabase GetDatabase() => _connectionMultiplexer.GetDatabase();

    public async Task CacheProductListAsync(IEnumerable<Product> products)
    {
        try
        {
            var serializedProducts = JsonConvert.SerializeObject(products);
            await GetDatabase().StringSetAsync("product_list", serializedProducts, TimeSpan.FromMinutes(30));
            _logger.LogInformation("Product list cached successfully with {Count} items.", products.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while caching the product list.");
        }
    }

    /// <summary>
    /// Retrieves the product list from Redis if there is.
    /// </summary>
    /// <returns>The cached products or null if no data is found or an error occurs.</returns>
    public async Task<IEnumerable<Product>> GetProductListFromCacheAsync()
    {
        try
        {
            var cachedData = await GetDatabase().StringGetAsync("product_list");

            if (cachedData.IsNullOrEmpty)
            {
                _logger.LogInformation("No product list found in Redis cache.");
                return null;
            }

            _logger.LogInformation("Product list retrieved from Redis cache.");
            return JsonConvert.DeserializeObject<IEnumerable<Product>>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the product list from Redis.");
            return null;
        }
    }
}
