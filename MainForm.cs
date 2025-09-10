using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ModernCameraApp
{
    public partial class MainForm : Form
    {
        private Timer fpsTimer;
        private int frameCount = 0;
        private DateTime lastFpsUpdate = DateTime.Now;
        
        public MainForm()
        {
            InitializeComponent();
            SetupModernUI();
            InitializeFpsTimer();
        }

        private void SetupModernUI()
        {
            // Form settings
            this.BackColor = Color.FromArgb(240, 242, 247);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Add drag functionality to the form
            this.MouseDown += Form_MouseDown;
            
            // Set default combo box selection
            comboBox1.SelectedIndex = 0;
            
            // Apply modern styling to all controls
            ApplyModernButtonStyle(btnConnect, Color.FromArgb(46, 204, 113), Color.White);
            ApplyModernButtonStyle(btnDisConnect, Color.FromArgb(231, 76, 60), Color.White);
            ApplyModernButtonStyle(btnTest, Color.FromArgb(52, 152, 219), Color.White);
            ApplyModernButtonStyle(btnCapture, Color.FromArgb(155, 89, 182), Color.White);
            ApplyModernButtonStyle(btnClose, Color.FromArgb(52, 73, 94), Color.White);
            ApplyModernButtonStyle(btnMinimize, Color.FromArgb(149, 165, 166), Color.White);
            
            // Style the picture box
            pictureBox1.BackColor = Color.FromArgb(44, 62, 80);
            pictureBox1.BorderStyle = BorderStyle.None;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            
            // Add shadow effect to picture box
            Panel shadowPanel = new Panel();
            shadowPanel.Size = new Size(pictureBox1.Width + 10, pictureBox1.Height + 10);
            shadowPanel.Location = new Point(pictureBox1.Location.X - 5, pictureBox1.Location.Y - 5);
            shadowPanel.BackColor = Color.FromArgb(30, 0, 0, 0);
            this.Controls.Add(shadowPanel);
            shadowPanel.SendToBack();
            
            // Style the combo box
            comboBox1.FlatStyle = FlatStyle.Flat;
            comboBox1.BackColor = Color.White;
            comboBox1.ForeColor = Color.FromArgb(44, 62, 80);
            comboBox1.Font = new Font("Segoe UI", 9F);
            
            // Style labels
            label1.ForeColor = Color.FromArgb(52, 73, 94);
            label1.Font = new Font("Segoe UI Semibold", 9F);
            
            label2.ForeColor = Color.FromArgb(52, 73, 94);
            label2.Font = new Font("Segoe UI Semibold", 9F);
            
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            
            // Style FPS label
            lblFps.BackColor = Color.FromArgb(44, 62, 80);
            lblFps.ForeColor = Color.FromArgb(46, 204, 113);
            lblFps.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblFps.BorderStyle = BorderStyle.None;
            
            // Add rounded corners to form
            this.Region = CreateRoundedRectangle(this.ClientRectangle, 20);
        }

        private void ApplyModernButtonStyle(Button button, Color backgroundColor, Color foreColor)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = backgroundColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            
            // Add hover effects
            button.MouseEnter += (s, e) => {
                Button btn = s as Button;
                btn.BackColor = ControlPaint.Light(backgroundColor, 0.1f);
            };
            
            button.MouseLeave += (s, e) => {
                Button btn = s as Button;
                btn.BackColor = backgroundColor;
            };
            
            // Add rounded corners
            button.Paint += (s, e) => {
                Button btn = s as Button;
                GraphicsPath path = new GraphicsPath();
                path.AddRoundedRectangle(btn.ClientRectangle, 8);
                btn.Region = new Region(path);
            };
        }

        private Region CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddRoundedRectangle(rect, radius);
            return new Region(path);
        }

        private void InitializeFpsTimer()
        {
            fpsTimer = new Timer();
            fpsTimer.Interval = 100; // Update every 100ms
            fpsTimer.Tick += FpsTimer_Tick;
        }

        private void FpsTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - lastFpsUpdate;
            if (elapsed.TotalSeconds >= 1)
            {
                double fps = frameCount / elapsed.TotalSeconds;
                lblFps.Text = fps.ToString("F1");
                frameCount = 0;
                lastFpsUpdate = DateTime.Now;
            }
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        // Import for window dragging
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        // Event handlers
        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Start camera connection
            ShowNotification("Camera connected successfully!", Color.FromArgb(46, 204, 113));
            fpsTimer.Start();
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            // Disconnect camera
            ShowNotification("Camera disconnected", Color.FromArgb(231, 76, 60));
            fpsTimer.Stop();
            lblFps.Text = "0.0";
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            // Test functionality
            ShowNotification("Test completed successfully", Color.FromArgb(52, 152, 219));
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            // Capture image
            string format = comboBox1.SelectedItem?.ToString() ?? "JPEG";
            ShowNotification($"Image captured as {format}", Color.FromArgb(155, 89, 182));
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void ShowNotification(string message, Color color)
        {
            Label notification = new Label();
            notification.Text = message;
            notification.BackColor = color;
            notification.ForeColor = Color.White;
            notification.Font = new Font("Segoe UI", 10F);
            notification.AutoSize = false;
            notification.Size = new Size(300, 40);
            notification.TextAlign = ContentAlignment.MiddleCenter;
            notification.Location = new Point((this.Width - 300) / 2, 10);
            
            this.Controls.Add(notification);
            notification.BringToFront();
            
            Timer timer = new Timer();
            timer.Interval = 3000;
            timer.Tick += (s, e) => {
                this.Controls.Remove(notification);
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }
    }

    // Extension class for rounded rectangles
    public static class GraphicsExtension
    {
        public static void AddRoundedRectangle(this GraphicsPath path, Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(rect.Location, size);

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
        }
    }
}