using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pausa
{
    // Aviso chico en la esquina, unos segundos antes de la pausa, para poder cerrar lo que estás haciendo
    public class AvisoForm : Form
    {
        readonly string texto;
        readonly Color acento;
        readonly int segundos;
        int restante;
        readonly Timer t;

        public AvisoForm(string texto, Color acento, int segundos)
        {
            this.texto = texto;
            this.acento = acento;
            this.segundos = segundos;
            this.restante = segundos;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            BackColor = Color.FromArgb(12, 17, 30);
            Opacity = 0.94;
            Size = new Size(330, 84);
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(area.Right - Width - 18, area.Bottom - Height - 18);

            t = new Timer();
            t.Interval = 1000;
            t.Tick += delegate
            {
                restante--;
                Invalidate();
                if (restante <= 0) { t.Stop(); Close(); }
            };
            t.Start();
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            using (Pen p = new Pen(Color.FromArgb(44, 54, 76)))
                g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
            using (Brush b = new SolidBrush(acento))
                g.FillRectangle(b, 0, 0, 4, Height);

            using (Font f = new Font("Segoe UI", 11F, FontStyle.Bold))
            using (Brush b = new SolidBrush(Color.FromArgb(232, 238, 248)))
                g.DrawString(texto, f, b, 20, 16);

            using (Font f = new Font("Segoe UI", 9.5F))
            using (Brush b = new SolidBrush(Color.FromArgb(140, 152, 176)))
                g.DrawString("en " + restante + " segundos", f, b, 20, 40);

            int bw = Width - 40;
            double fr = (double)restante / segundos;
            using (Brush b = new SolidBrush(Color.FromArgb(30, 38, 56)))
                g.FillRectangle(b, 20, Height - 18, bw, 3);
            using (Brush b = new SolidBrush(Color.FromArgb(170, acento)))
                g.FillRectangle(b, 20, Height - 18, (int)(bw * fr), 3);
        }
    }
}
