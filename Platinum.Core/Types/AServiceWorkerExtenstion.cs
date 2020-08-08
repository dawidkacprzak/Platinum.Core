using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;

namespace Platinum.Core.Types
{
    [ExcludeFromCodeCoverage]
    public static class AServiceWorkerExtenstion
    {
        public static IHostBuilder UseSystemDependedService(this IHostBuilder host)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    return host.UseWindowsService();
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    return host.UseSystemd();
                default: throw new NotImplementedException();
            }
        }
    }
}