using System;
using System.Threading;
using Autofac;
using MassTransit;
using Microsoft.Owin;
using Owin;

namespace Sds.Imaging.WebApi
{
    public static class BusConfig
    {
        public static void RegisterBus(IAppBuilder app, IContainer container)
        {
			// Starts Mass Transit Service bus, and registers stopping of bus on app dispose
			var bus = container.Resolve<IBusControl>();
			var busHandle = bus.Start();

			if (app.Properties.ContainsKey("host.OnAppDisposing"))
			{
				var context = new OwinContext(app.Properties);
				var token = context.Get<CancellationToken>("host.OnAppDisposing");
				if (token != CancellationToken.None)
				{
					token.Register(() => busHandle.Stop(TimeSpan.FromSeconds(30)));
				}
			}
		}
	}
}
