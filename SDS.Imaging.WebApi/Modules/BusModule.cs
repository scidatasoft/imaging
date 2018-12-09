using Autofac;
using MassTransit;
using System;
using System.Configuration;
using System.Reflection;

namespace Sds.Imaging.WebApi.Modules
{
	public class BusModule : Autofac.Module
	{
		private readonly Assembly[] _assembliesToScan;

		public BusModule(params Assembly[] assembliesToScan)
		{
			_assembliesToScan = assembliesToScan;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterConsumers(_assembliesToScan);

			// Creates our bus from the factory and registers it as a singleton against two interfaces
			builder.Register(c => Bus.Factory.CreateUsingRabbitMq(sbc => sbc.Host(new Uri(ConfigurationManager.AppSettings["RabbitMQHost"]), h =>
			{
				h.Username(ConfigurationManager.AppSettings["RabbitMQUsername"]);
				h.Password(ConfigurationManager.AppSettings["RabbitMQPassword"]);
			})))
				.As<IBusControl>()
				.As<IBus>()
				.SingleInstance();
		}
	}
}