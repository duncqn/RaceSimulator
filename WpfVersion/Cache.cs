using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace WpfVersion
{
    public static class Cache
    {
        internal static Dictionary<string, Bitmap> IMGCache = new();

        public static void Initialize()
        {
            IMGCache = new Dictionary<string, Bitmap>();
        }
        public static Bitmap GetImageData(string url, int width, int height)
        {
            if (url.Equals("Empty"))
                return new Bitmap(url);
            if (!IMGCache.ContainsKey(url))
                IMGCache.Add(url, (Bitmap)new Bitmap(Image.FromFile(url)).Clone());
            return AdjustSize(IMGCache[url], width, height);
        }
        public static Bitmap AdjustSize(Bitmap original, int width, int height)
        {
            return new Bitmap(original, new Size(width, height));
        }

        public static void ClearCache()
        {
            IMGCache.Clear();
        }

        public static Bitmap GenerateBitmap(int width, int height)
        {
            if (!IMGCache.ContainsKey("empty"))
            {
                Bitmap bmp = new(width, height);
                IMGCache.Add("empty", bmp);
            }

            return IMGCache["empty"].Clone() as Bitmap;
        }

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}