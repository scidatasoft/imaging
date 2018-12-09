using MassTransit;
using Newtonsoft.Json;
using Sds.Imaging.Domain.Contracts;
using Sds.Imaging.Worker;
using Serilog;
using System.Threading.Tasks;

namespace StarterKit.Service.Consumer
{
	public class SourceFileUploadedMessageConsumer : IConsumer<SourceFileUploadedMessage>
	{
		private readonly IProcessor _processor;

		public SourceFileUploadedMessageConsumer(IProcessor processor)
		{
			_processor = processor;
		}

		public async Task Consume(ConsumeContext<SourceFileUploadedMessage> context)
		{
			Log.Information($"Received Message: {JsonConvert.SerializeObject(context.Message)}");

			_processor.ProcessImage(context.Message);
		}
	}
}
