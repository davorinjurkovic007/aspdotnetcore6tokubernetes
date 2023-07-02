namespace GloboTicket.Frontend.Extensions.AcessTokenProvider
{
    public interface IAccessTokenProvider
    {
        Task<string?> Get(bool force = false, CancellationToken cancellationToken = default);
    }
}
