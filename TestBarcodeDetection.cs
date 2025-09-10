using System;
using System.Drawing;
using System.IO;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace BarcodeDetector
{
    /// <summary>
    /// Console test application to demonstrate barcode detection functionality
    /// This can be run on Linux to test the core barcode detection logic
    /// </summary>
    public class TestBarcodeDetection
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Barcode Detection Test Application");
            Console.WriteLine("===================================");

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run --project TestBarcodeDetection.csproj <image-path>");
                Console.WriteLine("Example: dotnet run --project TestBarcodeDetection.csproj sample.png");
                return;
            }

            string imagePath = args[0];
            
            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"Error: File '{imagePath}' not found.");
                return;
            }

            try
            {
                Console.WriteLine($"\nLoading image: {imagePath}");
                using var bitmap = new Bitmap(imagePath);
                
                Console.WriteLine($"Image size: {bitmap.Width}x{bitmap.Height}");
                Console.WriteLine("\nTrying different detection strategies...\n");

                // Strategy 1: Direct bitmap decoding
                Console.WriteLine("Strategy 1: Direct bitmap decoding");
                var reader1 = new BarcodeReader
                {
                    AutoRotate = true,
                    Options = new DecodingOptions
                    {
                        TryHarder = true,
                        TryInverted = true,
                        PossibleFormats = new List<BarcodeFormat>
                        {
                            BarcodeFormat.QR_CODE,
                            BarcodeFormat.CODE_128,
                            BarcodeFormat.CODE_39,
                            BarcodeFormat.EAN_13,
                            BarcodeFormat.EAN_8,
                            BarcodeFormat.UPC_A,
                            BarcodeFormat.UPC_E,
                            BarcodeFormat.DATA_MATRIX,
                            BarcodeFormat.PDF_417,
                            BarcodeFormat.AZTEC,
                            BarcodeFormat.ITF
                        }
                    }
                };
                
                var result1 = reader1.Decode(bitmap);
                if (result1 != null)
                {
                    Console.WriteLine($"  ✓ Detected: {result1.Text}");
                    Console.WriteLine($"    Format: {result1.BarcodeFormat}");
                    return;
                }
                else
                {
                    Console.WriteLine("  ✗ No barcode detected");
                }

                // Strategy 2: With HybridBinarizer
                Console.WriteLine("\nStrategy 2: HybridBinarizer");
                var luminanceSource = new BitmapLuminanceSource(bitmap);
                var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));
                var reader2 = new MultiFormatReader();
                
                var hints = new Dictionary<DecodeHintType, object>
                {
                    { DecodeHintType.TRY_HARDER, true },
                    { DecodeHintType.PURE_BARCODE, false }
                };
                
                var result2 = reader2.decode(binaryBitmap, hints);
                if (result2 != null)
                {
                    Console.WriteLine($"  ✓ Detected: {result2.Text}");
                    Console.WriteLine($"    Format: {result2.BarcodeFormat}");
                    return;
                }
                else
                {
                    Console.WriteLine("  ✗ No barcode detected");
                }

                // Strategy 3: With GlobalHistogramBinarizer
                Console.WriteLine("\nStrategy 3: GlobalHistogramBinarizer");
                var binaryBitmap2 = new BinaryBitmap(new GlobalHistogramBinarizer(luminanceSource));
                var result3 = reader2.decode(binaryBitmap2, hints);
                
                if (result3 != null)
                {
                    Console.WriteLine($"  ✓ Detected: {result3.Text}");
                    Console.WriteLine($"    Format: {result3.BarcodeFormat}");
                }
                else
                {
                    Console.WriteLine("  ✗ No barcode detected");
                    Console.WriteLine("\n❌ No barcode could be detected in the image.");
                    Console.WriteLine("\nTips for better detection:");
                    Console.WriteLine("  • Ensure the barcode is clear and not blurry");
                    Console.WriteLine("  • Make sure there's good contrast");
                    Console.WriteLine("  • Avoid shadows or reflections");
                    Console.WriteLine("  • Center the barcode in the image");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}