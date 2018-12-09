﻿using System;
using System.Drawing;
using System.IO;

namespace Sds.Imaging.Processing
{
	internal class StructureRasterizer : IFileRasterizer
	{
		public Image Rasterize(Stream data, string type)
		{
			using (StreamReader dataReader = new StreamReader(data))
			{
				var imageBytes = new IndigoAdapter().Mol2Image(dataReader.ReadToEnd());
				using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
				{
					Image image = Image.FromStream(ms, true);
					return image;
				}
			}
		}
	}
}
