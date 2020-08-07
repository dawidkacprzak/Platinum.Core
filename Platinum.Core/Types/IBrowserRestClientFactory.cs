namespace Platinum.Core.Types
{
    public interface IBrowserRestClientFactory
    {
        IBrowserRestClient GetBrowser(string host);
    }
}