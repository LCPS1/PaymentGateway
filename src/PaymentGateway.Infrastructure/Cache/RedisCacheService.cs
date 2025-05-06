using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.Common.Interfaces;
using PaymentGateway.Infrastructure.Cache.Options;

namespace PaymentGateway.Infrastructure.Cache
{
   public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly CacheSettings _settings;
        private readonly bool _isRedisAvailable;

        public RedisCacheService(
            IDistributedCache cache, 
            IOptions<CacheSettings> settings,
            ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _settings = settings.Value;
            _logger = logger;
            
            // Check if Redis appears to be configured
            _isRedisAvailable = !string.IsNullOrEmpty(_settings.ConnectionString);
            
            _logger.LogInformation(
                "Cache service initialized. Redis available: {IsAvailable}, Instance: {Instance}", 
                _isRedisAvailable, 
                _settings.InstanceName);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (!_isRedisAvailable)
                {
                    _logger.LogDebug("Redis not available, cache miss for key {Key}", key);
                    return default;
                }
                
                var cachedValue = await _cache.GetStringAsync(key);
                
                if (string.IsNullOrEmpty(cachedValue))
                    return default;
                    
                return JsonSerializer.Deserialize<T>(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data from cache for key {Key}", key);
                return default;  // Return default on error instead of throwing
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (!_isRedisAvailable)
            {
                _logger.LogDebug("Redis not available, skipping cache set for key {Key}", key);
                return;
            }
            
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(_settings.DefaultExpiryMinutes)
                };
                
                var serializedValue = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serializedValue, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting data in cache for key {Key}", key);
                // Don't throw - cache failures should not break the application
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (!_isRedisAvailable)
            {
                _logger.LogDebug("Redis not available, skipping cache remove for key {Key}", key);
                return;
            }
            
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing data from cache for key {Key}", key);
                // Don't throw - cache failures should not break the application
            }
        }

    }
}