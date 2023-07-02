using GloboTicket.Frontend.Extensions.AcessTokenProvider;

namespace GloboTicket.Frontend.Extensions.DelegatingHandlers
{
    public class CustomAuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IAccessTokenProvider accessTokenProvider;

        public CustomAuthenticationDelegatingHandler(IAccessTokenProvider accessTokenProvider)
        {
            this.accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(await AddAuthorization(request, false), cancellationToken);

            if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return await base.SendAsync(await AddAuthorization(request, true), cancellationToken);
            }

            return response;
        }

        private async Task<HttpRequestMessage> AddAuthorization(HttpRequestMessage request, bool forceRefresh)
        {
            var accessToken = await accessTokenProvider.Get(forceRefresh);

            if (accessToken != null)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }

            return request;
        }
    }
}
