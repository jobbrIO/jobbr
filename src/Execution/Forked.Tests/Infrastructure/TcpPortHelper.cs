﻿using System.Net;
using System.Net.Sockets;

namespace Jobbr.Server.ForkedExecution.Tests.Infrastructure
{
    public static class TcpPortHelper
    {
        public static int NextFreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}