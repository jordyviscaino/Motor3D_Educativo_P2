using Motor3D_Educativo_P2.Geometry;
using System;
using System.Collections.Generic; // Added for List<> 
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Motor3D_Educativo_P2
{
    public partial class Form1 : Form
    {
        Escena escena = new Escena(); // Reemplaza modeloActual único por una escena
        Modelo3D modeloSeleccionado; // Referencia al modelo actualmente seleccionado para editar
        Camara camara = new Camara(); // Instancia de la cámara
        Point centroPantalla;
        Point lastMousePos;
        bool isMouseDown = false;
        int figuraIndice = 0;
        
        // Contadores para nombres únicos de objetos
        private Dictionary<string, int> contadoresObjetos = new Dictionary<string, int>
        {
            { "Cubo", 0 },
            { "Pirámide", 0 },
            { "Cilindro", 0 },
            { "Cono", 0 },
            { "Esfera", 0 }
        };

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
            // Cargar cubo por defecto y añadirlo a la escena
            var cuboInicial = MeshFactory.CrearCubo(100);
            cuboInicial.Nombre = GenerarNombreUnico("Cubo");
            escena.Modelos.Add(cuboInicial);
            ActualizarListaObjetos();
            SeleccionarModelo(cuboInicial);

            // -- GENERACIÓN DE UI --
            flowLayoutPanel1.Controls.Clear();
            
            // Botón para añadir nueva figura
            Button btnAdd = new Button(); 
            btnAdd.Text = "Añadir Figura"; 
            btnAdd.Height = 40; 
            btnAdd.Width = 200;
            btnAdd.BackColor = Color.FromArgb(60, 60, 60);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btnAdd.Click += (s, ev) => AnadirFigura();
            flowLayoutPanel1.Controls.Add(btnAdd);

            // Botón para cambiar tipo de figura seleccionada (opcional, o para ciclar tipos en la nueva)
            Button btnChange = new Button(); 
            btnChange.Text = "Cambiar Tipo (Sel)"; 
            btnChange.Height = 40; 
            btnChange.Width = 200;
            btnChange.BackColor = Color.FromArgb(60, 60, 60);
            btnChange.ForeColor = Color.White;
            btnChange.FlatStyle = FlatStyle.Flat;
            btnChange.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btnChange.Click += (s, ev) => CambiarTipoFiguraSeleccionada();
            flowLayoutPanel1.Controls.Add(btnChange);

            // Botón para eliminar figura seleccionada
            Button btnDel = new Button(); 
            btnDel.Text = "Eliminar Seleccionada"; 
            btnDel.Height = 40; 
            btnDel.Width = 200;
            btnDel.BackColor = Color.FromArgb(80, 30, 30); // Rojo oscuro para eliminar
            btnDel.ForeColor = Color.White;
            btnDel.FlatStyle = FlatStyle.Flat;
            btnDel.FlatAppearance.BorderColor = Color.FromArgb(120, 50, 50);
            btnDel.Click += (s, ev) => EliminarFiguraSeleccionada();
            flowLayoutPanel1.Controls.Add(btnDel);

            GenerarSliders("TRASLACIÓN", -200, 200, 0, (c, v) => {
                if (modeloSeleccionado == null) return;
                if (c == 'X') modeloSeleccionado.Position.x = v; if (c == 'Y') modeloSeleccionado.Position.y = v; if (c == 'Z') modeloSeleccionado.Position.z = v;
                pictureBox1.Invalidate();
            });
            GenerarSliders("ROTACIÓN", 0, 360, 0, (c, v) => {
                if (modeloSeleccionado == null) return;
                if (c == 'X') modeloSeleccionado.Rotation.x = v; if (c == 'Y') modeloSeleccionado.Rotation.y = v; if (c == 'Z') modeloSeleccionado.Rotation.z = v;
                pictureBox1.Invalidate();
            });
            GenerarSliders("ESCALA", 1, 30, 10, (c, v) => {
                if (modeloSeleccionado == null) return;
                float s = v / 10.0f;
                if (c == 'X') modeloSeleccionado.Scale.x = s; if (c == 'Y') modeloSeleccionado.Scale.y = s; if (c == 'Z') modeloSeleccionado.Scale.z = s;
                pictureBox1.Invalidate();
            });
        }

        // Genera un nombre único para el objeto (ej: "Cubo 1", "Cubo 2")
        private string GenerarNombreUnico(string tipoBase)
        {
            contadoresObjetos[tipoBase]++;
            return $"{tipoBase} {contadoresObjetos[tipoBase]}";
        }

        // Obtiene el icono visual para cada tipo de objeto (similar a Blender)
        private string ObtenerIconoObjeto(string nombre)
        {
            if (nombre.Contains("Cubo")) return "🔳";
            if (nombre.Contains("Pirámide")) return "🔺";
            if (nombre.Contains("Cilindro")) return "⬭";
            if (nombre.Contains("Cono")) return "🔻";
            if (nombre.Contains("Esfera")) return "⚫";
            return "📦";
        }

        // Actualiza el ListBox con todos los objetos de la escena
        private void ActualizarListaObjetos()
        {
            listBoxObjetos.Items.Clear();
            foreach (var modelo in escena.Modelos)
            {
                // Agregar icono visual antes del nombre
                string itemText = $"  {ObtenerIconoObjeto(modelo.Nombre)} {modelo.Nombre}";
                listBoxObjetos.Items.Add(itemText);
            }
            
            // Seleccionar el item correspondiente al modelo seleccionado
            if (modeloSeleccionado != null)
            {
                int index = escena.Modelos.IndexOf(modeloSeleccionado);
                if (index >= 0 && index < listBoxObjetos.Items.Count)
                {
                    listBoxObjetos.SelectedIndex = index;
                }
            }
        }

        // Evento cuando se selecciona un objeto en el ListBox
        private void listBoxObjetos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxObjetos.SelectedIndex >= 0 && listBoxObjetos.SelectedIndex < escena.Modelos.Count)
            {
                SeleccionarModelo(escena.Modelos[listBoxObjetos.SelectedIndex]);
            }
        }

        // Helper para UI (simplificado)
        void GenerarSliders(string t, int min, int max, int def, Action<char, int> act)
        {
            GroupBox g = new GroupBox(); 
            g.Text = t; 
            g.Size = new Size(220, 160); 
            g.ForeColor = Color.White;
            g.BackColor = Color.FromArgb(37, 37, 37); // Fondo oscuro para el GroupBox
            int y = 20;
            foreach (char c in new[] { 'X', 'Y', 'Z' })
            {
                Label l = new Label(); 
                l.Text = c.ToString(); 
                l.Location = new Point(10, y); 
                l.ForeColor = Color.White; // Color blanco para el texto
                l.BackColor = Color.Transparent; // Fondo transparente
                l.AutoSize = true;
                
                TrackBar tb = new TrackBar(); 
                tb.Location = new Point(30, y); 
                tb.Size = new Size(180, 30); 
                tb.Minimum = min; 
                tb.Maximum = max; 
                tb.Value = def; 
                tb.TickStyle = TickStyle.None;
                tb.Scroll += (s, e) => act(c, tb.Value);
                
                g.Controls.Add(l); 
                g.Controls.Add(tb); 
                y += 45;
            }
            flowLayoutPanel1.Controls.Add(g);
        }

        void SeleccionarModelo(Modelo3D m)
        {
            if (modeloSeleccionado != null) modeloSeleccionado.Selected = false;
            modeloSeleccionado = m;
            if (modeloSeleccionado != null) modeloSeleccionado.Selected = true;
            
            // Actualizar selección en el ListBox sin disparar el evento
            if (m != null)
            {
                int index = escena.Modelos.IndexOf(m);
                if (index >= 0 && listBoxObjetos.SelectedIndex != index)
                {
                    listBoxObjetos.SelectedIndex = index;
                }
            }
            
            pictureBox1.Invalidate();
        }

        void AnadirFigura()
        {
            // Añade un cubo por defecto en una posición ligeramente aleatoria o fija
            var m = MeshFactory.CrearCubo(100);
            m.Nombre = GenerarNombreUnico("Cubo");
            m.Position = new Math3D.Vector3D(0, 0, 0);
            escena.Modelos.Add(m);
            ActualizarListaObjetos();
            SeleccionarModelo(m);
        }

        void EliminarFiguraSeleccionada()
        {
            if (modeloSeleccionado != null)
            {
                escena.Modelos.Remove(modeloSeleccionado);
                if (escena.Modelos.Count > 0) SeleccionarModelo(escena.Modelos[escena.Modelos.Count - 1]);
                else SeleccionarModelo(null);
                ActualizarListaObjetos();
            }
        }

        void SeleccionarSiguiente()
        {
            if (escena.Modelos.Count == 0) return;
            int index = escena.Modelos.IndexOf(modeloSeleccionado);
            index++;
            if (index >= escena.Modelos.Count) index = 0;
            SeleccionarModelo(escena.Modelos[index]);
        }

        void CambiarTipoFiguraSeleccionada()
        {
            if (modeloSeleccionado == null) return;
            
            // Guardar transformaciones actuales y nombre base
            var pos = modeloSeleccionado.Position;
            var rot = modeloSeleccionado.Rotation;
            var scl = modeloSeleccionado.Scale;

            figuraIndice++; if (figuraIndice > 4) figuraIndice = 0;
            
            Modelo3D nuevoModelo = null;
            string tipoNovo = "";
            
            switch (figuraIndice)
            {
                case 0: nuevoModelo = MeshFactory.CrearCubo(100); tipoNovo = "Cubo"; break;
                case 1: nuevoModelo = MeshFactory.CrearPiramide(100, 150); tipoNovo = "Pirámide"; break;
                case 2: nuevoModelo = MeshFactory.CrearCilindro(60, 150, 16); tipoNovo = "Cilindro"; break;
                case 3: nuevoModelo = MeshFactory.CrearCono(60, 150, 16); tipoNovo = "Cono"; break;
                case 4: nuevoModelo = MeshFactory.CrearEsfera(80, 12, 12); tipoNovo = "Esfera"; break;
            }

            // Asignar nuevo nombre único
            nuevoModelo.Nombre = GenerarNombreUnico(tipoNovo);
            
            // Restaurar transformaciones
            nuevoModelo.Position = pos;
            nuevoModelo.Rotation = rot;
            nuevoModelo.Scale = scl;
            nuevoModelo.Selected = true;

            // Reemplazar en la lista
            int index = escena.Modelos.IndexOf(modeloSeleccionado);
            if (index != -1)
            {
                escena.Modelos[index] = nuevoModelo;
                modeloSeleccionado = nuevoModelo;
            }
            
            ActualizarListaObjetos();
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

            // 2. Dibujar la Escena (todos los modelos)
            escena.Draw(e.Graphics, centroPantalla, camara);
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