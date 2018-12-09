using Autofac;
using MassTransit;
using Serilog;
using System;

namespace Sds.Imaging.Worker
{
	class Receiver
	{
		private static IContainer Container { get; set; }
		private static IBusControl BusControl { get; set; }
		private static BusHandle BusHandle { get; set; }

		public static void Main(string[] args)
		{
			try
			{
				var options = new ImagingOptions();

				Container = IocConfig.RegisterDependencies(options);
				LogConfig.RegisterLogs();

				if (CommandLine.Parser.Default.ParseArguments(args, options))
				{
					Log.Error("Invalid parameters");

					// Values are available here
					if (options.Verbose)
					{
						Log.Information($"Queue Name: {options.QueueName}");
						Log.Information($"Usage: {Environment.NewLine}{options.GetUsage()}");
					}

					return;
				}

				Log.Information($"Starting Imaging Worker with options {args}");

				Log.Information("Creating bus");
				BusControl = Container.Resolve<IBusControl>();

				Log.Information("Starting bus");
				BusHandle = BusControl.Start();

				Console.WriteLine("Press any key to exit");
				Console.ReadLine();
			}
			catch (Exception ex1)
			{
				Log.Fatal(ex1, "Imaging Worker Error");
				Console.ReadLine();
			}
			finally
			{
				if (BusHandle != null)
				{
					try
					{
						Log.Information("Stopping bus");
						BusHandle.Stop();
					}
					catch (Exception ex2)
					{
						Log.Warning(ex2, "Error while stopping bus");
					}
				}
			}
		}
	}
}
