using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Pausa
{
    // El ícono se dibuja en memoria: un ojo, así la app queda en un único archivo
    public static class IconoApp
    {
        public static Icon Crear(Color acento)
        {
            using (Bitmap bmp = new Bitmap(64, 64))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    using (GraphicsPath ojo = new GraphicsPath())
                    {
                        ojo.AddBezier(4, 32, 22, 8, 42, 8, 60, 32);
                        ojo.AddBezier(60, 32, 42, 56, 22, 56, 4, 32);
                        using (Pen p = new Pen(acento, 5))
                        {
                            p.LineJoin = LineJoin.Round;
                            g.DrawPath(p, ojo);
                        }
                    }
                    using (Brush b = new SolidBrush(acento))
                        g.FillEllipse(b, 24, 22, 20, 20);
                    using (Brush b = new SolidBrush(Color.FromArgb(12, 17, 30)))
                        g.FillEllipse(b, 30, 27, 8, 8);
                }
                IntPtr h = bmp.GetHicon();
                using (Icon tmp = Icon.FromHandle(h))
                {
                    Icon copia = (Icon)tmp.Clone();
                    DestroyIcon(h);
                    return copia;
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool DestroyIcon(IntPtr handle);
    }
}
