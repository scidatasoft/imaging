using Serilog;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace Sds.Imaging.WebApi.Attributes
{
	public class DefaultExceptionFilterAttribute : ExceptionFilterAttribute
	{
		public override void OnException(HttpActionExecutedContext context)
		{
			Log.Warning(context.Exception, "Action controller caught exception.");
			context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
		}
	}
}