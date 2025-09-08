using System;
using System.Drawing;
using System.Windows.Forms;

public enum PictureBoxViewColorMode
{
    Raw,
    GrayScale,
    RGB
}

public class PictureBoxDrv : PictureBox
{
    private PictureBoxViewColorMode _colorMode = PictureBoxViewColorMode.Raw;
    private Size _defaultImageSize = new Size(752, 480);
    private float _scale = 1.0f;
    private float _minScale = 0.5f;
    private float _maxScale = 16.0f;
    private bool _stopImageUpdate = false;

    public PictureBoxDrv()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | 
                 ControlStyles.OptimizedDoubleBuffer | 
                 ControlStyles.ResizeRedraw | 
                 ControlStyles.UserPaint, true);
        SizeMode = PictureBoxSizeMode.Zoom;
        BackColor = Color.Black;
    }

    public PictureBoxViewColorMode ColorMode
    {
        get { return _colorMode; }
        set 
        { 
            _colorMode = value;
            ProcessImage();
            Invalidate();
        }
    }

    public Size DefaultImageSize
    {
        get { return _defaultImageSize; }
        set { _defaultImageSize = value; }
    }

    public float Scale
    {
        get { return _scale; }
        set 
        { 
            _scale = Math.Max(_minScale, Math.Min(_maxScale, value));
            UpdateImageScale();
        }
    }

    public float MinScale
    {
        get { return _minScale; }
        set { _minScale = value; }
    }

    public float MaxScale
    {
        get { return _maxScale; }
        set { _maxScale = value; }
    }

    public bool StopImageUpdate
    {
        get { return _stopImageUpdate; }
        set { _stopImageUpdate = value; }
    }

    public event EventHandler ImageChanged;

    protected override void OnPaint(PaintEventArgs pe)
    {
        if (!_stopImageUpdate)
        {
            base.OnPaint(pe);
        }
    }

    private void ProcessImage()
    {
        if (Image == null) return;

        switch (_colorMode)
        {
            case PictureBoxViewColorMode.GrayScale:
                // Convert to grayscale if needed
                break;
            case PictureBoxViewColorMode.RGB:
                // Process as RGB
                break;
            case PictureBoxViewColorMode.Raw:
            default:
                // Use raw image
                break;
        }
    }

    private void UpdateImageScale()
    {
        if (Image != null)
        {
            int newWidth = (int)(Image.Width * _scale);
            int newHeight = (int)(Image.Height * _scale);
            Size = new Size(newWidth, newHeight);
        }
    }

    protected virtual void OnImageChanged(EventArgs e)
    {
        ImageChanged?.Invoke(this, e);
    }
}