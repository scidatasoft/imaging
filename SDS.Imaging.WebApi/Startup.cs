using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Owin;
using Sds.Imaging.WebApi.Attributes;
using Sds.Imaging.WebApi.Modules;
using Sds.Storage.KeyValue.Core;
using Sds.Storage.KeyValue.Redis;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;

[assembly: OwinStartup(typeof(Sds.Imaging.WebApi.Startup))]

namespace Sds.Imaging.WebApi
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			LogConfig.RegisterLogs();

			var config = new HttpConfiguration();

			config.EnableCors();

			var jsonSerializerSettings = config.Formatters.JsonFormatter.SerializerSettings;

			//Remove unix epoch date handling, in favor of ISO
			jsonSerializerSettings.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff" });

			//Remove nulls from payload and save bytes
			jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;

			// Make json output camelCase
			jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

			var builder = new ContainerBuilder();
			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

			builder.RegisterType<RedisKeyValueStore>().As<IKeyValueStore>().SingleInstance();
			builder.RegisterType<RedisKeyValueRepository>().As<IKeyValueRepository>().SingleInstance();
			builder.RegisterType<CommonSettings>().SingleInstance();

			builder.RegisterModule(new BusModule(typeof(BusModule).Assembly)); // Scan for consumers in the current assembly

			var container = builder.Build();

			GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

			BusConfig.RegisterBus(app, container);

			// Attribute routing
			config.MapHttpAttributeRoutes();

			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

			config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
			config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
			config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

			// Use this filter for exceptions
			config.Filters.Add(new DefaultExceptionFilterAttribute());

			// WebApi
			app.UseWebApi(config);
		}
	}
}