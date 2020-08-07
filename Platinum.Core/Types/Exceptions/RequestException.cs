using System;
using RestSharp;

namespace Platinum.Core.Types.Exceptions
{
    public class RequestException : Exception
    {
        public IRestResponse Response;
        
        public RequestException(string message) : base(message)
        {
        }
        
        public RequestException(string message, IRestResponse response) : base(message)
        {
            Response = response;
        }
    }
}