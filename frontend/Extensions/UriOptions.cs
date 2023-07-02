namespace GloboTicket.Frontend.Extensions
{
    public class UriOptions
    {
        public Uri CatalogUrl { get; set; } = default!;
        public Uri OrderingUrl { get; set; } = default!;
        public string AzureSubscriptionKey { get; set; } = default!;
        public string CustomResourceId { get; set; } = default!;
        public string TenantId { get; set; } = default!;
        public string CustomClientId { get; set; } = default!;
        public string CustomClientSecret { get; set;} = default!;
    }
}
