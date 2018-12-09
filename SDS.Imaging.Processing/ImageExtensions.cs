using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Sds.Imaging.Processing
{
	/// <summary>
	/// Extensions methods to work with images
	/// </summary>
	public static class ImageExtensions
	{
		/// <summary>
		/// Converts System.Drawing.Image to byte array that contains specific image format, such as PNG, JPG etc.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="format"></param>
		/// <returns>converted System.Drawing.Image to byte array that contains specific image format, such as PNG, JPG etc.</returns>
		public static byte[] Convert(this Image image, ImageFormat format)
		{
			byte[] result = null;
			using (MemoryStream stream = new MemoryStream())
			{
				image.Save(stream, format);
				result = stream.ToArray();
			}
			return result;
		}

		/// <summary>
		/// Returns resized image
		/// </summary>
		/// <param name="image"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static Image Scale(this Image image, int width, int height)
		{
			var ratioX = (double)width / image.Width;
			var ratioY = (double)height / image.Height;
			var ratio = Math.Min(ratioX, ratioY);

			var newWidth = (int)(image.Width * ratio);
			var newHeight = (int)(image.Height * ratio);

			var newImage = new Bitmap(newWidth, newHeight);

			using (var graphics = Graphics.FromImage(newImage))
				graphics.DrawImage(image, 0, 0, newWidth, newHeight);

			return newImage;
		}

		public static ImageFormat ParseImageFormat(this string format, string defaultFormat = "png")
		{
			if (string.IsNullOrWhiteSpace(format))
			{
				throw new ArgumentException("Empty format passed");
			}

			switch (format.ToLower().Trim())
			{
				case "bmp": return ImageFormat.Bmp;
				case "emf": return ImageFormat.Emf;
				case "exif": return ImageFormat.Exif;
				case "gif": return ImageFormat.Gif;
				case "ico": return ImageFormat.Icon;
				case "icon": return ImageFormat.Icon;
				case "jpeg": return ImageFormat.Jpeg;
				case "jpg": return ImageFormat.Jpeg;
				case "png": return ImageFormat.Png;
				case "tiff": return ImageFormat.Tiff;
				case "wmf": return ImageFormat.Wmf;
				default: return ImageFormat.Png;
			}
		}
	}
}
