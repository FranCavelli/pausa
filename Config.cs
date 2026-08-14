using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Pausa
{
    // Configuración persistida en %APPDATA%\Pausa\pausa.ini
    public class Config
    {
        public int MicroCadaMin = 20;      // regla 20-20-20
        public int MicroDuracionSeg = 20;
        public bool MicroActiva = true;

        public int PararseCadaMin = 30;    // cortar el sedentarismo
        public int PararseDuracionSeg = 20;
        public bool PararseActiva = true;

        public int LargaCadaMin = 60;      // pausa de recuperación
        public int LargaDuracionMin = 5;
        public bool LargaActiva = true;

        public bool PermitirPosponer = true;
        public int PosponerMin = 5;
        public bool PermitirSaltear = true;
        public bool Sonido = true;
        public bool PausarSiAusente = true;
        public int AusenteMin = 3;
        public bool RespetarPantallaCompleta = true;
        public bool ArrancarConWindows = true;
        public bool CuentaRegresivaPrevia = true;   // aviso 10 s antes
        public int OpacidadPorc = 95;

        // Procesos que, mientras esten corriendo, congelan todo (juegos que se abren por partida).
        // "League of Legends" es el proceso de la partida en sí: el cliente es otro y no cuenta.
        public List<string> AppsSinInterrupcion = new List<string>(new string[] { "League of Legends" });
        public bool PausaAlSalirDeApp = true;       // al cerrarse, pausa acorde a lo que duró

        public static string Carpeta
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pausa");
            }
        }

        public static string Ruta
        {
            get { return Path.Combine(Carpeta, "pausa.ini"); }
        }

        public static Config Cargar()
        {
            Config c = new Config();
            try
            {
                if (!File.Exists(Ruta)) return c;
                Dictionary<string, string> d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (string linea in File.ReadAllLines(Ruta, Encoding.UTF8))
                {
                    string l = linea.Trim();
                    if (l.Length == 0 || l.StartsWith("#") || l.StartsWith("[")) continue;
                    int i = l.IndexOf('=');
                    if (i <= 0) continue;
                    d[l.Substring(0, i).Trim()] = l.Substring(i + 1).Trim();
                }
                c.MicroCadaMin = Num(d, "MicroCadaMin", c.MicroCadaMin, 1, 240);
                c.MicroDuracionSeg = Num(d, "MicroDuracionSeg", c.MicroDuracionSeg, 5, 300);
                c.MicroActiva = Bol(d, "MicroActiva", c.MicroActiva);
                c.PararseCadaMin = Num(d, "PararseCadaMin", c.PararseCadaMin, 1, 240);
                c.PararseDuracionSeg = Num(d, "PararseDuracionSeg", c.PararseDuracionSeg, 5, 300);
                c.PararseActiva = Bol(d, "PararseActiva", c.PararseActiva);
                c.LargaCadaMin = Num(d, "LargaCadaMin", c.LargaCadaMin, 5, 480);
                c.LargaDuracionMin = Num(d, "LargaDuracionMin", c.LargaDuracionMin, 1, 60);
                c.LargaActiva = Bol(d, "LargaActiva", c.LargaActiva);
                c.PermitirPosponer = Bol(d, "PermitirPosponer", c.PermitirPosponer);
                c.PosponerMin = Num(d, "PosponerMin", c.PosponerMin, 1, 60);
                c.PermitirSaltear = Bol(d, "PermitirSaltear", c.PermitirSaltear);
                c.Sonido = Bol(d, "Sonido", c.Sonido);
                c.PausarSiAusente = Bol(d, "PausarSiAusente", c.PausarSiAusente);
                c.AusenteMin = Num(d, "AusenteMin", c.AusenteMin, 1, 60);
                c.RespetarPantallaCompleta = Bol(d, "RespetarPantallaCompleta", c.RespetarPantallaCompleta);
                c.ArrancarConWindows = Bol(d, "ArrancarConWindows", c.ArrancarConWindows);
                c.CuentaRegresivaPrevia = Bol(d, "CuentaRegresivaPrevia", c.CuentaRegresivaPrevia);
                c.OpacidadPorc = Num(d, "OpacidadPorc", c.OpacidadPorc, 60, 100);
                c.PausaAlSalirDeApp = Bol(d, "PausaAlSalirDeApp", c.PausaAlSalirDeApp);
                string apps;
                if (d.TryGetValue("AppsSinInterrupcion", out apps))
                    c.AppsSinInterrupcion = ParsearApps(apps);
            }
            catch { }
            return c;
        }

        public void Guardar()
        {
            try
            {
                Directory.CreateDirectory(Carpeta);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# Pausa - configuracion");
                sb.AppendLine("[general]");
                sb.AppendLine("MicroActiva=" + MicroActiva);
                sb.AppendLine("MicroCadaMin=" + MicroCadaMin);
                sb.AppendLine("MicroDuracionSeg=" + MicroDuracionSeg);
                sb.AppendLine("PararseActiva=" + PararseActiva);
                sb.AppendLine("PararseCadaMin=" + PararseCadaMin);
                sb.AppendLine("PararseDuracionSeg=" + PararseDuracionSeg);
                sb.AppendLine("LargaActiva=" + LargaActiva);
                sb.AppendLine("LargaCadaMin=" + LargaCadaMin);
                sb.AppendLine("LargaDuracionMin=" + LargaDuracionMin);
                sb.AppendLine("PermitirPosponer=" + PermitirPosponer);
                sb.AppendLine("PosponerMin=" + PosponerMin);
                sb.AppendLine("PermitirSaltear=" + PermitirSaltear);
                sb.AppendLine("Sonido=" + Sonido);
                sb.AppendLine("PausarSiAusente=" + PausarSiAusente);
                sb.AppendLine("AusenteMin=" + AusenteMin);
                sb.AppendLine("RespetarPantallaCompleta=" + RespetarPantallaCompleta);
                sb.AppendLine("ArrancarConWindows=" + ArrancarConWindows);
                sb.AppendLine("CuentaRegresivaPrevia=" + CuentaRegresivaPrevia);
                sb.AppendLine("OpacidadPorc=" + OpacidadPorc);
                sb.AppendLine("PausaAlSalirDeApp=" + PausaAlSalirDeApp);
                sb.AppendLine("AppsSinInterrupcion=" + string.Join("|", AppsSinInterrupcion.ToArray()));
                File.WriteAllText(Ruta, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }

        // Acepta separadas por | o por salto de línea, con o sin .exe
        public static List<string> ParsearApps(string texto)
        {
            List<string> lista = new List<string>();
            if (texto == null) return lista;
            foreach (string parte in texto.Split(new char[] { '|', '\n', '\r' }))
            {
                string n = parte.Trim();
                if (n.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    n = n.Substring(0, n.Length - 4);
                if (n.Length > 0 && !lista.Contains(n)) lista.Add(n);
            }
            return lista;
        }

        static int Num(Dictionary<string, string> d, string k, int def, int min, int max)
        {
            string v;
            int n;
            if (d.TryGetValue(k, out v) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out n))
                return Math.Max(min, Math.Min(max, n));
            return def;
        }

        static bool Bol(Dictionary<string, string> d, string k, bool def)
        {
            string v;
            bool b;
            if (d.TryGetValue(k, out v) && bool.TryParse(v, out b)) return b;
            return def;
        }
    }
}
