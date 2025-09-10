using System;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing;

namespace BarcodeDetector
{
    /// <summary>
    /// A luminance source class for converting System.Drawing.Bitmap to ZXing compatible format
    /// </summary>
    public class BitmapLuminanceSource : BaseLuminanceSource
    {
        public BitmapLuminanceSource(Bitmap bitmap)
            : base(bitmap.Width, bitmap.Height)
        {
            var height = bitmap.Height;
            var width = bitmap.Width;

            // Lock the bitmap's bits for faster access
            var rect = new Rectangle(0, 0, width, height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            try
            {
                var step = Math.Abs(data.Stride);
                var offset = step - width * 3;
                
                luminances = new byte[width * height];
                var luminanceIndex = 0;

                unsafe
                {
                    byte* ptr = (byte*)data.Scan0.ToPointer();

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            // Convert RGB to luminance using standard formula
                            // Y = 0.299R + 0.587G + 0.114B
                            byte b = ptr[0];
                            byte g = ptr[1];
                            byte r = ptr[2];
                            
                            luminances[luminanceIndex] = (byte)((r * 299 + g * 587 + b * 114) / 1000);
                            luminanceIndex++;
                            ptr += 3;
                        }
                        ptr += offset;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        protected BitmapLuminanceSource(byte[] luminanceArray, int width, int height)
            : base(width, height)
        {
            luminances = luminanceArray;
        }

        protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
        {
            return new BitmapLuminanceSource(newLuminances, width, height);
        }
    }
}