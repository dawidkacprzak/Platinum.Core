using RestSharp;

namespace Platinum.Core.Types
{
    public interface IRest
    {
        public IRestResponse Get(IRestRequest request);
        public IRestResponse Post(IRestRequest request);
    }
}