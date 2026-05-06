using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NavegadorComAbas
{
    public class ChromeTabControl : TabControl
    {
        private int hoverIndex = -1;
        private int hoverCloseIndex = -1;

        public ChromeTabControl()
        {
            DrawMode = TabDrawMode.OwnerDrawFixed;
            SizeMode = TabSizeMode.Fixed;
            Appearance = TabAppearance.Normal;
            ItemSize = new Size(220, 34);
            Padding = new Point(18, 6);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            this.DoubleBuffered = true;
            MouseMove += ChromeTabControl_MouseMove;
            MouseLeave += ChromeTabControl_MouseLeave;
        }
        private void ChromeTabControl_MouseMove(object sender, MouseEventArgs e)
        {
            int novoHoverIndex = -1;
            int novoHoverCloseIndex = -1;

            for (int i = 0; i < TabPages.Count; i++)
            {
                Rectangle tabRect = AjustarRect(GetTabRect(i));

                if (tabRect.Contains(e.Location))
                {
                    novoHoverIndex = i;

                    if (GetCloseRect(i).Contains(e.Location))
                        novoHoverCloseIndex = i;

                    break;
                }
            }

            if (hoverIndex != novoHoverIndex || hoverCloseIndex != novoHoverCloseIndex)
            {
                hoverIndex = novoHoverIndex;
                hoverCloseIndex = novoHoverCloseIndex;
                this.Refresh();
            }
        }
        private void ChromeTabControl_MouseLeave(object sender, EventArgs e)
        {
            hoverIndex = -1;
            hoverCloseIndex = -1;
            this.Refresh();
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= TabPages.Count)
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // limpa o retângulo da aba com a cor do topo
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(248, 249, 250)))
            {
                g.FillRectangle(bgBrush, e.Bounds);
            }

            TabPage tab = TabPages[e.Index];
            Rectangle r = AjustarRect(GetTabRect(e.Index));
            bool abaAdicionar = EhAbaAdicionar(tab);

            if (abaAdicionar)
            {
                DesenharAbaAdicionar(g, r, hoverIndex == e.Index);
                return;
            }

            bool selecionada = (SelectedIndex == e.Index);
            bool hover = (hoverIndex == e.Index);
            bool hoverClose = (hoverCloseIndex == e.Index);

            // Visual flat estilo Chrome
            Color fundo;
            if (selecionada)
                fundo = Color.FromArgb(248, 249, 250);   // quase branco
            else if (hover)
                fundo = Color.FromArgb(241, 243, 244);   // hover leve
            else
                fundo = Color.FromArgb(232, 234, 237);   // abas inativas

            Color borda = Color.FromArgb(210, 214, 220); // borda leve
            Color texto = Color.FromArgb(32, 33, 36);    // preto suave

            using (GraphicsPath path = CreateTabPath(r, 10))
            using (SolidBrush brush = new SolidBrush(fundo))
            {
                g.FillPath(brush, path);

                if (!selecionada)
                {
                    using (Pen pen = new Pen(borda))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            Rectangle closeRect = GetCloseRect(e.Index);

            if (hoverClose)
            {
                using (SolidBrush closeHoverBrush = new SolidBrush(Color.FromArgb(218, 220, 224)))
                {
                    g.FillEllipse(closeHoverBrush, closeRect);
                }
            }

            Rectangle textRect = new Rectangle(
                r.X + 12,
                r.Y + 8,
                r.Width - 38,
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
                Color closeColor = hoverClose
                    ? Color.FromArgb(60, 64, 67)
                    : Color.FromArgb(95, 99, 104);

                TextRenderer.DrawText(
                    g,
                    "×",
                    closeFont,
                    closeRect,
                    closeColor,
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
        private Rectangle AjustarRect(Rectangle r)
        {
            return new Rectangle(r.X + 2, r.Y + 4, r.Width - 4, r.Height - 6);
        }

        public Rectangle GetCloseRect(int index)
        {
            if (index < 0 || index >= TabPages.Count || EhAbaAdicionar(TabPages[index]))
                return Rectangle.Empty;

            Rectangle r = AjustarRect(GetTabRect(index));
            return new Rectangle(r.Right - 24, r.Y + 8, 16, 16);
        }

        private Rectangle GetCloseRect(Rectangle r)
        {
            return new Rectangle(r.Right - 24, r.Y + 8, 16, 16);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        private bool EhAbaAdicionar(TabPage tab)
        {
            return string.Equals(tab.Tag as string, "ADD_TAB", StringComparison.Ordinal);
        }

        private void DesenharAbaAdicionar(Graphics g, Rectangle areaTab, bool hover)
        {
            Rectangle rect = new Rectangle(areaTab.X + 10, areaTab.Y + 4, 32, areaTab.Height - 8);
            Color fundo = hover
                ? Color.FromArgb(232, 234, 237)
                : Color.FromArgb(248, 249, 250);
            Color borda = Color.FromArgb(210, 214, 220);
            Color simbolo = Color.FromArgb(60, 64, 67);

            using (GraphicsPath path = CreateTabPath(rect, 8))
            using (SolidBrush brush = new SolidBrush(fundo))
            using (Pen pen = new Pen(borda))
            using (Font font = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);

                TextRenderer.DrawText(
                    g,
                    "+",
                    font,
                    rect,
                    simbolo,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
