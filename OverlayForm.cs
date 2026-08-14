using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Windows.Forms;

namespace Pausa
{
    public enum Resultado { Completada, Pospuesta, Salteada }

    public class OverlayForm : Form
    {
        readonly Config cfg;
        readonly List<Ejercicio> pasos;
        readonly bool principal;
        readonly Color acento;
        readonly string etiqueta;
        readonly string consejo;
        readonly DateTime inicio;
        readonly int totalSeg;

        int pasoActual = 0;
        int restantePaso;
        int restanteTotal;
        Timer tick;
        Timer fade;
        Button btnPosponer;
        Button btnSaltar;
        bool cerrando = false;

        public Resultado Salida = Resultado.Completada;
        public event EventHandler Termino;

        static readonly Color Fondo = Color.FromArgb(9, 13, 24);
        static readonly Color TextoFuerte = Color.FromArgb(238, 242, 250);
        static readonly Color TextoSuave = Color.FromArgb(150, 162, 186);

        public OverlayForm(Config cfg, string etiqueta, List<Ejercicio> pasos, Color acento,
                           Screen pantalla, bool principal, DateTime inicio)
        {
            this.cfg = cfg;
            this.etiqueta = etiqueta;
            this.pasos = pasos;
            this.acento = acento;
            this.principal = principal;
            this.inicio = inicio;
            this.consejo = Salud.Consejo();

            totalSeg = 0;
            foreach (Ejercicio e in pasos) totalSeg += e.Segundos;
            restanteTotal = totalSeg;
            restantePaso = pasos[0].Segundos;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = pantalla.Bounds;
            BackColor = Fondo;
            ShowInTaskbar = principal;
            TopMost = true;
            KeyPreview = true;
            Opacity = 0;
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Text = "Pausa";

            if (principal) ArmarBotones();

            fade = new Timer();
            fade.Interval = 20;
            fade.Tick += FadeIn;
            fade.Start();

            tick = new Timer();
            tick.Interval = 250;
            tick.Tick += Tick;
            tick.Start();

            KeyDown += Teclas;
        }

        void ArmarBotones()
        {
            int ancho = 210, alto = 46;
            int y = Height - 110;

            if (cfg.PermitirPosponer)
            {
                btnPosponer = Boton("Posponer " + cfg.PosponerMin + " min   (Esc)", ancho, alto);
                btnPosponer.Location = new Point(Width / 2 - ancho - 12, y);
                btnPosponer.Click += delegate { Cerrar(Resultado.Pospuesta); };
                Controls.Add(btnPosponer);
            }
            if (cfg.PermitirSaltear)
            {
                btnSaltar = Boton("Saltear esta pausa   (S)", ancho, alto);
                btnSaltar.Location = new Point(cfg.PermitirPosponer ? Width / 2 + 12 : Width / 2 - ancho / 2, y);
                btnSaltar.Click += delegate { Cerrar(Resultado.Salteada); };
                Controls.Add(btnSaltar);
            }
        }

        Button Boton(string texto, int ancho, int alto)
        {
            Button b = new Button();
            b.Text = texto;
            b.Size = new Size(ancho, alto);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Color.FromArgb(52, 62, 84);
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(24, 32, 50);
            b.BackColor = Fondo;
            b.ForeColor = TextoSuave;
            b.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            b.Cursor = Cursors.Hand;
            b.TabStop = false;
            return b;
        }

        void FadeIn(object s, EventArgs e)
        {
            double objetivo = cfg.OpacidadPorc / 100.0;
            if (Opacity < objetivo - 0.02) Opacity += 0.06;
            else { Opacity = objetivo; fade.Stop(); }
        }

        void Teclas(object s, KeyEventArgs e)
        {
            if (!principal) return;
            if (e.KeyCode == Keys.Escape && cfg.PermitirPosponer) Cerrar(Resultado.Pospuesta);
            else if (e.KeyCode == Keys.S && cfg.PermitirSaltear) Cerrar(Resultado.Salteada);
        }

        void Tick(object s, EventArgs e)
        {
            int transcurrido = (int)(DateTime.Now - inicio).TotalSeconds;
            int restante = totalSeg - transcurrido;
            if (restante < 0) restante = 0;
            restanteTotal = restante;

            // ubicar en qué paso de la rutina estamos
            int acum = 0, idx = 0, dentro = 0;
            for (int i = 0; i < pasos.Count; i++)
            {
                if (transcurrido < acum + pasos[i].Segundos)
                {
                    idx = i;
                    dentro = acum + pasos[i].Segundos - transcurrido;
                    break;
                }
                acum += pasos[i].Segundos;
                idx = i;
                dentro = 0;
            }
            if (idx != pasoActual && cfg.Sonido && principal && restante > 0)
            {
                try { SystemSounds.Hand.Play(); }
                catch { }
            }
            pasoActual = idx;
            restantePaso = dentro;

            Invalidate();

            if (restante <= 0 && !cerrando) Cerrar(Resultado.Completada);
        }

        public void Cerrar(Resultado r)
        {
            if (cerrando) return;
            cerrando = true;
            Salida = r;
            tick.Stop();
            if (principal && cfg.Sonido && r == Resultado.Completada)
            {
                try { SystemSounds.Asterisk.Play(); }
                catch { }
            }
            if (Termino != null) Termino(this, EventArgs.Empty);
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // que no se cierre de un Alt+F4 despistado: hay botones para eso
            if (!cerrando && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

        protected override bool ShowWithoutActivation
        {
            get { return !principal; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Fondo);

            // resplandor suave detrás del anillo
            int cx = Width / 2;
            int cy = (int)(Height * 0.36);
            int radio = Math.Min(Width, Height) / 7;

            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddEllipse(cx - radio * 3, cy - radio * 3, radio * 6, radio * 6);
                using (PathGradientBrush pg = new PathGradientBrush(gp))
                {
                    pg.CenterColor = Color.FromArgb(38, acento);
                    pg.SurroundColors = new Color[] { Color.FromArgb(0, acento) };
                    g.FillPath(pg, gp);
                }
            }

            Ejercicio paso = pasos[Math.Min(pasoActual, pasos.Count - 1)];

            // anillo
            Rectangle rc = new Rectangle(cx - radio, cy - radio, radio * 2, radio * 2);
            using (Pen p = new Pen(Color.FromArgb(30, 38, 56), 12))
                g.DrawArc(p, rc, 0, 360);

            double frac = paso.Segundos > 0 ? (double)restantePaso / paso.Segundos : 0;
            using (Pen p = new Pen(acento, 12))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                g.DrawArc(p, rc, -90, (float)(-360 * frac));
            }

            // número grande
            string num = restantePaso >= 60
                ? string.Format("{0}:{1:00}", restantePaso / 60, restantePaso % 60)
                : restantePaso.ToString();
            using (Font f = new Font("Segoe UI Light", radio * 0.62F, FontStyle.Regular, GraphicsUnit.Pixel))
                Centrado(g, num, f, TextoFuerte, cy - (int)(radio * 0.42));

            // etiqueta arriba del anillo
            using (Font f = new Font("Segoe UI", 13F, FontStyle.Bold))
                Centrado(g, etiqueta.ToUpperInvariant(), f, acento, (int)(Height * 0.16), 3);

            // título y detalle
            using (Font f = new Font("Segoe UI Light", 42F))
                Centrado(g, paso.Titulo, f, TextoFuerte, cy + radio + 40);

            using (Font f = new Font("Segoe UI", 15F))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                using (Brush b = new SolidBrush(TextoSuave))
                    g.DrawString(paso.Detalle, f, b,
                        new RectangleF(Width * 0.18F, cy + radio + 120, Width * 0.64F, 140), sf);
            }

            // progreso general (solo si la pausa tiene varios pasos)
            if (pasos.Count > 1)
            {
                int bw = (int)(Width * 0.34), bh = 4;
                int bx = cx - bw / 2, by = Height - 165;
                using (Brush b = new SolidBrush(Color.FromArgb(30, 38, 56)))
                    g.FillRectangle(b, bx, by, bw, bh);
                double fr = 1.0 - (double)restanteTotal / totalSeg;
                using (Brush b = new SolidBrush(Color.FromArgb(120, acento)))
                    g.FillRectangle(b, bx, by, (int)(bw * fr), bh);
                using (Font f = new Font("Segoe UI", 9.5F))
                    Centrado(g, string.Format("{0} de {1}  ·  quedan {2}:{3:00}",
                        pasoActual + 1, pasos.Count, restanteTotal / 60, restanteTotal % 60),
                        f, Color.FromArgb(110, 122, 146), by + 16);
            }

            // consejo al pie
            using (Font f = new Font("Segoe UI", 10F, FontStyle.Italic))
                Centrado(g, consejo, f, Color.FromArgb(96, 108, 132), Height - 46);
        }

        void Centrado(Graphics g, string texto, Font f, Color c, int y)
        {
            Centrado(g, texto, f, c, y, 0);
        }

        void Centrado(Graphics g, string texto, Font f, Color c, int y, float espaciado)
        {
            using (StringFormat sf = new StringFormat())
            using (Brush b = new SolidBrush(c))
            {
                sf.Alignment = StringAlignment.Center;
                if (espaciado > 0)
                {
                    // separación de letras para el rótulo chico
                    string sep = "";
                    foreach (char ch in texto) sep += ch + " ";
                    texto = sep.TrimEnd();
                }
                g.DrawString(texto, f, b, new RectangleF(0, y, Width, f.GetHeight(g) * 2.2F), sf);
            }
        }
    }
}
