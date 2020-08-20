namespace Platinum.Core.Types
{
    public interface IBrowserRestClientFactory
    {
        IBrowserClient GetBrowser(string host);
    }
}