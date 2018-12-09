using System;
using CommandLine;
using CommandLine.Text;

namespace Sds.Imaging.Worker
{
	// Define a class to receive parsed values
	public class ImagingOptions
	{
		[Option('q', "queue-name", Required = true, HelpText = "Queue name to read messages from.")]
		public string QueueName { get; set; }

		[Option('v', "verbose", DefaultValue = true, HelpText = "Prints all messages to standard output.")]
		public bool Verbose { get; set; }

		[Option('f', "image-format", DefaultValue = "png", HelpText = "Prints all messages to standard output.")]
		public string ImageFormat { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}
