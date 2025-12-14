using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Motor3D_Educativo_P2
{
    public partial class Form1 : Form
    {
        Modelo3D modeloActual;
        Point centroPantalla;
        float rotacionAuto = 0;

        public Form1()
        {
            InitializeComponent();

            // Importante para que no parpadee
            this.DoubleBuffered = true;
            if (pictureBox1 != null) { } // Solo referencia para asegurar que existe
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Carga inicial
            modeloActual = MeshFactory.CrearCubo(100);

            // Timer de animación
            Timer t = new Timer();
            t.Interval = 30;
            t.Tick += (s, ev) => {
                rotacionAuto += 2.0f;
                if (modeloActual != null)
                {
                    // Combinamos rotación manual (sliders) + automática
                    float rx = (trackRotX != null) ? trackRotX.Value : 0;
                    float ry = (trackRotY != null) ? trackRotY.Value : 0;

                    modeloActual.RotX = rx;
                    modeloActual.RotY = ry + rotacionAuto;
                    pictureBox1.Invalidate(); // Pedir redibujar
                }
            };
            t.Start();

            // Configurar botones si existen
            if (btnCambiar != null) btnCambiar.Click += (s, ev) => CambiarFigura();
        }

        int figuraIndice = 0;
        void CambiarFigura()
        {
            figuraIndice++;
            if (figuraIndice > 2) figuraIndice = 0;

            switch (figuraIndice)
            {
                case 0: modeloActual = MeshFactory.CrearCubo(100); break;
                case 1: modeloActual = MeshFactory.CrearPiramide(100, 150); break;
                case 2: modeloActual = MeshFactory.CrearCilindro(50, 150, 12); break;
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // Configuración de Calidad
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.Black);

            if (modeloActual != null)
            {
                // Calcular centro
                centroPantalla = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
                modeloActual.Draw(e.Graphics, centroPantalla);
            }
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
        }
    }
}