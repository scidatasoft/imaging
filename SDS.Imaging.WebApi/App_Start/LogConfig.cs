using Serilog;
using System;
using System.IO;

namespace Sds.Imaging.WebApi
{
	public static class LogConfig
	{
		public static void RegisterLogs()
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.LiterateConsole()
				.WriteTo.RollingFile(
					Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imaging-webapi-log-{Date}.log"))
				.CreateLogger();
		}
	}
}
