using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Pausa
{
    // Lista las apps abiertas con ventana propia para no tener que adivinar el nombre del proceso
    public class ElegirAppForm : Form
    {
        static readonly Color Fondo = Color.FromArgb(16, 21, 34);
        static readonly Color Panel = Color.FromArgb(22, 28, 44);
        static readonly Color Texto = Color.FromArgb(228, 234, 246);
        static readonly Color Suave = Color.FromArgb(142, 154, 178);
        static readonly Color Acento = Color.FromArgb(56, 189, 248);

        ListBox lista;
        List<string> procesos = new List<string>();
        public string Elegido = null;

        public ElegirAppForm()
        {
            Text = "Elegir una app abierta";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Fondo;
            ForeColor = Texto;
            Font = new Font("Segoe UI", 9.5F);
            ClientSize = new Size(430, 380);
            Icon = IconoApp.Crear(Acento);

            Label ayuda = new Label();
            ayuda.Text = "Si el juego se abre por partida (como el LoL), abrilo una vez\ny elegilo de esta lista mientras esté en partida.";
            ayuda.ForeColor = Suave;
            ayuda.AutoSize = true;
            ayuda.Location = new Point(14, 12);
            Controls.Add(ayuda);

            lista = new ListBox();
            lista.Location = new Point(14, 56);
            lista.Size = new Size(402, 262);
            lista.BackColor = Panel;
            lista.ForeColor = Texto;
            lista.BorderStyle = BorderStyle.FixedSingle;
            lista.DoubleClick += delegate { Aceptar(); };
            Controls.Add(lista);

            Button agregar = new Button();
            agregar.Text = "Agregar";
            agregar.Size = new Size(110, 32);
            agregar.Location = new Point(306, 330);
            agregar.FlatStyle = FlatStyle.Flat;
            agregar.BackColor = Acento;
            agregar.ForeColor = Color.FromArgb(8, 12, 20);
            agregar.FlatAppearance.BorderColor = Color.FromArgb(52, 62, 84);
            agregar.Click += delegate { Aceptar(); };
            Controls.Add(agregar);

            Button cancelar = new Button();
            cancelar.Text = "Cancelar";
            cancelar.Size = new Size(110, 32);
            cancelar.Location = new Point(188, 330);
            cancelar.FlatStyle = FlatStyle.Flat;
            cancelar.BackColor = Panel;
            cancelar.ForeColor = Suave;
            cancelar.FlatAppearance.BorderColor = Color.FromArgb(52, 62, 84);
            cancelar.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(cancelar);

            AcceptButton = agregar;
            CancelButton = cancelar;
            Cargar();
        }

        void Cargar()
        {
            List<string> vistos = new List<string>();
            try
            {
                Process[] todos = Process.GetProcesses();
                foreach (Process p in todos)
                {
                    try
                    {
                        if (p.MainWindowHandle == IntPtr.Zero) continue;
                        string titulo = p.MainWindowTitle;
                        if (titulo == null || titulo.Length == 0) continue;
                        if (p.ProcessName == "Pausa") continue;
                        if (vistos.Contains(p.ProcessName)) continue;
                        vistos.Add(p.ProcessName);
                        procesos.Add(p.ProcessName);
                        lista.Items.Add(p.ProcessName + "   ·   " + titulo);
                    }
                    catch { }
                    finally { p.Dispose(); }
                }
            }
            catch { }
            if (lista.Items.Count > 0) lista.SelectedIndex = 0;
        }

        void Aceptar()
        {
            int i = lista.SelectedIndex;
            if (i < 0 || i >= procesos.Count) return;
            Elegido = procesos[i];
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
