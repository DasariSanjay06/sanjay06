using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace BarcodeDetector
{
    public partial class BarcodeDetectorForm : Form
    {
        private Image loadedImage;
        private PictureBox pictureBox;
        private Button btnLoadImage;
        private Button btnDetectBarcode;
        private TextBox txtResult;
        private Label lblResult;

        public BarcodeDetectorForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Barcode/QR Code Detector";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Picture Box
            pictureBox = new PictureBox
            {
                Location = new Point(12, 12),
                Size = new Size(760, 400),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            this.Controls.Add(pictureBox);

            // Load Image Button
            btnLoadImage = new Button
            {
                Text = "Load Image",
                Location = new Point(12, 420),
                Size = new Size(100, 30)
            };
            btnLoadImage.Click += LoadImage_Click;
            this.Controls.Add(btnLoadImage);

            // Detect Barcode Button
            btnDetectBarcode = new Button
            {
                Text = "Detect Barcode",
                Location = new Point(120, 420),
                Size = new Size(120, 30)
            };
            btnDetectBarcode.Click += BarcodeDetection_Click;
            this.Controls.Add(btnDetectBarcode);

            // Result Label
            lblResult = new Label
            {
                Text = "Result:",
                Location = new Point(12, 460),
                Size = new Size(50, 20)
            };
            this.Controls.Add(lblResult);

            // Result TextBox
            txtResult = new TextBox
            {
                Location = new Point(70, 460),
                Size = new Size(700, 80),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };
            this.Controls.Add(txtResult);
        }

        private void LoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Title = "Select an image with barcode/QR code";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    loadedImage = Image.FromFile(openFileDialog.FileName);
                    pictureBox.Image = loadedImage;
                    txtResult.Clear();
                }
            }
        }

        // CORRECTED BARCODE DETECTION METHOD
        private void BarcodeDetection_Click(object sender, EventArgs e)
        {
            if (loadedImage == null)
            {
                MessageBox.Show("Please load an image first!", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Method 1: Using BarcodeReader directly (RECOMMENDED)
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
                            BarcodeFormat.EAN_8,
                            BarcodeFormat.UPC_A,
                            BarcodeFormat.UPC_E,
                            BarcodeFormat.DATA_MATRIX,
                            BarcodeFormat.PDF_417,
                            BarcodeFormat.AZTEC
                        }
                    }
                };

                // Convert to Bitmap if needed
                Bitmap bitmap = loadedImage as Bitmap ?? new Bitmap(loadedImage);
                
                // Try direct decoding first
                var result = reader.Decode(bitmap);

                if (result != null)
                {
                    string resultText = $"Format: {result.BarcodeFormat}\r\n" +
                                      $"Content: {result.Text}\r\n" +
                                      $"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    txtResult.Text = resultText;
                    MessageBox.Show($"Barcode Detected!\r\n\r\n{resultText}", "Success", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Method 2: Try with manual luminance source (fallback)
                    result = DecodeWithManualLuminance(bitmap);
                    
                    if (result != null)
                    {
                        string resultText = $"Format: {result.BarcodeFormat}\r\n" +
                                          $"Content: {result.Text}\r\n" +
                                          $"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
                                          $"(Detected using manual luminance)";
                        txtResult.Text = resultText;
                        MessageBox.Show($"Barcode Detected!\r\n\r\n{resultText}", "Success", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        txtResult.Text = "No barcode or QR code found in the image.";
                        MessageBox.Show("No barcode or QR code could be detected in the image.\r\n\r\n" +
                                      "Tips:\r\n" +
                                      "- Ensure the barcode is clear and not blurry\r\n" +
                                      "- Try different lighting conditions\r\n" +
                                      "- Make sure the barcode is not damaged",
                                      "No Barcode Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Error: {ex.Message}";
                MessageBox.Show($"An error occurred while detecting barcode:\r\n{ex.Message}", 
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Alternative method using manual luminance conversion
        private Result DecodeWithManualLuminance(Bitmap bitmap)
        {
            try
            {
                int width = bitmap.Width;
                int height = bitmap.Height;

                // Lock bits for faster pixel access
                BitmapData bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                try
                {
                    byte[] pixels = new byte[width * height * 4];
                    System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);

                    // Create luminance source using the correct constructor
                    var luminanceSource = new BitmapLuminanceSource(bitmap);
                    
                    // Create binary bitmap
                    var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));

                    // Create reader with multiple options
                    var reader = new MultiFormatReader();
                    var hints = new Dictionary<DecodeHintType, object>
                    {
                        { DecodeHintType.TRY_HARDER, true },
                        { DecodeHintType.PURE_BARCODE, false },
                        { DecodeHintType.CHARACTER_SET, "UTF-8" }
                    };

                    // Try to decode
                    return reader.decode(binaryBitmap, hints);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
            catch
            {
                // If manual method fails, return null
                return null;
            }
        }

        // Additional helper method for preprocessing image
        private Bitmap PreprocessImage(Bitmap original)
        {
            // Create a copy for processing
            Bitmap processed = new Bitmap(original.Width, original.Height);

            // Convert to grayscale with better algorithm
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color pixel = original.GetPixel(x, y);
                    // Use weighted grayscale conversion (ITU-R BT.709)
                    int gray = (int)(pixel.R * 0.2126 + pixel.G * 0.7152 + pixel.B * 0.0722);
                    processed.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }

            return processed;
        }
    }

    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BarcodeDetectorForm());
        }
    }
}