using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Pausa
{
    enum TipoPausa { Ocular, DePie, Combinada, Larga }

    public class TrayApp : ApplicationContext
    {
        static readonly Color AzulOjo = Color.FromArgb(56, 189, 248);
        static readonly Color Ambar = Color.FromArgb(251, 191, 36);
        static readonly Color Violeta = Color.FromArgb(167, 139, 250);

        Config cfg;
        NotifyIcon tray;
        System.Windows.Forms.Timer reloj;
        ToolStripMenuItem miProxima, miSuspender, miReanudar, miInicio;

        int microAcum, pararseAcum, largaAcum;
        int pantallaHoy;                 // segundos de uso frente a la pantalla
        int completadas, pospuestas, salteadas;
        DateTime diaStats = DateTime.Today;

        DateTime? suspendidoHasta = null;
        bool suspendidoIndefinido = false;
        bool overlayActivo = false;
        bool avisoMostrado = false;
        List<OverlayForm> overlays = new List<OverlayForm>();

        string appEnCurso = null;          // juego o app de la lista que está abierto ahora
        DateTime appDesde;
        int chequeoApp = 0;
        int retrasoSalida = 0;             // segundos entre que cierra la app y salta la pausa

        public TrayApp()
        {
            cfg = Config.Cargar();
            SincronizarInicioWindows();

            tray = new NotifyIcon();
            tray.Icon = IconoApp.Crear(AzulOjo);
            tray.Visible = true;
            tray.Text = "Pausa";
            tray.ContextMenuStrip = ArmarMenu();
            tray.DoubleClick += delegate { AbrirConfig(); };
            tray.BalloonTipTitle = "Pausa";
            tray.BalloonTipText = "Cuidando tus ojos y tu espalda. El ícono queda en la bandeja.";
            tray.ShowBalloonTip(4000);

            reloj = new System.Windows.Forms.Timer();
            reloj.Interval = 1000;
            reloj.Tick += Segundo;
            reloj.Start();

            SystemEvents.PowerModeChanged += delegate(object s, PowerModeChangedEventArgs e)
            {
                if (e.Mode == PowerModes.Resume) ReiniciarContadores();
            };
            SystemEvents.SessionSwitch += delegate(object s, SessionSwitchEventArgs e)
            {
                if (e.Reason == SessionSwitchReason.SessionUnlock ||
                    e.Reason == SessionSwitchReason.SessionLogon) ReiniciarContadores();
            };
        }

        // ---------- menú de la bandeja ----------

        ContextMenuStrip ArmarMenu()
        {
            ContextMenuStrip m = new ContextMenuStrip();
            m.Font = new Font("Segoe UI", 9.5F);

            miProxima = new ToolStripMenuItem("Próxima pausa: --");
            miProxima.Enabled = false;
            m.Items.Add(miProxima);
            m.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem ya = new ToolStripMenuItem("Descanso ocular ahora");
            ya.Click += delegate { Disparar(TipoPausa.Ocular, true); };
            m.Items.Add(ya);

            ToolStripMenuItem larga = new ToolStripMenuItem("Pausa larga ahora");
            larga.Click += delegate { Disparar(TipoPausa.Larga, true); };
            m.Items.Add(larga);
            m.Items.Add(new ToolStripSeparator());

            miSuspender = new ToolStripMenuItem("Suspender");
            ToolStripMenuItem s30 = new ToolStripMenuItem("30 minutos");
            s30.Click += delegate { Suspender(30, false); };
            ToolStripMenuItem s60 = new ToolStripMenuItem("1 hora");
            s60.Click += delegate { Suspender(60, false); };
            ToolStripMenuItem s180 = new ToolStripMenuItem("3 horas");
            s180.Click += delegate { Suspender(180, false); };
            ToolStripMenuItem sInd = new ToolStripMenuItem("Hasta que lo reactive");
            sInd.Click += delegate { Suspender(0, true); };
            miSuspender.DropDownItems.AddRange(new ToolStripItem[] { s30, s60, s180, sInd });
            m.Items.Add(miSuspender);

            miReanudar = new ToolStripMenuItem("Reanudar");
            miReanudar.Visible = false;
            miReanudar.Click += delegate { Reanudar(); };
            m.Items.Add(miReanudar);
            m.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem resumen = new ToolStripMenuItem("Resumen del día");
            resumen.Click += delegate { MostrarResumen(); };
            m.Items.Add(resumen);

            ToolStripMenuItem conf = new ToolStripMenuItem("Configuración...");
            conf.Click += delegate { AbrirConfig(); };
            m.Items.Add(conf);

            miInicio = new ToolStripMenuItem("Arrancar con Windows");
            miInicio.CheckOnClick = true;
            miInicio.Checked = cfg.ArrancarConWindows;
            miInicio.Click += delegate
            {
                cfg.ArrancarConWindows = miInicio.Checked;
                cfg.Guardar();
                SincronizarInicioWindows();
            };
            m.Items.Add(miInicio);
            m.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem salir = new ToolStripMenuItem("Salir");
            salir.Click += delegate { Cerrar(); };
            m.Items.Add(salir);
            return m;
        }

        // ---------- reloj ----------

        void Segundo(object s, EventArgs e)
        {
            if (DateTime.Today != diaStats)
            {
                diaStats = DateTime.Today;
                pantallaHoy = completadas = pospuestas = salteadas = 0;
            }

            if (overlayActivo) return;

            if (suspendidoIndefinido) { Tooltip("suspendido"); return; }
            if (suspendidoHasta.HasValue)
            {
                if (DateTime.Now < suspendidoHasta.Value)
                {
                    TimeSpan q = suspendidoHasta.Value - DateTime.Now;
                    Tooltip("suspendido " + (int)q.TotalMinutes + " min más");
                    return;
                }
                Reanudar();
            }

            if (!ManejarAppSinInterrupcion()) return;

            int idle = Native.SegundosSinActividad();
            if (cfg.PausarSiAusente && idle >= cfg.AusenteMin * 60)
            {
                // un rato lejos del teclado ya es la pausa que buscábamos
                microAcum = 0;
                pararseAcum = 0;
                if (idle >= cfg.LargaDuracionMin * 60) largaAcum = 0;
                avisoMostrado = false;
                Tooltip("ausente, contador en pausa");
                return;
            }

            microAcum++;
            pararseAcum++;
            largaAcum++;
            pantallaHoy++;

            int faltan = SegundosParaProxima();
            Tooltip(null);

            if (cfg.CuentaRegresivaPrevia && !avisoMostrado && faltan <= 10 && faltan > 0)
            {
                avisoMostrado = true;
                TipoPausa t = ProximoTipo();
                AvisoForm a = new AvisoForm(TextoEtiqueta(t), ColorDe(t), Math.Max(3, faltan));
                a.Show();
            }

            if (faltan <= 0)
            {
                avisoMostrado = false;
                Disparar(ProximoTipo(), false);
            }
        }

        // Mientras haya un juego de la lista abierto, no se interrumpe ni se acumula tiempo.
        // Al cerrarse (en el LoL eso es el fin de la partida) la pausa que toca sale sola,
        // proporcional a lo que duró. Devuelve false si hay que cortar el tick acá.
        bool ManejarAppSinInterrupcion()
        {
            if (++chequeoApp >= 2)
            {
                chequeoApp = 0;
                string abierta = AppDeLaLista();

                if (abierta != null && appEnCurso == null)
                {
                    // arrancó la partida: el reloj vuelve a cero y queda congelado
                    appEnCurso = abierta;
                    appDesde = DateTime.Now;
                    ReiniciarContadores();
                }
                else if (abierta == null && appEnCurso != null)
                {
                    int duro = (int)(DateTime.Now - appDesde).TotalSeconds;
                    appEnCurso = null;
                    if (cfg.PausaAlSalirDeApp)
                    {
                        microAcum = duro;
                        pararseAcum = duro;
                        largaAcum = duro;
                        pantallaHoy += duro;
                        if (SegundosParaProxima() <= 0)
                        {
                            retrasoSalida = 10;
                            if (cfg.CuentaRegresivaPrevia)
                            {
                                TipoPausa t = ProximoTipo();
                                new AvisoForm(TextoEtiqueta(t), ColorDe(t), retrasoSalida).Show();
                            }
                        }
                    }
                    else ReiniciarContadores();
                }
            }

            if (appEnCurso != null)
            {
                Tooltip(appEnCurso + ", sin interrupciones");
                return false;
            }

            if (retrasoSalida > 0)
            {
                retrasoSalida--;
                Tooltip("pausa en " + retrasoSalida + " s");
                return false;
            }
            return true;
        }

        string AppDeLaLista()
        {
            foreach (string nombre in cfg.AppsSinInterrupcion)
            {
                if (nombre == null || nombre.Length == 0) continue;
                try
                {
                    Process[] ps = Process.GetProcessesByName(nombre);
                    bool hay = ps.Length > 0;
                    foreach (Process p in ps) p.Dispose();
                    if (hay) return nombre;
                }
                catch { }
            }
            return null;
        }

        int RestanteMicro()
        {
            return cfg.MicroActiva ? cfg.MicroCadaMin * 60 - microAcum : int.MaxValue;
        }

        int RestantePararse()
        {
            return cfg.PararseActiva ? cfg.PararseCadaMin * 60 - pararseAcum : int.MaxValue;
        }

        int RestanteLarga()
        {
            return cfg.LargaActiva ? cfg.LargaCadaMin * 60 - largaAcum : int.MaxValue;
        }

        // gana la que vence antes; si la ocular y la de pararse caen juntas, van en una sola
        TipoPausa ProximoTipo()
        {
            int rMicro = RestanteMicro(), rParar = RestantePararse(), rLarga = RestanteLarga();
            int menor = Math.Min(rLarga, Math.Min(rMicro, rParar));
            if (menor == int.MaxValue) return TipoPausa.Ocular;
            if (rLarga == menor) return TipoPausa.Larga;
            if (rMicro == menor && rParar == menor) return TipoPausa.Combinada;
            if (rParar == menor) return TipoPausa.DePie;
            return TipoPausa.Ocular;
        }

        int SegundosParaProxima()
        {
            return Math.Min(RestanteLarga(), Math.Min(RestanteMicro(), RestantePararse()));
        }

        void Tooltip(string estado)
        {
            string t;
            if (estado != null) t = "Pausa - " + estado;
            else
            {
                int f = SegundosParaProxima();
                if (f == int.MaxValue) t = "Pausa - sin pausas activas";
                else t = "Pausa - próxima en " + (f >= 60 ? (f / 60) + " min" : f + " s");
            }
            if (t.Length > 63) t = t.Substring(0, 63);
            tray.Text = t;
            if (miProxima != null) miProxima.Text = t.Replace("Pausa - ", "Próxima pausa: ");
        }

        // ---------- pausas ----------

        void Disparar(TipoPausa tipo, bool manual)
        {
            if (overlayActivo) return;

            if (!manual && cfg.RespetarPantallaCompleta && Native.HayPantallaCompleta())
            {
                // hay algo a pantalla completa: reintento en un par de minutos
                microAcum = Math.Max(0, microAcum - 120);
                pararseAcum = Math.Max(0, pararseAcum - 120);
                largaAcum = Math.Max(0, largaAcum - 120);
                Tooltip("esperando, hay pantalla completa");
                return;
            }

            List<Ejercicio> pasos = new List<Ejercicio>();
            string[] v;
            switch (tipo)
            {
                case TipoPausa.Larga:
                    pasos = Salud.RutinaLarga(cfg.LargaDuracionMin * 60);
                    break;
                case TipoPausa.DePie:
                    v = Salud.MicroDePie();
                    pasos.Add(new Ejercicio(v[0], v[1], cfg.PararseDuracionSeg));
                    break;
                case TipoPausa.Combinada:
                    v = Salud.MicroDePie();
                    pasos.Add(new Ejercicio(v[0], v[1], cfg.PararseDuracionSeg));
                    v = Salud.MicroOcular();
                    pasos.Add(new Ejercicio(v[0], v[1], cfg.MicroDuracionSeg));
                    break;
                default:
                    v = Salud.MicroOcular();
                    pasos.Add(new Ejercicio(v[0], v[1], cfg.MicroDuracionSeg));
                    break;
            }

            overlayActivo = true;
            if (cfg.Sonido)
            {
                try { SystemSounds.Asterisk.Play(); }
                catch { }
            }

            DateTime inicio = DateTime.Now;
            Screen conCursor = Screen.FromPoint(Cursor.Position);
            overlays.Clear();
            OverlayForm maestro = null;

            foreach (Screen sc in Screen.AllScreens)
            {
                bool esPrincipal = sc.Equals(conCursor) && maestro == null;
                OverlayForm o = new OverlayForm(cfg, TextoEtiqueta(tipo), pasos, ColorDe(tipo),
                                                sc, esPrincipal, inicio);
                overlays.Add(o);
                if (esPrincipal) maestro = o;
            }
            if (maestro == null && overlays.Count > 0) maestro = overlays[0];

            maestro.Termino += delegate(object ss, EventArgs ee)
            {
                Resultado r = maestro.Salida;
                foreach (OverlayForm o in overlays)
                    if (o != maestro) o.Cerrar(r);
                overlays.Clear();
                overlayActivo = false;
                avisoMostrado = false;
                Cerrada(tipo, r);
            };

            foreach (OverlayForm o in overlays) o.Show();
            maestro.Activate();
            Native.TraerAlFrente(maestro.Handle);
        }

        void Cerrada(TipoPausa tipo, Resultado r)
        {
            if (r == Resultado.Pospuesta)
            {
                pospuestas++;
                int atras = cfg.PosponerMin * 60;
                if (tipo == TipoPausa.Larga) largaAcum = Math.Max(0, cfg.LargaCadaMin * 60 - atras);
                if (tipo == TipoPausa.Ocular || tipo == TipoPausa.Combinada)
                    microAcum = Math.Max(0, cfg.MicroCadaMin * 60 - atras);
                if (tipo == TipoPausa.DePie || tipo == TipoPausa.Combinada)
                    pararseAcum = Math.Max(0, cfg.PararseCadaMin * 60 - atras);
                return;
            }

            if (r == Resultado.Completada) completadas++;
            else salteadas++;

            switch (tipo)
            {
                case TipoPausa.Larga:
                    largaAcum = 0; microAcum = 0; pararseAcum = 0;
                    break;
                case TipoPausa.Combinada:
                    microAcum = 0; pararseAcum = 0;
                    break;
                case TipoPausa.DePie:
                    pararseAcum = 0;
                    break;
                default:
                    microAcum = 0;
                    break;
            }
        }

        static string TextoEtiqueta(TipoPausa t)
        {
            switch (t)
            {
                case TipoPausa.Larga: return "Pausa larga";
                case TipoPausa.DePie: return "Movete";
                case TipoPausa.Combinada: return "Pausa";
                default: return "Descanso ocular";
            }
        }

        static Color ColorDe(TipoPausa t)
        {
            switch (t)
            {
                case TipoPausa.Larga: return Violeta;
                case TipoPausa.DePie: return Ambar;
                case TipoPausa.Combinada: return Ambar;
                default: return AzulOjo;
            }
        }

        // ---------- acciones del menú ----------

        void Suspender(int minutos, bool indefinido)
        {
            suspendidoIndefinido = indefinido;
            suspendidoHasta = indefinido ? (DateTime?)null : DateTime.Now.AddMinutes(minutos);
            miReanudar.Visible = true;
            ReiniciarContadores();
        }

        void Reanudar()
        {
            suspendidoIndefinido = false;
            suspendidoHasta = null;
            miReanudar.Visible = false;
            ReiniciarContadores();
        }

        void ReiniciarContadores()
        {
            microAcum = 0;
            pararseAcum = 0;
            largaAcum = 0;
            avisoMostrado = false;
        }

        void AbrirConfig()
        {
            using (SettingsForm f = new SettingsForm(cfg))
            {
                f.TopMost = true;
                if (f.ShowDialog() == DialogResult.OK)
                {
                    cfg = f.Cfg;
                    miInicio.Checked = cfg.ArrancarConWindows;
                    SincronizarInicioWindows();
                    ReiniciarContadores();
                }
            }
        }

        void MostrarResumen()
        {
            int h = pantallaHoy / 3600, m = (pantallaHoy % 3600) / 60;
            string txt =
                "Pantalla hoy: " + h + " h " + m + " min\n\n" +
                "Pausas completadas: " + completadas + "\n" +
                "Pospuestas: " + pospuestas + "\n" +
                "Salteadas: " + salteadas + "\n\n" +
                "Ritmo actual:\n" +
                "  ojos cada " + cfg.MicroCadaMin + " min (" + cfg.MicroDuracionSeg + " s)\n" +
                "  pararse cada " + cfg.PararseCadaMin + " min\n" +
                "  pausa larga cada " + cfg.LargaCadaMin + " min (" + cfg.LargaDuracionMin + " min)";
            MessageBox.Show(txt, "Pausa - resumen del día",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void Cerrar()
        {
            reloj.Stop();
            tray.Visible = false;
            tray.Dispose();
            ExitThread();
        }

        // ---------- arranque con Windows ----------

        void SincronizarInicioWindows()
        {
            try
            {
                string exe = Assembly.GetExecutingAssembly().Location;
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (k == null) return;
                    if (cfg.ArrancarConWindows) k.SetValue("Pausa", "\"" + exe + "\"");
                    else if (k.GetValue("Pausa") != null) k.DeleteValue("Pausa", false);
                }
            }
            catch { }
        }
    }

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            bool nuevo;
            using (Mutex mtx = new Mutex(true, "Pausa.Instancia", out nuevo))
            {
                if (!nuevo)
                {
                    MessageBox.Show("Pausa ya está corriendo. Mirá el ícono del ojo en la bandeja.",
                        "Pausa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TrayApp());
            }
        }
    }
}
