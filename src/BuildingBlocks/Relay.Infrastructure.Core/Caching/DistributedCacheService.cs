using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Relay.CrossCutting.Caching;

namespace Relay.Infrastructure.Core.Caching;

/// <summary>
/// Default implementation of <see cref="ICacheService"/> over <see cref="IDistributedCache"/>.
/// Values are serialized as JSON. For an in-memory dev setup, register
/// <c>AddDistributedMemoryCache</c>.
/// </summary>
public sealed class DistributedCacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IDistributedCache _cache;

    public DistributedCacheService(IDistributedCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(key, cancellationToken);
        return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        var options = new DistributedCacheEntryOptions();
        if (ttl.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = ttl;
        }
        return _cache.SetAsync(key, bytes, options, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(key, cancellationToken);

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync<T>(key, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var value = await factory(cancellationToken);
        await SetAsync(key, value, ttl, cancellationToken);
        return value;
    }
}
