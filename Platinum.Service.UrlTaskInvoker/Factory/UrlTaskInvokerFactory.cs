using System.Diagnostics.CodeAnalysis;
using Platinum.Core.Types;

namespace Platinum.Service.UrlTaskInvoker.Factory
{
    [ExcludeFromCodeCoverage]
    public abstract class UrlTaskInvokerFactory
    {
        public abstract IUrlTaskInvoker GetInvoker();
    }
}