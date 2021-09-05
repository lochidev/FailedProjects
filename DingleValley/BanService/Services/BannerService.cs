
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace BanService
{
    public class BannerService : Banner.BannerBase
    {
        private readonly IMemoryCache _cache;
        public BannerService(IMemoryCache cache)
        {
            _cache = cache;
        }
        public override Task<BanResponse> BanUser(BanRequest request, ServerCallContext context)
        {
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(request.Duration.ToTimeSpan());
            _cache.Set($"{request.Username}_{request.ServiceId}", request.ServiceId, cacheEntryOptions);
            return Task.FromResult(new BanResponse { IsBanned = true });
        }
        public override Task<IsBannedResponse> IsBanned(IsBannedRequest request, ServerCallContext context)
        {
            return Task.FromResult(new IsBannedResponse { IsBanned = _cache.TryGetValue($"{request.Username}_{request.ServiceId}", out _) });
        }
        public override Task<UnbanResponse> UnbanUser(UnbanRequest request, ServerCallContext context)
        {
            if (!string.IsNullOrEmpty(request.Username) || !string.IsNullOrEmpty(request.ServiceId))
            {
                _cache.Remove($"{request.Username}_{request.ServiceId}");
                return Task.FromResult(new UnbanResponse { Success = true });
            }
            return Task.FromResult(new UnbanResponse { Success = false });
        }
    }
}
