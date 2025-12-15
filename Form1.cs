using Motor3D_Educativo_P2.Geometry;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Motor3D_Educativo_P2
{
    public partial class Form1 : Form
    {
        Modelo3D modeloActual;
        Camara camara = new Camara(); // Instancia de la cámara
        Point centroPantalla;
        Point lastMousePos;
        bool isMouseDown = false;
        int figuraIndice = 0;

        public Form1()
        {
            InitializeComponent();

            // Configuración vital para gráficos suaves
            this.DoubleBuffered = true;

            // Habilitar captura de teclas para WASD
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            // Eventos del Mouse
            pictureBox1.MouseDown += (s, e) => { isMouseDown = true; lastMousePos = e.Location; };
            pictureBox1.MouseUp += (s, e) => { isMouseDown = false; };
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Cargar cubo por defecto
            modeloActual = MeshFactory.CrearCubo(100);

            // -- GENERACIÓN DE UI --
            flowLayoutPanel1.Controls.Clear();
            Button btn = new Button(); btn.Text = "Cambiar Figura"; btn.Height = 40; btn.Width = 200;
            btn.Click += (s, ev) => CambiarFigura();
            flowLayoutPanel1.Controls.Add(btn);

            GenerarSliders("TRASLACIÓN", -200, 200, 0, (c, v) => {
                if (modeloActual == null) return;
                if (c == 'X') modeloActual.Position.x = v; if (c == 'Y') modeloActual.Position.y = v; if (c == 'Z') modeloActual.Position.z = v;
                pictureBox1.Invalidate();
            });
            GenerarSliders("ROTACIÓN", 0, 360, 0, (c, v) => {
                if (modeloActual == null) return;
                if (c == 'X') modeloActual.Rotation.x = v; if (c == 'Y') modeloActual.Rotation.y = v; if (c == 'Z') modeloActual.Rotation.z = v;
                pictureBox1.Invalidate();
            });
            GenerarSliders("ESCALA", 1, 30, 10, (c, v) => {
                if (modeloActual == null) return;
                float s = v / 10.0f;
                if (c == 'X') modeloActual.Scale.x = s; if (c == 'Y') modeloActual.Scale.y = s; if (c == 'Z') modeloActual.Scale.z = s;
                pictureBox1.Invalidate();
            });
        }

        // Helper para UI (simplificado)
        void GenerarSliders(string t, int min, int max, int def, Action<char, int> act)
        {
            GroupBox g = new GroupBox(); g.Text = t; g.Size = new Size(220, 160); g.ForeColor = Color.White;
            int y = 20;
            foreach (char c in new[] { 'X', 'Y', 'Z' })
            {
                Label l = new Label(); l.Text = c.ToString(); l.Location = new Point(10, y); l.ForeColor = Color.Black; l.AutoSize = true;
                TrackBar tb = new TrackBar(); tb.Location = new Point(30, y); tb.Size = new Size(180, 30); tb.Minimum = min; tb.Maximum = max; tb.Value = def; tb.TickStyle = TickStyle.None;
                tb.Scroll += (s, e) => act(c, tb.Value);
                g.Controls.Add(l); g.Controls.Add(tb); y += 45;
            }
            flowLayoutPanel1.Controls.Add(g);
        }

        void CambiarFigura()
        {
            figuraIndice++; if (figuraIndice > 4) figuraIndice = 0;
            switch (figuraIndice)
            {
                case 0: modeloActual = MeshFactory.CrearCubo(100); break;
                case 1: modeloActual = MeshFactory.CrearPiramide(100, 150); break;
                case 2: modeloActual = MeshFactory.CrearCilindro(60, 150, 16); break;
                case 3: modeloActual = MeshFactory.CrearCono(60, 150, 16); break;
                case 4: modeloActual = MeshFactory.CrearEsfera(80, 12, 12); break;
            }
            modeloActual.Rotation = new Math3D.Vector3D(0, 0, 0);
            pictureBox1.Invalidate();
        }

        // --- CONTROLES WASD ---
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            float s = camara.Speed;
            switch (e.KeyCode)
            {
                case Keys.W: camara.Position.z += s; break;
                case Keys.S: camara.Position.z -= s; break;
                case Keys.A: camara.Position.x -= s; break;
                case Keys.D: camara.Position.x += s; break;
                case Keys.Q: camara.Position.y += s; break; // Subir
                case Keys.E: camara.Position.y -= s; break; // Bajar
            }
            pictureBox1.Invalidate();
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                camara.Rotation.y += (e.X - lastMousePos.X) * 0.5f;
                camara.Rotation.x += (e.Y - lastMousePos.Y) * 0.5f;
                lastMousePos = e.Location;
                pictureBox1.Invalidate();
            }
        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            camara.Position.z += e.Delta * 0.5f;
            pictureBox1.Invalidate();
        }

        // --- RENDERIZADO PRINCIPAL ---
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.FromArgb(20, 20, 20)); // Fondo Gris Oscuro
            centroPantalla = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);

            // 1. Dibujar el Suelo (Grid)
            DrawGrid(e.Graphics);

            // 2. Dibujar la Figura
            if (modeloActual != null)
            {
                modeloActual.Draw(e.Graphics, centroPantalla, camara);
            }
        }

        // --- DIBUJO DEL GRID Y AUXILIARES (Aquí tenías errores antes) ---
        private void DrawGrid(Graphics g)
        {
            int size = 2000; int step = 200;
            Pen gridPen = new Pen(Color.FromArgb(60, 60, 60));

            for (int i = -size; i <= size; i += step)
            {
                DrawLine3D(g, gridPen, new Math3D.Vector3D(-size, 0, i), new Math3D.Vector3D(size, 0, i));
                DrawLine3D(g, gridPen, new Math3D.Vector3D(i, 0, -size), new Math3D.Vector3D(i, 0, size));
            }
            // Ejes centrales
            DrawLine3D(g, Pens.Red, new Math3D.Vector3D(-size, 0, 0), new Math3D.Vector3D(size, 0, 0));
            DrawLine3D(g, Pens.Blue, new Math3D.Vector3D(0, 0, -size), new Math3D.Vector3D(0, 0, size));
        }

        private void DrawLine3D(Graphics g, Pen p, Math3D.Vector3D p1, Math3D.Vector3D p2)
        {
            PointF? s = ProjectPoint(p1);
            PointF? e = ProjectPoint(p2);
            if (s.HasValue && e.HasValue) g.DrawLine(p, s.Value, e.Value);
        }

        // Proyecta un punto SOLO para el Grid (Lógica simplificada de la cámara)
        private PointF? ProjectPoint(Math3D.Vector3D p)
        {
            // 1. Mundo -> Vista
            float vx = p.x - camara.Position.x;
            float vy = p.y - camara.Position.y;
            float vz = p.z - camara.Position.z;

            // 2. Rotación Cámara
            Math3D.Vector3D temp = Math3D.RotateY(new Math3D.Vector3D(vx, vy, vz), -camara.Rotation.y);
            temp = Math3D.RotateX(temp, -camara.Rotation.x);

            // 3. Proyección
            if (temp.z > -1) return null; // Detrás de cámara
            float zoom = 600f;
            float zDepth = -temp.z;
            return new PointF((temp.x * zoom) / zDepth + centroPantalla.X, (-temp.y * zoom) / zDepth + centroPantalla.Y);
        }

        private void pictureBox1_Resize(object sender, EventArgs e) { pictureBox1.Invalidate(); }
    }
}