# Barcode Detector Application

A complete barcode detection solution with both Windows Forms GUI and console test application using ZXing.Net library.

## Features

- Load images from file system
- Detect multiple barcode formats including:
  - QR Code
  - Code 128
  - Code 39
  - EAN-13 / EAN-8
  - UPC-A / UPC-E
  - Data Matrix
  - PDF-417
  - Aztec
  - ITF
- Multiple detection strategies for improved accuracy
- User-friendly interface with visual feedback
- Detailed detection results display

## Prerequisites

- .NET 6.0 SDK or later
- Windows OS (for Windows Forms support)

## Building the Application

1. Clone or download the project files
2. Open a terminal in the project directory
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
4. Build the project:
   ```bash
   dotnet build
   ```

## Running the Application

Run the application using:
```bash
dotnet run
```

Or build and run the executable:
```bash
dotnet build -c Release
./bin/Release/net6.0-windows/BarcodeDetector.exe
```

## Usage

1. **Load an Image**: Click the "Load Image" button and select an image file containing a barcode
2. **Detect Barcode**: Click the "Detect Barcode" button to scan the image for barcodes
3. **View Results**: The detected barcode content and format will be displayed in the result area

## Detection Strategies

The application uses multiple detection strategies to improve accuracy:

1. **Direct Bitmap Decoding**: Uses ZXing's BarcodeReader with multiple format support
2. **HybridBinarizer**: Better for images with complex backgrounds
3. **GlobalHistogramBinarizer**: Better for clean, high-contrast images
4. **Preprocessed Detection**: Resizes large images for better performance

## Tips for Better Detection

- Use clear, high-quality images
- Ensure good contrast between barcode and background
- Avoid shadows, reflections, or glare on the barcode
- Center the barcode in the image
- Ensure the barcode is neither too small nor too large in the frame

## Projects Included

1. **BarcodeDetector** - Windows Forms GUI application (Windows only)
2. **TestBarcodeDetection** - Console test application (cross-platform)

## Key Fixes Applied to Your Original Code

The original code had several critical issues that have been fixed:

1. **BitmapLuminanceSource Implementation**: Created a proper implementation that converts System.Drawing.Bitmap to ZXing-compatible luminance data with unsafe code for performance
2. **BarcodeReader Type Fix**: Fixed the generic type issue by using `ZXing.Windows.Compatibility.BarcodeReader` instead of the generic `BarcodeReader<T>`
3. **BinaryBitmap Usage**: Fixed the incorrect decoder usage - now properly passes BinaryBitmap to MultiFormatReader.decode() method
4. **TryInverted Property**: Moved the deprecated `TryInverted` property from BarcodeReader to DecodingOptions
5. **Multiple Detection Strategies**: Enhanced with four different strategies for better detection rates
6. **Nullable Reference Types**: Fixed nullable warnings by properly annotating fields and event handlers
7. **Error Handling**: Improved error handling with detailed feedback and strategy tracking
8. **Format Support**: Added support for multiple barcode formats (QR Code, Code 128, Code 39, EAN, UPC, Data Matrix, PDF-417, Aztec, ITF)
9. **Project Configuration**: Added necessary project settings including `EnableWindowsTargeting` and `AllowUnsafeBlocks`

## Dependencies

- ZXing.Net (v0.16.9) - Core barcode detection library
- ZXing.Net.Bindings.Windows.Compatibility (v0.16.12) - Windows compatibility layer

## Troubleshooting

If barcode detection fails:
1. Ensure the image is clear and not blurry
2. Try different lighting conditions when capturing the image
3. Adjust the distance from the barcode
4. Make sure the entire barcode is visible in the image
5. Try different image formats (JPG, PNG, BMP)