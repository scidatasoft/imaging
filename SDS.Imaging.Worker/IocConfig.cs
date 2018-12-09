using Autofac;
using Sds.Imaging.Worker.Modules;
using Sds.Storage.KeyValue.Core;
using Sds.Storage.KeyValue.Redis;
using System.Reflection;

namespace Sds.Imaging.Worker
{
	public class IocConfig
	{
		internal static IContainer RegisterDependencies(ImagingOptions options)
		{
			var builder = new ContainerBuilder();

			builder.RegisterModule(new BusModule(Assembly.GetExecutingAssembly()));

			builder.RegisterType<RedisKeyValueStore>().As<IKeyValueStore>().SingleInstance();
			builder.RegisterType<RedisKeyValueRepository>().As<IKeyValueRepository>().SingleInstance();
			builder.RegisterType<Processor>().As<IProcessor>();
			builder.RegisterInstance(options);

			return builder.Build();
		}
	}
}
