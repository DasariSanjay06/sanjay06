using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

// This file shows the CORRECTED version of your barcode detection method
// The main issues with your original code were:
// 1. RGBLuminanceSource constructor was being used incorrectly
// 2. You should use BitmapLuminanceSource for Windows Forms
// 3. Missing proper BarcodeFormat specifications

public class FixedBarcodeDetection
{
    private Image loadedImage;

    // YOUR ORIGINAL CODE (PROBLEMATIC):
    /*
    private void barcodeDetection_Click_ORIGINAL(object sender, EventArgs e)
    {
        if (loadedImage == null)
        {
            MessageBox.Show("Load an image first!");
            return;
        }

        // Convert image to grayscale byte array
        Bitmap bmp = new Bitmap(loadedImage);
        int width = bmp.Width;
        int height = bmp.Height;
        byte[] luminances = new byte[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color c = bmp.GetPixel(x, y);
                luminances[y * width + x] = (byte)((c.R + c.G + c.B) / 3);
            }
        }

        // PROBLEM: RGBLuminanceSource doesn't work this way!
        var source = new RGBLuminanceSource(luminances, width, height, RGBLuminanceSource.BitmapFormat.Gray8);
        var binaryBitmap = new BinaryBitmap(new HybridBinarizer(source));

        var reader = new BarcodeReader
        {
            AutoRotate = true,
            TryInverted = true,
            Options = new DecodingOptions { TryHarder = true }
        };

        var result = reader.Decode(binaryBitmap);

        if (result != null)
            MessageBox.Show("Detected: " + result.Text);
        else
            MessageBox.Show("No barcode/QR found.");
    }
    */

    // CORRECTED VERSION - METHOD 1 (SIMPLEST AND MOST RELIABLE):
    private void barcodeDetection_Click_FIXED_Simple(object sender, EventArgs e)
    {
        if (loadedImage == null)
        {
            MessageBox.Show("Load an image first!");
            return;
        }

        // Simply use BarcodeReader directly with the bitmap
        var reader = new BarcodeReader
        {
            AutoRotate = true,
            TryInverted = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                // Specify which barcode formats to look for
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
                    BarcodeFormat.PDF_417
                }
            }
        };

        // Convert to Bitmap and decode directly
        Bitmap bitmap = loadedImage as Bitmap ?? new Bitmap(loadedImage);
        var result = reader.Decode(bitmap);

        if (result != null)
            MessageBox.Show($"Detected ({result.BarcodeFormat}): {result.Text}");
        else
            MessageBox.Show("No barcode/QR found.");
    }

    // CORRECTED VERSION - METHOD 2 (IF YOU NEED MANUAL LUMINANCE):
    private void barcodeDetection_Click_FIXED_Manual(object sender, EventArgs e)
    {
        if (loadedImage == null)
        {
            MessageBox.Show("Load an image first!");
            return;
        }

        Bitmap bitmap = loadedImage as Bitmap ?? new Bitmap(loadedImage);

        // Use BitmapLuminanceSource instead of RGBLuminanceSource
        var luminanceSource = new BitmapLuminanceSource(bitmap);
        var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));

        // Use MultiFormatReader for better compatibility
        var reader = new MultiFormatReader();
        var hints = new Dictionary<DecodeHintType, object>
        {
            { DecodeHintType.TRY_HARDER, true },
            { DecodeHintType.CHARACTER_SET, "UTF-8" }
        };

        try
        {
            var result = reader.decode(binaryBitmap, hints);
            if (result != null)
                MessageBox.Show($"Detected: {result.Text}");
            else
                MessageBox.Show("No barcode/QR found.");
        }
        catch (NotFoundException)
        {
            MessageBox.Show("No barcode/QR found.");
        }
    }

    // CORRECTED VERSION - METHOD 3 (WITH PROPER RGB LUMINANCE IF NEEDED):
    private void barcodeDetection_Click_FIXED_RGB(object sender, EventArgs e)
    {
        if (loadedImage == null)
        {
            MessageBox.Show("Load an image first!");
            return;
        }

        Bitmap bitmap = loadedImage as Bitmap ?? new Bitmap(loadedImage);
        int width = bitmap.Width;
        int height = bitmap.Height;

        // If you really need to use RGBLuminanceSource, you need to provide RGB data correctly
        byte[] rgbBytes = new byte[width * height * 3]; // Note: 3 bytes per pixel for RGB

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                int offset = (y * width + x) * 3;
                rgbBytes[offset] = pixel.R;
                rgbBytes[offset + 1] = pixel.G;
                rgbBytes[offset + 2] = pixel.B;
            }
        }

        // Now use RGBLuminanceSource with proper RGB data
        var source = new RGBLuminanceSource(rgbBytes, width, height, RGBLuminanceSource.BitmapFormat.RGB24);
        var binaryBitmap = new BinaryBitmap(new HybridBinarizer(source));

        var reader = new BarcodeReader
        {
            AutoRotate = true,
            TryInverted = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new List<BarcodeFormat>
                {
                    BarcodeFormat.QR_CODE,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.CODE_39,
                    BarcodeFormat.EAN_13,
                    BarcodeFormat.DATA_MATRIX
                }
            }
        };

        var result = reader.Decode(binaryBitmap);

        if (result != null)
            MessageBox.Show($"Detected: {result.Text}");
        else
            MessageBox.Show("No barcode/QR found.");
    }
}