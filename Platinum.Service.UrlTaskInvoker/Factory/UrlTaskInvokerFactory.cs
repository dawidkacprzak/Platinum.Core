using Platinum.Core.Types;

namespace Platinum.Service.UrlTaskInvoker.Factory
{
    public abstract class UrlTaskInvokerFactory
    {
        public abstract IUrlTaskInvoker GetInvoker();
    }
}