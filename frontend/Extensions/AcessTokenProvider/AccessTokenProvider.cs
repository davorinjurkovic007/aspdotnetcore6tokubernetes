using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GloboTicket.Frontend.Extensions.AcessTokenProvider
{
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private static readonly SemaphoreSlim semaphore = new(1, 1);
        private readonly UriOptions uriOptions;
        private readonly IMemoryCache memoryCache;

        public AccessTokenProvider(IOptions<UriOptions> options, IMemoryCache cache)
        {
            uriOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
            memoryCache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<string?> Get(bool force = false, CancellationToken cancellationToken = default)
        {
            var key = $"Access_Token_{uriOptions.CustomResourceId}";

            if(force)
            {
                memoryCache.Remove(key);
            }


            bool isAvailable = memoryCache.TryGetValue(key, out string? token);
            if (isAvailable) return token;

            try
            {
                await semaphore.WaitAsync(cancellationToken);

                isAvailable = memoryCache.TryGetValue(key, out token);
                if (isAvailable) return token;

                var tokenCredential = new ClientSecretCredential(
                    tenantId: uriOptions.TenantId,
                    clientId: uriOptions.CustomClientId,
                    clientSecret: uriOptions.CustomClientSecret);

                var context = new TokenRequestContext(new[] { $"{uriOptions.CustomResourceId}/.default" });
                var response = await tokenCredential.GetTokenAsync(context, cancellationToken);

                return memoryCache.Set(key, response.Token, response.ExpiresOn);
            }
            catch
            {
                throw;
            }
            finally
            {
                semaphore.Release();
            }
            
        }
    }
}
