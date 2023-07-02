using GloboTicket.Frontend.Extensions;
using GloboTicket.Frontend.Extensions.AcessTokenProvider;
using GloboTicket.Frontend.Extensions.DelegatingHandlers;
using GloboTicket.Frontend.Models;
using GloboTicket.Frontend.Services;
using GloboTicket.Frontend.Services.Ordering;
using GloboTicket.Frontend.Services.ShoppingBasket;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace GloboTicket.Frontend
{
    public static class Configure
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services)
        {
            AddDependencyInjectionService(services);

            services.AddOptions<UriOptions>()
                .Configure<IConfiguration>((uriOptions, configuration) =>
                {
                    uriOptions.OrderingUrl = new Uri(configuration.GetValue<string>("ApiConfigs:Ordering:Uri") ?? throw new InvalidOperationException("Missing config"));
                    uriOptions.CatalogUrl = new Uri(configuration.GetValue<string>("ApiConfigs:ConcertCatalog:Uri") ?? throw new InvalidOperationException("Missing config"));
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }

        private static void AddDependencyInjectionService(IServiceCollection services)
        {
            var retryPolicy = HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        .OrResult(msg => msg.IsSuccessStatusCode == false)
                        .Or<SocketException>()
                        .WaitAndRetryAsync(
                            retryCount: 3,
                            attempt => TimeSpan.FromMicroseconds(attempt *10) + TimeSpan.FromMicroseconds(RandomNumberGenerator.GetInt32(0,100)));

            services.AddScoped<GZipClientHandler>();
            //services.AddScoped<IAccessTokenProvider, AccessTokenProvider>();
            //services.AddScoped<CustomAuthenticationDelegatingHandler>();

            services.AddSingleton<IShoppingBasketService, InMemoryShoppingBasketService>();
            services.AddSingleton<Settings>();

            services.AddCustomHttpClient<IConcertCatalogService, ConcertCatalogService>(retryPolicy, o => o.CatalogUrl);
            services.AddCustomHttpClient<IOrderSubmissionService, HttpOrderSubmissionService>(retryPolicy, o => o.OrderingUrl);
        }

        private static void AddCustomHttpClient<TClient, IImplementation>(
            this IServiceCollection services,
            AsyncRetryPolicy<HttpResponseMessage> retryPolicy,
            Func<UriOptions, Uri> getBaseUrl)
        where TClient : class where IImplementation : class, TClient
        {
            services
                .AddHttpClient<TClient, IImplementation>((sp, c) =>
                {
                    var options = sp.GetRequiredService<IOptions<UriOptions>>().Value;

                    c.BaseAddress = getBaseUrl(options);
                    //c.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", options.AzureSubscriptionKey);
                    c.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                })
                //.AddHttpMessageHandler<CustomAuthenticationDelegatingHandler>()
                .ConfigurePrimaryHttpMessageHandler<GZipClientHandler>()
                .AddPolicyHandler(retryPolicy);
        }
    }
}
