using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

public class WaitingFormManager
{
    private Form _parentForm;
    private Form _waitingForm;
    private Thread _waitingThread;
    private string _message = "Please wait...";
    private bool _isShowing = false;

    public WaitingFormManager(Form parentForm)
    {
        _parentForm = parentForm;
    }

    public void Show(string message = "Please wait...")
    {
        if (_isShowing) return;
        
        _message = message;
        _isShowing = true;
        
        _waitingThread = new Thread(() =>
        {
            _waitingForm = new WaitingForm(_message);
            _waitingForm.StartPosition = FormStartPosition.CenterParent;
            Application.Run(_waitingForm);
        });
        
        _waitingThread.SetApartmentState(ApartmentState.STA);
        _waitingThread.IsBackground = true;
        _waitingThread.Start();
        
        Thread.Sleep(100); // Give it time to show
    }

    public void Hide()
    {
        if (!_isShowing) return;
        
        _isShowing = false;
        
        if (_waitingForm != null)
        {
            if (_waitingForm.InvokeRequired)
            {
                _waitingForm.Invoke(new Action(() => _waitingForm.Close()));
            }
            else
            {
                _waitingForm.Close();
            }
        }
        
        _waitingForm = null;
        _waitingThread = null;
    }

    private class WaitingForm : Form
    {
        private ProgressBar progressBar;
        private Label messageLabel;

        public WaitingForm(string message)
        {
            InitializeComponent(message);
        }

        private void InitializeComponent(string message)
        {
            this.progressBar = new ProgressBar();
            this.messageLabel = new Label();
            
            this.SuspendLayout();
            
            // messageLabel
            this.messageLabel.AutoSize = false;
            this.messageLabel.Location = new Point(12, 20);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new Size(360, 30);
            this.messageLabel.Text = message;
            this.messageLabel.TextAlign = ContentAlignment.MiddleCenter;
            
            // progressBar
            this.progressBar.Location = new Point(12, 60);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(360, 23);
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 30;
            
            // WaitingForm
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(384, 100);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.messageLabel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WaitingForm";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Processing...";
            this.TopMost = true;
            
            this.ResumeLayout(false);
        }
    }
}