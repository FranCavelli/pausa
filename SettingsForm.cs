using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pausa
{
    public class SettingsForm : Form
    {
        static readonly Color Fondo = Color.FromArgb(16, 21, 34);
        static readonly Color Panel = Color.FromArgb(22, 28, 44);
        static readonly Color Texto = Color.FromArgb(228, 234, 246);
        static readonly Color Suave = Color.FromArgb(142, 154, 178);
        static readonly Color Acento = Color.FromArgb(56, 189, 248);

        public Config Cfg;

        CheckBox cMicro, cPararse, cLarga, cPosponer, cSaltear, cSonido, cAusente, cFull,
                 cInicio, cAviso, cSalirApp;
        NumericUpDown nMicroCada, nMicroDur, nPararCada, nPararDur, nLargaCada, nLargaDur,
                      nPosponer, nAusente, nOpacidad;
        TextBox tApps;

        int colX = 18;   // columna donde se van apilando los controles

        public SettingsForm(Config cfg)
        {
            Cfg = cfg;
            Text = "Pausa - configuración";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Fondo;
            ForeColor = Texto;
            Font = new Font("Segoe UI", 9.5F);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(900, 640);
            Icon = IconoApp.Crear(Acento);

            colX = 18;
            int y = 18;
            Titulo("Descanso ocular  ·  regla 20-20-20", ref y);
            cMicro = Check("Cortar cada tanto para mirar a lo lejos", ref y);
            nMicroCada = Numero("cada", 1, 240, ref y, "minutos");
            nMicroDur = Numero("durante", 5, 300, ref y, "segundos");
            Nota("La Academia Americana de Oftalmología recomienda mirar algo a 6 metros\ndurante 20 segundos, cada 20 minutos de pantalla.", ref y);

            Titulo("Levantarse  ·  cortar el sedentarismo", ref y);
            cPararse = Check("Avisarme para pararme y estirar", ref y);
            nPararCada = Numero("cada", 1, 240, ref y, "minutos");
            nPararDur = Numero("durante", 5, 300, ref y, "segundos");
            Nota("Estar sentado de corrido carga la lumbar y frena la circulación:\nun minuto de pie por cada media hora ya cambia las cosas.", ref y);

            Titulo("Pausa larga  ·  recuperación", ref y);
            cLarga = Check("Rutina guiada de movilidad y descanso", ref y);
            nLargaCada = Numero("cada", 5, 480, ref y, "minutos");
            nLargaDur = Numero("durante", 1, 60, ref y, "minutos");

            colX = 460;
            y = 18;
            Titulo("Comportamiento", ref y);
            cPosponer = Check("Permitir posponer", ref y);
            nPosponer = Numero("posponer", 1, 60, ref y, "minutos");
            cSaltear = Check("Permitir saltear la pausa", ref y);
            cAviso = Check("Avisarme 10 segundos antes", ref y);
            cAusente = Check("No contar el tiempo si estoy ausente", ref y);
            nAusente = Numero("ausente desde", 1, 60, ref y, "minutos");
            cFull = Check("No interrumpir si hay algo en pantalla completa", ref y);
            cSonido = Check("Sonido al empezar y terminar", ref y);
            nOpacidad = Numero("opacidad del cartel", 60, 100, ref y, "%");
            cInicio = Check("Arrancar con Windows", ref y);

            y += 10;
            Titulo("Juegos y apps que no se interrumpen", ref y);
            Nota("Un proceso por línea. Mientras alguno esté abierto no salta ninguna\npausa y el reloj queda en cero. Pensado para juegos que se abren por\npartida: en el LoL el proceso de la partida es \"League of Legends\".", ref y);
            tApps = new TextBox();
            tApps.Multiline = true;
            tApps.ScrollBars = ScrollBars.Vertical;
            tApps.Location = new Point(colX + 4, y);
            tApps.Size = new Size(400, 76);
            tApps.BackColor = Panel;
            tApps.ForeColor = Texto;
            tApps.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(tApps);
            y += 84;

            Button elegir = Boton("Elegir de las apps abiertas...", Panel, Suave);
            elegir.Width = 190;
            elegir.Location = new Point(colX + 4, y);
            elegir.Click += delegate { ElegirApp(); };
            Controls.Add(elegir);
            y += 42;

            cSalirApp = Check("Al cerrarse, mandarme la pausa que corresponda", ref y);

            Button guardar = Boton("Guardar", Acento, Color.FromArgb(8, 12, 20));
            guardar.Location = new Point(ClientSize.Width - 130, ClientSize.Height - 52);
            guardar.Click += delegate { Aplicar(); DialogResult = DialogResult.OK; Close(); };
            Controls.Add(guardar);

            Button cancelar = Boton("Cancelar", Panel, Suave);
            cancelar.Location = new Point(ClientSize.Width - 246, ClientSize.Height - 52);
            cancelar.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(cancelar);

            Button recomendado = Boton("Valores recomendados", Panel, Suave);
            recomendado.Width = 170;
            recomendado.Location = new Point(18, ClientSize.Height - 52);
            recomendado.Click += delegate { Volcar(new Config()); };
            Controls.Add(recomendado);

            AcceptButton = guardar;
            CancelButton = cancelar;
            Volcar(cfg);
        }

        void ElegirApp()
        {
            using (ElegirAppForm f = new ElegirAppForm())
            {
                f.TopMost = TopMost;
                if (f.ShowDialog(this) == DialogResult.OK && f.Elegido != null)
                {
                    string actual = tApps.Text.TrimEnd();
                    if (actual.Length > 0) actual += Environment.NewLine;
                    tApps.Text = actual + f.Elegido;
                }
            }
        }

        void Titulo(string t, ref int y)
        {
            y += 6;
            Label l = new Label();
            l.Text = t.ToUpperInvariant();
            l.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            l.ForeColor = Acento;
            l.AutoSize = true;
            l.Location = new Point(colX, y);
            Controls.Add(l);
            y += 24;
        }

        void Nota(string t, ref int y)
        {
            Label l = new Label();
            l.Text = t;
            l.Font = new Font("Segoe UI", 8.5F);
            l.ForeColor = Color.FromArgb(110, 122, 146);
            l.AutoSize = true;
            l.Location = new Point(colX + 22, y);
            Controls.Add(l);
            y += 16 * (t.Split('\n').Length) + 12;
        }

        CheckBox Check(string t, ref int y)
        {
            CheckBox c = new CheckBox();
            c.Text = t;
            c.ForeColor = Texto;
            c.AutoSize = true;
            c.Location = new Point(colX + 4, y);
            c.FlatStyle = FlatStyle.Flat;
            Controls.Add(c);
            y += 26;
            return c;
        }

        NumericUpDown Numero(string etiqueta, int min, int max, ref int y, string unidad)
        {
            Label l = new Label();
            l.Text = etiqueta;
            l.ForeColor = Suave;
            l.AutoSize = true;
            l.Location = new Point(colX + 26, y + 3);
            Controls.Add(l);

            NumericUpDown n = new NumericUpDown();
            n.Minimum = min;
            n.Maximum = max;
            n.Width = 62;
            n.Location = new Point(colX + 197, y);
            n.BackColor = Panel;
            n.ForeColor = Texto;
            n.BorderStyle = BorderStyle.FixedSingle;
            n.TextAlign = HorizontalAlignment.Center;
            Controls.Add(n);

            Label u = new Label();
            u.Text = unidad;
            u.ForeColor = Suave;
            u.AutoSize = true;
            u.Location = new Point(colX + 265, y + 3);
            Controls.Add(u);

            y += 30;
            return n;
        }

        Button Boton(string t, Color fondo, Color texto)
        {
            Button b = new Button();
            b.Text = t;
            b.Size = new Size(110, 34);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Color.FromArgb(52, 62, 84);
            b.BackColor = fondo;
            b.ForeColor = texto;
            b.Cursor = Cursors.Hand;
            return b;
        }

        void Volcar(Config c)
        {
            cMicro.Checked = c.MicroActiva;
            nMicroCada.Value = c.MicroCadaMin;
            nMicroDur.Value = c.MicroDuracionSeg;
            cPararse.Checked = c.PararseActiva;
            nPararCada.Value = c.PararseCadaMin;
            nPararDur.Value = c.PararseDuracionSeg;
            cLarga.Checked = c.LargaActiva;
            nLargaCada.Value = c.LargaCadaMin;
            nLargaDur.Value = c.LargaDuracionMin;
            cPosponer.Checked = c.PermitirPosponer;
            nPosponer.Value = c.PosponerMin;
            cSaltear.Checked = c.PermitirSaltear;
            cAviso.Checked = c.CuentaRegresivaPrevia;
            cAusente.Checked = c.PausarSiAusente;
            nAusente.Value = c.AusenteMin;
            cFull.Checked = c.RespetarPantallaCompleta;
            cSonido.Checked = c.Sonido;
            nOpacidad.Value = c.OpacidadPorc;
            cInicio.Checked = c.ArrancarConWindows;
            cSalirApp.Checked = c.PausaAlSalirDeApp;
            tApps.Text = string.Join(Environment.NewLine, c.AppsSinInterrupcion.ToArray());
        }

        void Aplicar()
        {
            Cfg.MicroActiva = cMicro.Checked;
            Cfg.MicroCadaMin = (int)nMicroCada.Value;
            Cfg.MicroDuracionSeg = (int)nMicroDur.Value;
            Cfg.PararseActiva = cPararse.Checked;
            Cfg.PararseCadaMin = (int)nPararCada.Value;
            Cfg.PararseDuracionSeg = (int)nPararDur.Value;
            Cfg.LargaActiva = cLarga.Checked;
            Cfg.LargaCadaMin = (int)nLargaCada.Value;
            Cfg.LargaDuracionMin = (int)nLargaDur.Value;
            Cfg.PermitirPosponer = cPosponer.Checked;
            Cfg.PosponerMin = (int)nPosponer.Value;
            Cfg.PermitirSaltear = cSaltear.Checked;
            Cfg.CuentaRegresivaPrevia = cAviso.Checked;
            Cfg.PausarSiAusente = cAusente.Checked;
            Cfg.AusenteMin = (int)nAusente.Value;
            Cfg.RespetarPantallaCompleta = cFull.Checked;
            Cfg.Sonido = cSonido.Checked;
            Cfg.OpacidadPorc = (int)nOpacidad.Value;
            Cfg.ArrancarConWindows = cInicio.Checked;
            Cfg.PausaAlSalirDeApp = cSalirApp.Checked;
            Cfg.AppsSinInterrupcion = Config.ParsearApps(tApps.Text);
            Cfg.Guardar();
        }
    }
}
