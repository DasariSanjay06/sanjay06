using System;
using System.Drawing;
using System.Windows.Forms;

public class ThumbBarWithTextBox : UserControl
{
    private TrackBar trackBar;
    private TextBox textBox;
    private Label label;
    private int _minimumValue = 0;
    private int _maximumValue = 100;
    private int _value = 0;
    private string _labelText = "Value";
    private ContentAlignment _labelValueTextAlign = ContentAlignment.MiddleLeft;

    public event EventHandler UserModificationCompleted;

    public ThumbBarWithTextBox()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.trackBar = new TrackBar();
        this.textBox = new TextBox();
        this.label = new Label();
        
        this.SuspendLayout();
        
        // Label
        this.label.AutoSize = true;
        this.label.Location = new Point(3, 5);
        this.label.Name = "label";
        this.label.Size = new Size(60, 13);
        this.label.Text = _labelText;
        
        // TrackBar
        this.trackBar.Location = new Point(100, 0);
        this.trackBar.Name = "trackBar";
        this.trackBar.Size = new Size(400, 28);
        this.trackBar.TabIndex = 0;
        this.trackBar.TickStyle = TickStyle.None;
        this.trackBar.Minimum = _minimumValue;
        this.trackBar.Maximum = _maximumValue;
        this.trackBar.Value = _value;
        this.trackBar.ValueChanged += TrackBar_ValueChanged;
        this.trackBar.MouseUp += TrackBar_MouseUp;
        
        // TextBox
        this.textBox.Location = new Point(510, 2);
        this.textBox.Name = "textBox";
        this.textBox.Size = new Size(80, 20);
        this.textBox.TabIndex = 1;
        this.textBox.Text = _value.ToString();
        this.textBox.TextAlign = HorizontalAlignment.Center;
        this.textBox.KeyPress += TextBox_KeyPress;
        this.textBox.Leave += TextBox_Leave;
        
        // ThumbBarWithTextBox
        this.Controls.Add(this.label);
        this.Controls.Add(this.trackBar);
        this.Controls.Add(this.textBox);
        this.MinimumSize = new Size(600, 28);
        this.Size = new Size(600, 28);
        this.BackColor = Color.Transparent;
        
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    public string LabelText
    {
        get { return _labelText; }
        set 
        { 
            _labelText = value;
            if (label != null)
                label.Text = value;
        }
    }

    public ContentAlignment LabelValueTextAlign
    {
        get { return _labelValueTextAlign; }
        set 
        { 
            _labelValueTextAlign = value;
            if (label != null)
                label.TextAlign = value;
        }
    }

    public int MinimumValue
    {
        get { return _minimumValue; }
        set 
        { 
            _minimumValue = value;
            if (trackBar != null)
                trackBar.Minimum = value;
        }
    }

    public int MaximumValue
    {
        get { return _maximumValue; }
        set 
        { 
            _maximumValue = value;
            if (trackBar != null)
                trackBar.Maximum = value;
        }
    }

    public int Value
    {
        get { return _value; }
        set 
        { 
            _value = Math.Max(_minimumValue, Math.Min(_maximumValue, value));
            if (trackBar != null)
                trackBar.Value = _value;
            if (textBox != null)
                textBox.Text = _value.ToString();
        }
    }

    private void TrackBar_ValueChanged(object sender, EventArgs e)
    {
        _value = trackBar.Value;
        textBox.Text = _value.ToString();
    }

    private void TrackBar_MouseUp(object sender, MouseEventArgs e)
    {
        OnUserModificationCompleted();
    }

    private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Return)
        {
            UpdateValueFromTextBox();
            OnUserModificationCompleted();
            e.Handled = true;
        }
        else if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private void TextBox_Leave(object sender, EventArgs e)
    {
        UpdateValueFromTextBox();
        OnUserModificationCompleted();
    }

    private void UpdateValueFromTextBox()
    {
        if (int.TryParse(textBox.Text, out int newValue))
        {
            Value = newValue;
        }
        else
        {
            textBox.Text = _value.ToString();
        }
    }

    protected virtual void OnUserModificationCompleted()
    {
        UserModificationCompleted?.Invoke(this, EventArgs.Empty);
    }
}