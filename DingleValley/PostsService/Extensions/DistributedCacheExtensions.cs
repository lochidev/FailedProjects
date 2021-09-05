using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PostsService.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };
        public static async Task SetRecordAsync<T>(this IDistributedCache cache, string recordId, T data, TimeSpan? absoluteExpirationTime = null, TimeSpan? unusedExpirationTime = null)
        {
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationTime ?? TimeSpan.FromSeconds(60),
                SlidingExpiration = unusedExpirationTime
            };
            string jsonData = JsonSerializer.Serialize(data, jsonSerializerOptions);
            await cache.SetStringAsync(recordId, jsonData, options);

        }
        public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache, string recordId)
        {
            if (recordId.Contains("PostsService_"))
            {
                recordId = recordId.Replace("PostsService_", string.Empty);
            }
            string jsonData = await cache.GetStringAsync(recordId);
            if (jsonData is null)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(jsonData, jsonSerializerOptions);
        }
        public static string ToSha256(this string randomString)
        {
            SHA256Managed crypt = new SHA256Managed();
            string hash = string.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }
    }
}
