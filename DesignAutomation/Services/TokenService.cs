using Autodesk.Authentication;
using Autodesk.Authentication.Model;
using DesignAutomation.Config;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DesignAutomation.Services
{
    public class TokenService
    {
        private readonly AuthenticationClient _authClient;
        private readonly APSOptions _options;
        private readonly IMemoryCache _cache;

        private const string CacheKey = "APS_TOKEN_DESIGN_AUTOMATION";

        public TokenService(IOptions<APSOptions> options, IMemoryCache cache, AuthenticationClient authClient)
        {
            _options = options.Value;
            _cache = cache;
            _authClient = authClient;
        }

        public async Task<TwoLeggedToken> GetTokenAsync()
        {
            if (_cache.TryGetValue(CacheKey, out TwoLeggedToken cachedToken))
                return cachedToken;

            var scopes = new List<Scopes> { Scopes.CodeAll, Scopes.DataRead, Scopes.DataWrite, Scopes.BucketCreate, Scopes.BucketRead };
            TwoLeggedToken authResult = await _authClient.GetTwoLeggedTokenAsync(_options.ClientId, _options.ClientSecret, scopes);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds((double)authResult.ExpiresIn - 60));
            _cache.Set(CacheKey, authResult, cacheOptions);

            return authResult;
        }
    }
}
