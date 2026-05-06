using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NavegadorComAbas
{
    public class ChromeTabControl : TabControl
    {
        public ChromeTabControl()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            DrawMode = TabDrawMode.OwnerDrawFixed;
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(220, 34);
            Padding = new Point(18, 6);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            if (e.Index < 0 || e.Index >= TabPages.Count)
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            TabPage tab = TabPages[e.Index];
            Rectangle r = GetTabRect(e.Index);

            r = new Rectangle(r.X + 2, r.Y + 4, r.Width - 4, r.Height - 6);

            bool selecionada = (SelectedIndex == e.Index);

            Color fundo = selecionada ? Color.White : Color.FromArgb(230, 230, 230);
            Color borda = Color.FromArgb(180, 180, 180);
            Color texto = Color.Black;

            using (GraphicsPath path = CreateTabPath(r, 12))
            using (SolidBrush brush = new SolidBrush(fundo))
            using (Pen pen = new Pen(borda))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            Rectangle closeRect = GetCloseRect(r);

            Rectangle textRect = new Rectangle(
                r.X + 12,
                r.Y + 8,
                r.Width - 34,
                r.Height - 12
            );

            TextRenderer.DrawText(
                g,
                tab.Text,
                Font,
                textRect,
                texto,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis
            );

            using (Font closeFont = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                TextRenderer.DrawText(
                    g,
                    "×",
                    closeFont,
                    closeRect,
                    Color.FromArgb(80, 80, 80),
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter
                );
            }
        }

        private GraphicsPath CreateTabPath(Rectangle r, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int d = radius * 2;

            path.StartFigure();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddLine(r.Right, r.Y + radius, r.Right, r.Bottom);
            path.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
            path.AddLine(r.X, r.Bottom, r.X, r.Y + radius);
            path.CloseFigure();

            return path;
        }

        public Rectangle GetCloseRect(int index)
        {
            Rectangle r = GetTabRect(index);
            r = new Rectangle(r.X + 2, r.Y + 4, r.Width - 4, r.Height - 6);
            return GetCloseRect(r);
        }

        private Rectangle GetCloseRect(Rectangle r)
        {
            return new Rectangle(r.Right - 24, r.Y + 8, 16, 16);
        }
    }
}