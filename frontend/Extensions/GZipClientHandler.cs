namespace GloboTicket.Frontend.Extensions
{
    public class GZipClientHandler : HttpClientHandler
    {
        public GZipClientHandler() 
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip;
        }

    }
}
