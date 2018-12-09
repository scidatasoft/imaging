using System;
using System.Net;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace Sds.Imaging.WebApi.Attributes
{
	public class CacheControlAttribute : ActionFilterAttribute
	{
		public int MaxAge { get; set; }

		public CacheControlAttribute(int age = 604800)// 604800 week
		{
			MaxAge = age;
		}

		public override void OnActionExecuted(HttpActionExecutedContext context)
		{
			if (context.Response != null && context.Response.StatusCode == HttpStatusCode.OK)
			{
				context.Response.Headers.CacheControl = new CacheControlHeaderValue()
				{
					Public = true,
					MaxAge = TimeSpan.FromSeconds(MaxAge)
				};
			}

			base.OnActionExecuted(context);
		}
	}
}
