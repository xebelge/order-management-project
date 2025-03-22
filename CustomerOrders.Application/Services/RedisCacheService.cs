using CustomerOrders.Core.Entities;
using Newtonsoft.Json;
using StackExchange.Redis;

public class RedisCacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    private IDatabase GetDatabase() => _connectionMultiplexer.GetDatabase();

    public async Task CacheProductListAsync(IEnumerable<Product> products)
    {
        var serializedProducts = JsonConvert.SerializeObject(products);
        await GetDatabase().StringSetAsync("product_list", serializedProducts, TimeSpan.FromMinutes(30));
    }

    public async Task<IEnumerable<Product>> GetProductListFromCacheAsync()
    {
        var cachedData = await GetDatabase().StringGetAsync("product_list");
        if (cachedData.IsNullOrEmpty)
            return null;

        return JsonConvert.DeserializeObject<IEnumerable<Product>>(cachedData);
    }
}
