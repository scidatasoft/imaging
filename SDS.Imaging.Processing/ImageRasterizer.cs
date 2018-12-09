using System;
using System.Drawing;
using System.IO;

namespace Sds.Imaging.Processing
{
	internal class ImageRasterizer : IFileRasterizer
	{
		public Image Rasterize(Stream data, string type)
		{
			var width = 96;
			var height = 96;
			var image = Image.FromStream(data, false).Scale(width, height);
			return image;
		}
	}
}
