using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace NV22SpectralInteg
{
    public class RoundedButton : Button
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius { get; set; } = 20;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.White;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderThickness { get; set; } = 0;

        public RoundedButton()
        {
            FlatStyle = FlatStyle.Flat;

            // ✨ Fixes for unwanted glow/focus/highlight
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseDownBackColor = Color.Black;
            FlatAppearance.MouseOverBackColor = Color.Black;
            FlatAppearance.CheckedBackColor = Color.Black;

            BackColor = Color.Black;
            ForeColor = Color.White;
            TabStop = false;
            CausesValidation = false; // Prevent focus visual when clicked

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable, false); // Disables selection style
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle surfaceRect = ClientRectangle;
            Rectangle borderRect = Rectangle.Inflate(surfaceRect, -BorderThickness, -BorderThickness);
            GraphicsPath surfacePath = GetRoundedPath(surfaceRect, BorderRadius);

            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, surfacePath);
            }

            if (BorderThickness > 0)
            {
                using (Pen pen = new Pen(BorderColor, BorderThickness))
                {
                    GraphicsPath borderPath = GetRoundedPath(borderRect, BorderRadius);
                    e.Graphics.DrawPath(pen, borderPath);
                }
            }

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                surfaceRect,
                ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );

            this.Region = new Region(surfacePath);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            if (radius > 0)
            {
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();
            }
            else
            {
                path.AddRectangle(rect);
            }

            return path;
        }

        protected override bool ShowFocusCues => false; // Fully disables dotted focus border
    }
}
