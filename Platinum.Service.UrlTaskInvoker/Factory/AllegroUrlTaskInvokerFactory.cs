using System.Diagnostics.CodeAnalysis;
using Platinum.Core.Types;

namespace Platinum.Service.UrlTaskInvoker.Factory
{        
    [ExcludeFromCodeCoverage]
    public class AllegroUrlTaskInvokerFactory : UrlTaskInvokerFactory
    {
        public override IUrlTaskInvoker GetInvoker()
        {
            return new AllegroTaskInvoker();
        }
    }
}