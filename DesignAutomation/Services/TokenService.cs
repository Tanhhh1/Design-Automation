using Autodesk.Authentication;
using Autodesk.Authentication.Model;
using DesignAutomation.Config;
using Microsoft.Extensions.Options;

namespace DesignAutomation.Services
{
    public class TokenService
    {
        private readonly AuthenticationClient _authClient;
        private readonly APSOptions _options;

        private static Token _cachedToken;

        public TokenService(IOptions<APSOptions> options)
        {
            _options = options.Value;
            _authClient = new AuthenticationClient();
        }

        public async Task<Token> GetTokenAsync()
        {
            if (_cachedToken != null && _cachedToken.ExpireAt > DateTime.UtcNow)
                return _cachedToken;

            var scopes = new List<Scopes>{ Scopes.CodeAll, Scopes.DataRead, Scopes.DataWrite, Scopes.BucketCreate, Scopes.BucketRead };

            var authResult = await _authClient.GetTwoLeggedTokenAsync(
                _options.ClientId,
                _options.ClientSecret,
                scopes,
                false
            );

            double expiresInSeconds = authResult.ExpiresIn ?? 3600;

            var token = new Token
            {
                access_token = authResult.AccessToken,
                expires_in = expiresInSeconds,
                ExpireAt = DateTime.UtcNow.AddSeconds(expiresInSeconds - 60)
            };

            _cachedToken = token;
            return token;
        }
    }
}
