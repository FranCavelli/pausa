using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Pausa
{
    public static class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("kernel32.dll")]
        static extern uint GetTickCount();

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [StructLayout(LayoutKind.Sequential)]
        struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        // Segundos que lleva el usuario sin tocar teclado ni mouse
        public static int SegundosSinActividad()
        {
            LASTINPUTINFO lii = new LASTINPUTINFO();
            lii.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
            if (!GetLastInputInfo(ref lii)) return 0;
            long delta = (long)((uint)Environment.TickCount - lii.dwTime);
            if (delta < 0) delta = 0;
            return (int)(delta / 1000);
        }

        // True si la ventana en primer plano ocupa toda la pantalla (juego, video, presentación)
        public static bool HayPantallaCompleta()
        {
            try
            {
                IntPtr h = GetForegroundWindow();
                if (h == IntPtr.Zero) return false;
                if (h == GetShellWindow() || h == GetDesktopWindow()) return false;

                StringBuilder clase = new StringBuilder(256);
                GetClassName(h, clase, clase.Capacity);
                string c = clase.ToString();
                if (c == "Progman" || c == "WorkerW" || c == "Shell_TrayWnd") return false;

                RECT r;
                if (!GetWindowRect(h, out r)) return false;
                Screen pantalla = Screen.FromHandle(h);
                return r.Left <= pantalla.Bounds.Left && r.Top <= pantalla.Bounds.Top
                    && r.Right >= pantalla.Bounds.Right && r.Bottom >= pantalla.Bounds.Bottom;
            }
            catch { return false; }
        }

        public static void TraerAlFrente(IntPtr h)
        {
            try { SetForegroundWindow(h); }
            catch { }
        }
    }
}
