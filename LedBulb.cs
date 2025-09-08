using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class LedBulb : Control
{
    private Color _color = Color.Green;
    private bool _on = false;
    private Color _reflectionColor = Color.FromArgb(180, 255, 255, 255);
    private Color[] _surroundColor = new Color[] { Color.FromArgb(0, 255, 255, 255) };

    public LedBulb()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | 
                 ControlStyles.OptimizedDoubleBuffer | 
                 ControlStyles.ResizeRedraw | 
                 ControlStyles.UserPaint, true);
        Size = new Size(32, 32);
    }

    public Color Color
    {
        get { return _color; }
        set 
        { 
            _color = value; 
            Invalidate();
        }
    }

    public bool On
    {
        get { return _on; }
        set 
        { 
            _on = value; 
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Calculate the drawing rectangle
        Rectangle rect = new Rectangle(1, 1, Width - 2, Height - 2);
        
        // Draw the LED bulb
        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddEllipse(rect);
            
            // Create the gradient brush for the LED
            using (PathGradientBrush pgb = new PathGradientBrush(path))
            {
                if (_on)
                {
                    // LED is ON - bright color
                    pgb.CenterColor = ControlPaint.Light(_color, 0.5f);
                    pgb.SurroundColors = new Color[] { _color };
                }
                else
                {
                    // LED is OFF - dark color
                    Color darkColor = ControlPaint.Dark(_color, 0.5f);
                    pgb.CenterColor = ControlPaint.Dark(darkColor, 0.3f);
                    pgb.SurroundColors = new Color[] { darkColor };
                }
                
                g.FillPath(pgb, path);
            }
            
            // Draw border
            using (Pen pen = new Pen(Color.FromArgb(100, 100, 100), 1))
            {
                g.DrawPath(pen, path);
            }
            
            // Draw reflection effect when ON
            if (_on)
            {
                Rectangle reflectRect = new Rectangle(rect.X + rect.Width / 4, 
                                                     rect.Y + rect.Height / 10,
                                                     rect.Width / 2, 
                                                     rect.Height / 3);
                
                using (GraphicsPath reflectPath = new GraphicsPath())
                {
                    reflectPath.AddEllipse(reflectRect);
                    
                    using (PathGradientBrush reflectBrush = new PathGradientBrush(reflectPath))
                    {
                        reflectBrush.CenterColor = Color.FromArgb(100, 255, 255, 255);
                        reflectBrush.SurroundColors = new Color[] { Color.FromArgb(0, 255, 255, 255) };
                        g.FillPath(reflectBrush, reflectPath);
                    }
                }
            }
        }
    }
}