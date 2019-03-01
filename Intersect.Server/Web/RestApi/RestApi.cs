﻿using System;
using System.Web.Http;
using Intersect.Server.Web.RestApi.RouteProviders;
using JetBrains.Annotations;
using Microsoft.Owin.Hosting;
using Owin;

namespace Intersect.Server.Web.RestApi
{
    internal sealed class RestApi : IDisposable
    {
        [NotNull]
        public static StartOptions DefaultStartOptions => new StartOptions(
#if DEBUG
            "http://localhost:5401/"
#endif
        );

        public bool Disposing { get; private set; }

        public bool Disposed { get; private set; }

        [CanBeNull] private IDisposable mWebAppHandle;

        [NotNull]
        public StartOptions StartOptions { get; }

        public RestApi() : this(DefaultStartOptions)
        {

        }

        public RestApi([NotNull] StartOptions startOptions)
        {
            StartOptions = startOptions;
        }

        public void Start()
        {
            mWebAppHandle = WebApp.Start<Startup>(StartOptions);
        }

        internal sealed class Startup
        {
            public void Configuration(IAppBuilder appBuilder)
            {
                // Configure Web API for self-host. 
                var config = new HttpConfiguration();

                // Map routes
                config.MapHttpAttributeRoutes(new VersionedRouteProvider());

                // Make JSON the default response type for browsers
                config.Formatters?.JsonFormatter?.Map("accept", "text/html", "application/json");

                appBuilder.UseWebApi(config);
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (Disposed || Disposing)
                {
                    return;
                }

                Disposing = true;
            }

            mWebAppHandle?.Dispose();
            Disposed = true;
        }
    }
}
