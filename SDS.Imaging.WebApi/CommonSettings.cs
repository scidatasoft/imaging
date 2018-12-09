using System;
using System.Configuration;
using System.Drawing;

namespace Sds.Imaging.WebApi
{
	public class CommonSettings
	{
		public TimeSpan FileExpiry { get; private set; }
		public Size ImageSize { get; private set; }

		public CommonSettings()
		{
			var fileExpirySec = long.Parse(ConfigurationManager.AppSettings["FileLifeTime"]);
			FileExpiry = TimeSpan.FromSeconds(fileExpirySec);

			var imageSizeWidth = int.Parse(ConfigurationManager.AppSettings["ImageSizeWidth"]);
			var imageSizeHeigth = int.Parse(ConfigurationManager.AppSettings["ImageSizeHeigth"]);
			ImageSize = new Size(imageSizeWidth, imageSizeHeigth);
		}
	}
}