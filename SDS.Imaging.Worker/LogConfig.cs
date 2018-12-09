using Serilog;

namespace Sds.Imaging.Worker
{
	public static class LogConfig
	{
		public static void RegisterLogs()
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.LiterateConsole()
				.WriteTo.RollingFile("imaging-webapi-log-{Date}.log")
				.CreateLogger();
		}
	}
}
