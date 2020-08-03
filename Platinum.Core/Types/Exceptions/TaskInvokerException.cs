using System;

namespace Platinum.Core.Types.Exceptions
{
    public class TaskInvokerException : Exception
    {
        public TaskInvokerException() : base()
        {
                
        }

        public TaskInvokerException(string message) : base(message)
        {
            
        }

        public TaskInvokerException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}