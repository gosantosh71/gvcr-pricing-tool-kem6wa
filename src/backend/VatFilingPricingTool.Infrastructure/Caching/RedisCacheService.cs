using Microsoft.Extensions.Logging; // version 6.0.0
using Microsoft.Extensions.Options; // version 6.0.0
using StackExchange.Redis; // version 2.6.48
using System;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Helpers;
using VatFilingPricingTool.Common.Models;

namespace VatFilingPricingTool.Infrastructure.Caching
{
    /// <summary>
    /// Provides a Redis-based caching service implementation for the VAT Filing Pricing Tool.
    /// </summary>
    public class RedisCacheService : IDisposable
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly CacheOptions _options;
        private readonly bool _isEnabled;
        private readonly string _instanceName;
        private readonly TimeSpan _defaultExpiration;

        /// <summary>
        /// Initializes a new instance of the RedisCacheService class with the specified options and logger.
        /// </summary>
        /// <param name="options">The cache configuration options.</param>
        /// <param name="logger">The logger for cache operations.</param>
        public RedisCacheService(IOptions<CacheOptions> options, ILogger<RedisCacheService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            if (options == null)
                throw new ArgumentNullException(nameof(options));
                
            _options = options.Value;
            _isEnabled = _options.Enabled;
            _instanceName = _options.InstanceName;
            _defaultExpiration = TimeSpan.FromMinutes(_options.DefaultExpirationMinutes);

            if (_isEnabled)
            {
                try
                {
                    var configOptions = new ConfigurationOptions
                    {
                        AbortOnConnectFail = _options.AbortOnConnectFail,
                        ConnectRetry = _options.ConnectionRetryCount,
                        ConnectTimeout = _options.ConnectionTimeoutSeconds * 1000 // Convert to milliseconds
                    };

                    _connection = ConnectionMultiplexer.Connect(_options.ConnectionString);
                    _database = _connection.GetDatabase();
                    
                    _logger.LogInformation("Redis cache service initialized successfully with instance name: {InstanceName}", _instanceName);
                }
                catch (Exception ex)
                {
                    _isEnabled = false;
                    _logger.LogError(ex, "Failed to initialize Redis cache service: {ErrorMessage}", ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("Redis cache service is disabled by configuration");
            }
        }

        /// <summary>
        /// Retrieves a cached item of the specified type by key.
        /// </summary>
        /// <typeparam name="T">The type of the cached item.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached item, or default(T) if not found.</returns>
        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!_isEnabled)
                return default;

            try
            {
                string formattedKey = FormatKey(key);
                RedisValue value = _database.StringGet(formattedKey);

                if (!value.HasValue)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", formattedKey);
                    return default;
                }

                _logger.LogDebug("Cache hit for key: {Key}", formattedKey);
                return JsonHelper.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item from cache with key: {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a cached item of the specified type by key.
        /// </summary>
        /// <typeparam name="T">The type of the cached item.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>A task that represents the asynchronous operation, containing the cached item or default(T) if not found.</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!_isEnabled)
                return default;

            try
            {
                string formattedKey = FormatKey(key);
                RedisValue value = await _database.StringGetAsync(formattedKey);

                if (!value.HasValue)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", formattedKey);
                    return default;
                }

                _logger.LogDebug("Cache hit for key: {Key}", formattedKey);
                return JsonHelper.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving item from cache with key: {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// Stores an item in the cache with the specified key and optional expiration time.
        /// </summary>
        /// <typeparam name="T">The type of the item to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The item to cache.</param>
        /// <param name="expiration">Optional expiration timespan. If null, the default expiration time will be used.</param>
        /// <returns>True if the item was successfully stored, false otherwise.</returns>
        public bool Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!_isEnabled)
                return false;

            try
            {
                string formattedKey = FormatKey(key);
                
                // If value is null, remove the key
                if (value == null)
                {
                    _logger.LogDebug("Removing key from cache as value is null: {Key}", formattedKey);
                    _database.KeyDelete(formattedKey);
                    return true;
                }

                string serializedValue = JsonHelper.Serialize(value);
                TimeSpan expirationTime = expiration ?? _defaultExpiration;

                bool result = _database.StringSet(formattedKey, serializedValue, expirationTime);
                _logger.LogDebug("Item stored in cache with key: {Key}, expiration: {Expiration}", formattedKey, expirationTime);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing item in cache with key: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Asynchronously stores an item in the cache with the specified key and optional expiration time.
        /// </summary>
        /// <typeparam name="T">The type of the item to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The item to cache.</param>
        /// <param name="expiration">Optional expiration timespan. If null, the default expiration time will be used.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the item was successfully stored, false otherwise.</returns>
        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!_isEnabled)
                return false;

            try
            {
                string formattedKey = FormatKey(key);
                
                // If value is null, remove the key
                if (value == null)
                {
                    _logger.LogDebug("Removing key from cache as value is null: {Key}", formattedKey);
                    await _database.KeyDeleteAsync(formattedKey);
                    return true;
                }

                string serializedValue = JsonHelper.Serialize(value);
                TimeSpan expirationTime = expiration ?? _defaultExpiration;

                bool result = await _database.StringSetAsync(formattedKey, serializedValue, expirationTime);
                _logger.LogDebug("Item stored in cache with key: {Key}, expiration: {Expiration}", formattedKey, expirationTime);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing item in cache with key: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Removes an item from the cache by key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>True if the item was successfully removed, false otherwise.</returns>
        public bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!_isEnabled)
                return false;

            try
            {
                string formattedKey = FormatKey(key);
                bool result = _database.KeyDelete(formattedKey);
                _logger.LogDebug("Item removed from cache with key: {Key}, result: {Result}", formattedKey, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cache with key: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Asynchronously removes an item from the cache by key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the item was successfully removed, false otherwise.</returns>
        public async Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!_isEnabled)
                return false;

            try
            {
                string formattedKey = FormatKey(key);
                bool result = await _database.KeyDeleteAsync(formattedKey);
                _logger.LogDebug("Item removed from cache with key: {Key}, result: {Result}", formattedKey, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cache with key: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Checks if an item exists in the cache by key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>True if the item exists, false otherwise.</returns>
        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!_isEnabled)
                return false;

            try
            {
                string formattedKey = FormatKey(key);
                bool exists = _database.KeyExists(formattedKey);
                _logger.LogDebug("Checked if key exists in cache: {Key}, result: {Exists}", formattedKey, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key exists in cache: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Asynchronously checks if an item exists in the cache by key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the item exists, false otherwise.</returns>
        public async Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!_isEnabled)
                return false;

            try
            {
                string formattedKey = FormatKey(key);
                bool exists = await _database.KeyExistsAsync(formattedKey);
                _logger.LogDebug("Checked if key exists in cache: {Key}, result: {Exists}", formattedKey, exists);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if key exists in cache: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Formats a cache key with the instance name prefix.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The formatted cache key.</returns>
        private string FormatKey(string key)
        {
            return string.IsNullOrEmpty(_instanceName) ? key : $"{_instanceName}:{key}";
        }

        /// <summary>
        /// Disposes the Redis connection.
        /// </summary>
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _logger.LogInformation("Redis connection disposed");
            }
        }
    }
}