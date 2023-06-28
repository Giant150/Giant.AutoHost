using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Giant.AutoHost;

internal class DnsApiHelper
{
    [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
    private static extern UInt32 DnsFlushResolverCache();

    public static void FlushMyCache() //This can be named whatever name you want and is the function you will call
    {
        uint result = DnsFlushResolverCache();
    }
}
