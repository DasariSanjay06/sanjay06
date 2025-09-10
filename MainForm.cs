using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;

namespace BarcodeDetector
{
    public partial class MainForm : Form
    {
        private Image? loadedImage;
        private PictureBox pictureBox = null!;
        private Button btnLoadImage = null!;
        private Button btnDetectBarcode = null!;
        private TextBox txtResult = null!;
        private Label lblStatus = null!;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Barcode Detector";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Picture Box to display the image
            pictureBox = new PictureBox
            {
                Location = new Point(12, 12),
                Size = new Size(760, 400),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray
            };

            // Load Image Button
            btnLoadImage = new Button
            {
                Location = new Point(12, 420),
                Size = new Size(120, 30),
                Text = "Load Image",
                UseVisualStyleBackColor = true
            };
            btnLoadImage.Click += loadImage_Click;

            // Detect Barcode Button
            btnDetectBarcode = new Button
            {
                Location = new Point(140, 420),
                Size = new Size(120, 30),
                Text = "Detect Barcode",
                UseVisualStyleBackColor = true
            };
            btnDetectBarcode.Click += barcodeDetection_Click;

            // Status Label
            lblStatus = new Label
            {
                Location = new Point(270, 425),
                Size = new Size(200, 25),
                Text = "Ready"
            };

            // Result TextBox
            txtResult = new TextBox
            {
                Location = new Point(12, 460),
                Size = new Size(760, 90),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10)
            };

            // Add controls to form
            this.Controls.Add(pictureBox);
            this.Controls.Add(btnLoadImage);
            this.Controls.Add(btnDetectBarcode);
            this.Controls.Add(lblStatus);
            this.Controls.Add(txtResult);
        }

        private void loadImage_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*";
                openFileDialog.Title = "Select an Image with Barcode";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        loadedImage = Image.FromFile(openFileDialog.FileName);
                        pictureBox.Image = loadedImage;
                        lblStatus.Text = "Image loaded successfully";
                        txtResult.Clear();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void barcodeDetection_Click(object? sender, EventArgs e)
        {
            if (loadedImage == null)
            {
                MessageBox.Show("Please load an image first!", "No Image", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                lblStatus.Text = "Detecting barcode...";
                Application.DoEvents();

                // Convert Image to Bitmap if necessary
                Bitmap bmp = loadedImage as Bitmap ?? new Bitmap(loadedImage);
                
                // Create luminance source for ZXing
                var luminanceSource = new BitmapLuminanceSource(bmp);

                // Try different decoding strategies
                var strategies = new Func<Result?>[]
                {
                    // Strategy 1: Direct bitmap decoding with multiple barcode formats
                    () =>
                    {
                        var reader = new ZXing.Windows.Compatibility.BarcodeReader
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
                        return reader.Decode(bmp);
                    },

                    // Strategy 2: With HybridBinarizer (better for complex backgrounds)
                    () =>
                    {
                        var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));
                        var reader = new MultiFormatReader();
                        
                        var hints = new Dictionary<DecodeHintType, object>
                        {
                            { DecodeHintType.TRY_HARDER, true },
                            { DecodeHintType.PURE_BARCODE, false }
                        };
                        
                        return reader.decode(binaryBitmap, hints);
                    },

                    // Strategy 3: With GlobalHistogramBinarizer (better for clean images)
                    () =>
                    {
                        var binaryBitmap = new BinaryBitmap(new GlobalHistogramBinarizer(luminanceSource));
                        var reader = new MultiFormatReader();
                        
                        var hints = new Dictionary<DecodeHintType, object>
                        {
                            { DecodeHintType.TRY_HARDER, true }
                        };
                        
                        return reader.decode(binaryBitmap, hints);
                    },

                    // Strategy 4: Try with image preprocessing (resize if too large)
                    () =>
                    {
                        Bitmap processedBmp = bmp;
                        
                        // If image is too large, resize it
                        if (bmp.Width > 2000 || bmp.Height > 2000)
                        {
                            int newWidth = bmp.Width > 2000 ? 2000 : bmp.Width;
                            int newHeight = bmp.Height > 2000 ? 2000 : bmp.Height;
                            
                            float ratio = Math.Min((float)newWidth / bmp.Width, (float)newHeight / bmp.Height);
                            newWidth = (int)(bmp.Width * ratio);
                            newHeight = (int)(bmp.Height * ratio);
                            
                            processedBmp = new Bitmap(bmp, newWidth, newHeight);
                        }
                        
                        var reader = new ZXing.Windows.Compatibility.BarcodeReader
                        {
                            AutoRotate = true,
                            Options = new DecodingOptions 
                            { 
                                TryHarder = true,
                                TryInverted = true,
                                CharacterSet = "UTF-8"
                            }
                        };
                        
                        var result = reader.Decode(processedBmp);
                        
                        if (processedBmp != bmp)
                            processedBmp.Dispose();
                            
                        return result;
                    }
                };

                Result? validResult = null;
                string strategyUsed = "";
                int strategyIndex = 0;

                foreach (var strategy in strategies)
                {
                    strategyIndex++;
                    try
                    {
                        validResult = strategy();
                        if (validResult != null)
                        {
                            strategyUsed = $"Strategy {strategyIndex}";
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue with next strategy
                        System.Diagnostics.Debug.WriteLine($"Strategy {strategyIndex} failed: {ex.Message}");
                        continue;
                    }
                }

                if (validResult != null)
                {
                    lblStatus.Text = "Barcode detected!";
                    
                    string resultText = $"=== BARCODE DETECTED ==={Environment.NewLine}";
                    resultText += $"Content: {validResult.Text}{Environment.NewLine}";
                    resultText += $"Format: {validResult.BarcodeFormat}{Environment.NewLine}";
                    resultText += $"Detection Method: {strategyUsed}{Environment.NewLine}";
                    
                    if (validResult.ResultPoints != null && validResult.ResultPoints.Length > 0)
                    {
                        resultText += $"Location Points: {validResult.ResultPoints.Length}{Environment.NewLine}";
                    }
                    
                    if (validResult.ResultMetadata != null && validResult.ResultMetadata.Count > 0)
                    {
                        resultText += $"{Environment.NewLine}Metadata:{Environment.NewLine}";
                        foreach (var metadata in validResult.ResultMetadata)
                        {
                            resultText += $"  {metadata.Key}: {metadata.Value}{Environment.NewLine}";
                        }
                    }
                    
                    txtResult.Text = resultText;
                    
                    MessageBox.Show($"Barcode Detected!{Environment.NewLine}{Environment.NewLine}" +
                                  $"Content: {validResult.Text}{Environment.NewLine}" +
                                  $"Format: {validResult.BarcodeFormat}", 
                                  "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    lblStatus.Text = "No barcode detected";
                    
                    string helpText = $"No barcode detected in the image.{Environment.NewLine}{Environment.NewLine}";
                    helpText += $"Image Details:{Environment.NewLine}";
                    helpText += $"  Size: {bmp.Width} x {bmp.Height} pixels{Environment.NewLine}";
                    helpText += $"  Format: {bmp.PixelFormat}{Environment.NewLine}{Environment.NewLine}";
                    helpText += $"Tips for better detection:{Environment.NewLine}";
                    helpText += $"  • Ensure the barcode is clear and not blurry{Environment.NewLine}";
                    helpText += $"  • Make sure there's good contrast{Environment.NewLine}";
                    helpText += $"  • Try to avoid shadows or reflections{Environment.NewLine}";
                    helpText += $"  • Center the barcode in the image{Environment.NewLine}";
                    helpText += $"  • Ensure the barcode is not too small or too large";
                    
                    txtResult.Text = helpText;
                    
                    MessageBox.Show("No barcode detected in the image.{Environment.NewLine}" +
                                  "Please try with a clearer image or different barcode.", 
                                  "No Barcode Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Clean up if we created a new bitmap
                if (bmp != loadedImage)
                {
                    bmp.Dispose();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error occurred";
                txtResult.Text = $"Error during barcode detection:{Environment.NewLine}{ex.Message}" +
                               $"{Environment.NewLine}{Environment.NewLine}Stack Trace:{Environment.NewLine}{ex.StackTrace}";
                
                MessageBox.Show($"An error occurred during barcode detection:{Environment.NewLine}{Environment.NewLine}" +
                              $"{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}