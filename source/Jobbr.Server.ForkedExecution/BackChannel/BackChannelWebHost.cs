﻿using System;
using System.Net;
using System.Net.Sockets;
using Jobbr.ComponentModel.Registration;
using Jobbr.Server.ForkedExecution.Logging;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Starter;

namespace Jobbr.Server.ForkedExecution.BackChannel
{
    public class BackChannelWebHost : IJobbrComponent
    {
        private static readonly ILog Logger = LogProvider.For<BackChannelWebHost>();

        private readonly IJobbrServiceProvider jobbrServiceProvider;
        private readonly ForkedExecutionConfiguration configuration;

        private IDisposable webHost;

        public BackChannelWebHost(IJobbrServiceProvider jobbrServiceProvider, ForkedExecutionConfiguration configuration)
        {
            this.jobbrServiceProvider = jobbrServiceProvider;
            this.configuration = configuration;
        }

        private static int NextFreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public void Dispose()
        {
            
        }

        public void Start()
        {
            if (string.IsNullOrWhiteSpace(this.configuration.BackendAddress))
            {
                // Fallback to automatic endpoint port
                Logger.Warn("There was no BackendAdress specified. Falling back to random port, which is not guaranteed to work in production scenarios");
                var port = NextFreeTcpPort();

                this.configuration.BackendAddress = $"http://localhost:{port}";
            }


            var services = (ServiceProvider)ServicesFactory.Create();
            var options = new StartOptions()
            {
                Urls = { this.configuration.BackendAddress },
                AppStartup = typeof(Startup).FullName
            };

            // Pass through the IJobbrServiceProvider to allow Startup-Classes to let them inject this dependency to owin components
            services.Add(typeof(IJobbrServiceProvider), () => this.jobbrServiceProvider);

            var hostingStarter = services.GetService<IHostingStarter>();
            this.webHost = hostingStarter.Start(options);

            Logger.InfoFormat($"Started OWIN-Host for WebAPI at '{this.configuration.BackendAddress}'.");

        }

        public void Stop()
        {
            this.webHost.Dispose();
        }
    }
}
