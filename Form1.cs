using Motor3D_Educativo_P2.Geometry;
using System;
using System.Collections.Generic; // Added for List<> 
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq; // Necesario para OrderByDescending
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
        bool isDragging = false; // Para diferenciar entre clic y arrastre
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
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            pictureBox1.MouseClick += PictureBox1_MouseClick; // Nuevo evento para selección
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Cargar cubo por defecto y añadirlo a la escena
            var cuboInicial = MeshFactory.CrearCubo(100);
            cuboInicial.Nombre = GenerarNombreUnico("Cubo");
            escena.Modelos.Add(cuboInicial);
            ActualizarListaObjetos();

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

            GenerarSliders("TRASLACIÓN", -200, 200, 0, slidersTraslacion, (c, v) => {
                if (modeloSeleccionado == null) return;
                if (c == 'X') modeloSeleccionado.Position.x = v; 
                if (c == 'Y') modeloSeleccionado.Position.y = v; 
                if (c == 'Z') modeloSeleccionado.Position.z = v;
                pictureBox1.Invalidate();
            });
            GenerarSliders("ROTACIÓN", 0, 360, 0, slidersRotacion, (c, v) => {
                if (modeloSeleccionado == null) return;
                if (c == 'X') modeloSeleccionado.Rotation.x = v; 
                if (c == 'Y') modeloSeleccionado.Rotation.y = v; 
                if (c == 'Z') modeloSeleccionado.Rotation.z = v;
                pictureBox1.Invalidate();
            });
            GenerarSliders("ESCALA", 1, 30, 10, slidersEscala, (c, v) => {
                if (modeloSeleccionado == null) return;
                float s = v / 10.0f;
                if (c == 'X') modeloSeleccionado.Scale.x = s; 
                if (c == 'Y') modeloSeleccionado.Scale.y = s; 
                if (c == 'Z') modeloSeleccionado.Scale.z = s;
                pictureBox1.Invalidate();
            });

            // Seleccionar el cubo inicial después de crear los sliders
            SeleccionarModelo(cuboInicial);
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

        // Helper para UI (modificado para almacenar referencias a los sliders)
        void GenerarSliders(string t, int min, int max, int def, Dictionary<string, TrackBar> diccionarioSliders, Action<char, int> act)
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
                
                // Almacenar referencia al TrackBar
                diccionarioSliders[c.ToString()] = tb;
                
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
            if (modeloSeleccionado != null) 
            {
                modeloSeleccionado.Selected = true;
                
                // Actualizar los sliders con los valores del modelo seleccionado
                ActualizarSliders();
            }
            
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

        /// <summary>
        /// Actualiza los valores de los sliders según el modelo seleccionado
        /// </summary>
        void ActualizarSliders()
        {
            if (modeloSeleccionado == null) return;

            // Actualizar Traslación
            if (slidersTraslacion.ContainsKey("X"))
                slidersTraslacion["X"].Value = (int)Math.Max(slidersTraslacion["X"].Minimum, Math.Min(slidersTraslacion["X"].Maximum, modeloSeleccionado.Position.x));
            if (slidersTraslacion.ContainsKey("Y"))
                slidersTraslacion["Y"].Value = (int)Math.Max(slidersTraslacion["Y"].Minimum, Math.Min(slidersTraslacion["Y"].Maximum, modeloSeleccionado.Position.y));
            if (slidersTraslacion.ContainsKey("Z"))
                slidersTraslacion["Z"].Value = (int)Math.Max(slidersTraslacion["Z"].Minimum, Math.Min(slidersTraslacion["Z"].Maximum, modeloSeleccionado.Position.z));

            // Actualizar Rotación
            if (slidersRotacion.ContainsKey("X"))
                slidersRotacion["X"].Value = (int)Math.Max(slidersRotacion["X"].Minimum, Math.Min(slidersRotacion["X"].Maximum, modeloSeleccionado.Rotation.x));
            if (slidersRotacion.ContainsKey("Y"))
                slidersRotacion["Y"].Value = (int)Math.Max(slidersRotacion["Y"].Minimum, Math.Min(slidersRotacion["Y"].Maximum, modeloSeleccionado.Rotation.y));
            if (slidersRotacion.ContainsKey("Z"))
                slidersRotacion["Z"].Value = (int)Math.Max(slidersRotacion["Z"].Minimum, Math.Min(slidersRotacion["Z"].Maximum, modeloSeleccionado.Rotation.z));

            // Actualizar Escala (multiplicar por 10 porque los sliders van de 1 a 30)
            if (slidersEscala.ContainsKey("X"))
                slidersEscala["X"].Value = (int)Math.Max(slidersEscala["X"].Minimum, Math.Min(slidersEscala["X"].Maximum, modeloSeleccionado.Scale.x * 10));
            if (slidersEscala.ContainsKey("Y"))
                slidersEscala["Y"].Value = (int)Math.Max(slidersEscala["Y"].Minimum, Math.Min(slidersEscala["Y"].Maximum, modeloSeleccionado.Scale.y * 10));
            if (slidersEscala.ContainsKey("Z"))
                slidersEscala["Z"].Value = (int)Math.Max(slidersEscala["Z"].Minimum, Math.Min(slidersEscala["Z"].Maximum, modeloSeleccionado.Scale.z * 10));
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

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            // Solo seleccionar si no fue un arrastre
            if (!isDragging && e.Button == MouseButtons.Left)
            {
                SeleccionarObjetoPorMouse(e.Location);
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDown = true;
            isDragging = false;
            lastMousePos = e.Location;
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                // Si se mueve el mouse mientras está presionado, es un arrastre
                if (Math.Abs(e.X - lastMousePos.X) > 3 || Math.Abs(e.Y - lastMousePos.Y) > 3)
                {
                    isDragging = true;
                }

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

        /// <summary>
        /// Selecciona un objeto en la escena basándose en la posición del mouse
        /// </summary>
        private void SeleccionarObjetoPorMouse(Point mousePos)
        {
            centroPantalla = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            
            // Buscar el objeto más cercano que intersecte con el punto del mouse
            Modelo3D objetoSeleccionado = null;
            float menorDistancia = float.MaxValue;

            // Recorrer todos los modelos de atrás hacia adelante (ordenados por profundidad)
            var modelosOrdenados = escena.Modelos.OrderByDescending(m => {
                return Math.Sqrt(Math.Pow(m.Position.x - camara.Position.x, 2) +
                                 Math.Pow(m.Position.y - camara.Position.y, 2) +
                                 Math.Pow(m.Position.z - camara.Position.z, 2));
            }).ToList();

            foreach (var modelo in modelosOrdenados)
            {
                // Proyectar todas las caras del modelo
                foreach (var face in modelo.Faces)
                {
                    PointF[] puntosProyectados = new PointF[face.Corners3D.Length];
                    bool visible = true;
                    float profundidadPromedio = 0;

                    for (int i = 0; i < face.Corners3D.Length; i++)
                    {
                        // Aplicar las mismas transformaciones que en el método Draw
                        Math3D.Vector3D worldPoint = AplicarTransformacion(face.Corners3D[i], modelo);
                        
                        // Transformación de cámara
                        Math3D.Vector3D viewPoint = new Math3D.Vector3D(
                            worldPoint.x - camara.Position.x,
                            worldPoint.y - camara.Position.y,
                            worldPoint.z - camara.Position.z
                        );

                        // Rotar según la cámara
                        viewPoint = RotarPunto(viewPoint, -camara.Rotation.x, -camara.Rotation.y, 0);

                        // Proyección
                        float zoom = 600f;

                        if (viewPoint.z > -1)
                        {
                            visible = false;
                            break;
                        }

                        float zDepth = -viewPoint.z;
                        profundidadPromedio += zDepth;
                        float x2d = (viewPoint.x * zoom) / zDepth + centroPantalla.X;
                        float y2d = (-viewPoint.y * zoom) / zDepth + centroPantalla.Y;

                        puntosProyectados[i] = new PointF(x2d, y2d);
                    }

                    if (visible && puntosProyectados.Length >= 3)
                    {
                        profundidadPromedio /= puntosProyectados.Length;

                        // Verificar si el punto del mouse está dentro del polígono proyectado
                        if (PuntoEnPoligono(mousePos, puntosProyectados))
                        {
                            // Seleccionar el objeto más cercano a la cámara
                            if (profundidadPromedio < menorDistancia)
                            {
                                menorDistancia = profundidadPromedio;
                                objetoSeleccionado = modelo;
                            }
                        }
                    }
                }

                // Si ya encontramos un objeto, detener la búsqueda (el primero visible es el más cercano)
                if (objetoSeleccionado != null)
                {
                    break;
                }
            }

            // Seleccionar el objeto encontrado
            if (objetoSeleccionado != null)
            {
                SeleccionarModelo(objetoSeleccionado);
            }
        }

        /// <summary>
        /// Aplica las transformaciones de escala, rotación y traslación a un vértice
        /// </summary>
        private Math3D.Vector3D AplicarTransformacion(Math3D.Vector3D vertice, Modelo3D modelo)
        {
            // 1. Escala
            float vx = vertice.x * modelo.Scale.x;
            float vy = vertice.y * modelo.Scale.y;
            float vz = vertice.z * modelo.Scale.z;

            // 2. Rotación
            Math3D.Vector3D rotado = RotarPunto(new Math3D.Vector3D(vx, vy, vz), 
                                                 modelo.Rotation.x, modelo.Rotation.y, modelo.Rotation.z);

            // 3. Traslación
            rotado.x += modelo.Position.x;
            rotado.y += modelo.Position.y;
            rotado.z += modelo.Position.z;

            return rotado;
        }

        /// <summary>
        /// Rota un punto en el espacio 3D
        /// </summary>
        private Math3D.Vector3D RotarPunto(Math3D.Vector3D punto, float rx, float ry, float rz)
        {
            Math3D.Vector3D resultado = punto;
            if (rx != 0) resultado = Math3D.RotateX(resultado, rx);
            if (ry != 0) resultado = Math3D.RotateY(resultado, ry);
            if (rz != 0) resultado = Math3D.RotateZ(resultado, rz);
            return resultado;
        }

        /// <summary>
        /// Determina si un punto está dentro de un polígono usando el algoritmo Ray Casting
        /// </summary>
        private bool PuntoEnPoligono(Point punto, PointF[] poligono)
        {
            bool dentro = false;
            int j = poligono.Length - 1;

            for (int i = 0; i < poligono.Length; i++)
            {
                if ((poligono[i].Y > punto.Y) != (poligono[j].Y > punto.Y) &&
                    punto.X < (poligono[j].X - poligono[i].X) * (punto.Y - poligono[i].Y) / 
                    (poligono[j].Y - poligono[i].Y) + poligono[i].X)
                {
                    dentro = !dentro;
                }
                j = i;
            }

            return dentro;
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

        // --- FUNCIONES DE CARGA DE TEXTURAS (solo imágenes desde disco) ---

        // Abre OpenFileDialog y carga bitmap sin bloquear el archivo. Devuelve null si no se cargó.
        private Bitmap PromptLoadTexture(string title = "Selecciona imagen de textura")
        {
            using (OpenFileDialog of = new OpenFileDialog())
            {
                of.Title = title;
                of.Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp";
                if (of.ShowDialog() != DialogResult.OK) return null;
                try
                {
                    var bmp = LoadBitmapNoLock(of.FileName);
                    // Opcional: reescalar si es demasiado grande para mantener rendimiento
                    const int maxDim = 1024;
                    if (bmp.Width > maxDim || bmp.Height > maxDim)
                    {
                        var resized = ResizeBitmapProportional(bmp, maxDim, maxDim);
                        bmp.Dispose();
                        return resized;
                    }
                    return bmp;
                }
                catch
                {
                    MessageBox.Show("No se pudo cargar la imagen.");
                    return null;
                }
            }
        }

        // Carga un Bitmap sin mantener el archivo bloqueado
        private Bitmap LoadBitmapNoLock(string path)
        {
            using (var fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                return new Bitmap(fs);
            }
        }

        // Asigna la textura al material del modelo liberando la anterior
        private void SetModelTexture(Bitmap bmp)
        {
            if (modeloActual == null) return;
            if (modeloActual.Material == null) modeloActual.Material = new Material(Color.LightGray);

            var prev = modeloActual.Material.Texture;
            modeloActual.Material.Texture = bmp;
            prev?.Dispose();
        }

        // Reescala manteniendo aspecto
        private Bitmap ResizeBitmapProportional(Bitmap src, int maxWidth, int maxHeight)
        {
            float ratio = Math.Min((float)maxWidth / src.Width, (float)maxHeight / src.Height);
            int w = Math.Max(1, (int)(src.Width * ratio));
            int h = Math.Max(1, (int)(src.Height * ratio));
            Bitmap dst = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(dst))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(src, 0, 0, w, h);
            }
            return dst;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {   

        }
    }
}