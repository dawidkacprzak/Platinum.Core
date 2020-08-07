using Platinum.Core.Types;

namespace Platinum.Service.UrlTaskInvoker.Factory
{
    public class AllegroUrlTaskInvokerFactory : UrlTaskInvokerFactory
    {
        public override IUrlTaskInvoker GetInvoker()
        {
            return new AllegroTaskInvoker();
        }
    }
}