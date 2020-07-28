using System;

namespace Platinum.Core.Types.Exceptions
{
    public class DalException : Exception
    {
        public DalException(string message) : base(message)
        {
            
        }
    }
}