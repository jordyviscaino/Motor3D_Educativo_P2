using Motor3D_Educativo_P2.Geometry;
using Motor3D_Educativo_P2.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Motor3D_Educativo_P2
{
    public partial class Form1 : Form
    {
        Escena escena = new Escena();
        Modelo3D modeloSeleccionado;
        Camara camara = new Camara();
        Point centroPantalla;
        Point lastMousePos;
        bool isMouseDown = false;
        bool isDragging = false;
        int figuraIndice = 0;

        private Dictionary<string, int> contadoresObjetos = new Dictionary<string, int>
        {
            { "Cubo", 0 },
            { "Pirámide", 0 },
            { "Cilindro", 0 },
            { "Cono", 0 },
            { "Esfera", 0 }
        };

        private Dictionary<string, TrackBar> slidersTraslacion = new Dictionary<string, TrackBar>();
        private Dictionary<string, TrackBar> slidersRotacion = new Dictionary<string, TrackBar>();
        private Dictionary<string, TrackBar> slidersEscala = new Dictionary<string, TrackBar>();

        private Button btnLightColor;
        private TrackBar trackLightIntensity;

        // Textura seleccionada persistente
        private ComboBox cbTextureSelector;
        private string selectedTextureName = null;
        private Bitmap selectedTextureBitmap = null;

        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            pictureBox1.MouseClick += PictureBox1_MouseClick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var cuboInicial = MeshFactory.CrearCubo(100);
            cuboInicial.Nombre = GenerarNombreUnico("Cubo");
            escena.Modelos.Add(cuboInicial);
            ActualizarListaObjetos();

            // Mejora visual: tema oscuro consistente, fuente y espaciado
            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel1.BackColor = Color.FromArgb(34, 34, 34);
            flowLayoutPanel1.Padding = new Padding(12);
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.AutoScroll = true;

            var header = new Label
            {
                Text = "Motor 3D — Herramientas",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 28,
                Width = 260,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 0, 0, 0)
            };
            flowLayoutPanel1.Controls.Add(header);

            // Grupo: Escena (añadir / eliminar / cambiar tipo)
            var grpScene = new GroupBox
            {
                Text = "Escena",
                Width = 280,
                Height = 120,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(37, 37, 37),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };
            var btnAdd = new Button { Text = "Añadir Figura", Width = 120, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            btnAdd.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 90);
            btnAdd.Click += (s, ev) => AnadirFigura();

            var btnDel = new Button { Text = "Eliminar Seleccionada", Width = 150, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(85, 30, 30), ForeColor = Color.White };
            btnDel.FlatAppearance.BorderColor = Color.FromArgb(120, 50, 50);
            btnDel.Click += (s, ev) => EliminarFiguraSeleccionada();

            var btnChange = new Button { Text = "Cambiar Tipo (Sel)", Width = 120, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            btnChange.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 90);
            btnChange.Click += (s, ev) => CambiarTipoFiguraSeleccionada();

            // Layout simple dentro del groupbox
            grpScene.Controls.Add(btnAdd);
            grpScene.Controls.Add(btnChange);
            grpScene.Controls.Add(btnDel);
            btnAdd.Location = new Point(12, 22);
            btnChange.Location = new Point(12, 62);
            btnDel.Location = new Point(140, 22);
            flowLayoutPanel1.Controls.Add(grpScene);

            // Grupo: Iluminación
            var grpLight = new GroupBox
            {
                Text = "Iluminación",
                Width = 280,
                Height = 120,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(37, 37, 37),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            btnLightColor = new Button { Text = "Color de Luz...", Width = 120, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            btnLightColor.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 90);
            btnLightColor.Click += (s, ev2) =>
            {
                using (ColorDialog cd = new ColorDialog())
                {
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        Scene.LightColor = cd.Color;
                        pictureBox1.Invalidate();
                    }
                }
            };

            var lblLI = new Label { Text = "Intensidad", AutoSize = true, ForeColor = Color.White, Location = new Point(12, 64) };
            trackLightIntensity = new TrackBar { Minimum = 0, Maximum = 200, Value = (int)(Scene.LightIntensity * 100), TickStyle = TickStyle.None, Width = 150, Location = new Point(90, 58) };
            trackLightIntensity.Scroll += (s, ev3) => { Scene.LightIntensity = trackLightIntensity.Value / 100f; pictureBox1.Invalidate(); };

            grpLight.Controls.Add(btnLightColor);
            grpLight.Controls.Add(lblLI);
            grpLight.Controls.Add(trackLightIntensity);
            btnLightColor.Location = new Point(12, 22);
            flowLayoutPanel1.Controls.Add(grpLight);

            // Grupo: Texturas (un único botón consolidado)
            var grpTex = new GroupBox
            {
                Text = "Textura",
                Width = 280,
                Height = 80,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(37, 37, 37),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            var btnCargarTex = new Button { Text = "Cargar Textura...", Width = 240, Height = 36, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Location = new Point(12, 22) };
            btnCargarTex.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 90);
            btnCargarTex.Click += (s, ev) =>
            {
                var bmp = PromptLoadTexture();
                if (bmp != null)
                {
                    SetModelTexture(bmp);
                    pictureBox1.Invalidate();
                }
            };
            grpTex.Controls.Add(btnCargarTex);
            flowLayoutPanel1.Controls.Add(grpTex);

            // Grupo: Controles de cámara / movimiento (ordenados y con botones de acceso rápido)
            var grpCam = new GroupBox
            {
                Text = "Controles Cámara",
                Width = 280,
                Height = 150,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(37, 37, 37),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular)
            };

            var btnResetCam = new Button { Text = "Reset Cámara", Width = 120, Height = 30, Location = new Point(12, 22), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(70, 70, 70), ForeColor = Color.White };
            btnResetCam.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 90);
            btnResetCam.Click += (s, ev) =>
            {
                camara.Position = new Math3D.Vector3D(0, 0, -800);
                camara.Rotation = new Math3D.Vector3D(0, 0, 0);
                pictureBox1.Invalidate();
            };

            // Movimiento: adelante/atrás/izq/der/arriba/abajo
            var btnFwd = new Button { Text = "Frente (W)", Width = 80, Height = 26, Location = new Point(12, 58), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            var btnBack = new Button { Text = "Atrás (S)", Width = 80, Height = 26, Location = new Point(102, 58), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            var btnLeft = new Button { Text = "Izq (A)", Width = 80, Height = 26, Location = new Point(12, 88), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            var btnRight = new Button { Text = "Der (D)", Width = 80, Height = 26, Location = new Point(102, 88), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            var btnUp = new Button { Text = "Subir (Q)", Width = 80, Height = 26, Location = new Point(192, 58), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            var btnDown = new Button { Text = "Bajar (E)", Width = 80, Height = 26, Location = new Point(192, 88), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };

            btnFwd.Click += (s, ev) => { camara.Position.z += camara.Speed; pictureBox1.Invalidate(); };
            btnBack.Click += (s, ev) => { camara.Position.z -= camara.Speed; pictureBox1.Invalidate(); };
            btnLeft.Click += (s, ev) => { camara.Position.x -= camara.Speed; pictureBox1.Invalidate(); };
            btnRight.Click += (s, ev) => { camara.Position.x += camara.Speed; pictureBox1.Invalidate(); };
            btnUp.Click += (s, ev) => { camara.Position.y += camara.Speed; pictureBox1.Invalidate(); };
            btnDown.Click += (s, ev) => { camara.Position.y -= camara.Speed; pictureBox1.Invalidate(); };

            grpCam.Controls.Add(btnResetCam);
            grpCam.Controls.Add(btnFwd);
            grpCam.Controls.Add(btnBack);
            grpCam.Controls.Add(btnLeft);
            grpCam.Controls.Add(btnRight);
            grpCam.Controls.Add(btnUp);
            grpCam.Controls.Add(btnDown);
            flowLayoutPanel1.Controls.Add(grpCam);

            // Sliders (traslación, rotación, escala)
            GenerarSliders("TRASLACIÓN", -200, 200, 0, slidersTraslacion, (c, v) =>
            {
                if (modeloSeleccionado == null) return;
                if (c == 'X') modeloSeleccionado.Position.x = v;
                if (c == 'Y') modeloSeleccionado.Position.y = v;
                if (c == 'Z') modeloSeleccionado.Position.z = v;
                pictureBox1.Invalidate();
            });

            GenerarSliders("ROTACIÓN", 0, 360, 0, slidersRotacion, (c, v) =>
            {
                if (modeloSeleccionado == null) return;
                if (c == 'X') modeloSeleccionado.Rotation.x = v;
                if (c == 'Y') modeloSeleccionado.Rotation.y = v;
                if (c == 'Z') modeloSeleccionado.Rotation.z = v;
                pictureBox1.Invalidate();
            });

            GenerarSliders("ESCALA", 1, 30, 10, slidersEscala, (c, v) =>
            {
                if (modeloSeleccionado == null) return;
                float s = v / 10.0f;
                if (c == 'X') modeloSeleccionado.Scale.x = s;
                if (c == 'Y') modeloSeleccionado.Scale.y = s;
                if (c == 'Z') modeloSeleccionado.Scale.z = s;
                pictureBox1.Invalidate();
            });

            // Seleccionar cubo inicial
            SeleccionarModelo(cuboInicial);
        }

        private string GenerarNombreUnico(string tipoBase)
        {
            contadoresObjetos[tipoBase]++;
            return $"{tipoBase} {contadoresObjetos[tipoBase]}";
        }

        private string ObtenerIconoObjeto(string nombre)
        {
            if (nombre.Contains("Cubo")) return "🔳";
            if (nombre.Contains("Pirámide")) return "🔺";
            if (nombre.Contains("Cilindro")) return "⬭";
            if (nombre.Contains("Cono")) return "🔻";
            if (nombre.Contains("Esfera")) return "⚫";
            return "📦";
        }

        private void ActualizarListaObjetos()
        {
            listBoxObjetos.Items.Clear();
            foreach (var modelo in escena.Modelos)
            {
                string itemText = $"  {ObtenerIconoObjeto(modelo.Nombre)} {modelo.Nombre}";
                listBoxObjetos.Items.Add(itemText);
            }

            if (modeloSeleccionado != null)
            {
                int index = escena.Modelos.IndexOf(modeloSeleccionado);
                if (index >= 0 && index < listBoxObjetos.Items.Count)
                {
                    listBoxObjetos.SelectedIndex = index;
                }
            }
        }

        private void listBoxObjetos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxObjetos.SelectedIndex >= 0 && listBoxObjetos.SelectedIndex < escena.Modelos.Count)
            {
                SeleccionarModelo(escena.Modelos[listBoxObjetos.SelectedIndex]);
            }
        }

        void GenerarSliders(string t, int min, int max, int def, Dictionary<string, TrackBar> diccionarioSliders, Action<char, int> act)
        {
            GroupBox g = new GroupBox();
            g.Text = t;
            g.Size = new Size(260, 150);
            g.ForeColor = Color.White;
            g.BackColor = Color.FromArgb(37, 37, 37);
            int y = 20;
            foreach (char c in new[] { 'X', 'Y', 'Z' })
            {
                Label l = new Label();
                l.Text = c.ToString();
                l.Location = new Point(10, y);
                l.ForeColor = Color.White;
                l.BackColor = Color.Transparent;
                l.AutoSize = true;

                TrackBar tb = new TrackBar();
                tb.Location = new Point(40, y - 6);
                tb.Size = new Size(200, 30);
                tb.Minimum = min;
                tb.Maximum = max;
                tb.Value = def;
                tb.TickStyle = TickStyle.None;
                tb.Scroll += (s, e) => act(c, tb.Value);

                diccionarioSliders[c.ToString()] = tb;

                g.Controls.Add(l);
                g.Controls.Add(tb);
                y += 40;
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
                ActualizarSliders();
            }

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

        void ActualizarSliders()
        {
            if (modeloSeleccionado == null) return;

            if (slidersTraslacion.ContainsKey("X"))
                slidersTraslacion["X"].Value = (int)Math.Max(slidersTraslacion["X"].Minimum, Math.Min(slidersTraslacion["X"].Maximum, modeloSeleccionado.Position.x));
            if (slidersTraslacion.ContainsKey("Y"))
                slidersTraslacion["Y"].Value = (int)Math.Max(slidersTraslacion["Y"].Minimum, Math.Min(slidersTraslacion["Y"].Maximum, modeloSeleccionado.Position.y));
            if (slidersTraslacion.ContainsKey("Z"))
                slidersTraslacion["Z"].Value = (int)Math.Max(slidersTraslacion["Z"].Minimum, Math.Min(slidersTraslacion["Z"].Maximum, modeloSeleccionado.Position.z));

            if (slidersRotacion.ContainsKey("X"))
                slidersRotacion["X"].Value = (int)Math.Max(slidersRotacion["X"].Minimum, Math.Min(slidersRotacion["X"].Maximum, modeloSeleccionado.Rotation.x));
            if (slidersRotacion.ContainsKey("Y"))
                slidersRotacion["Y"].Value = (int)Math.Max(slidersRotacion["Y"].Minimum, Math.Min(slidersRotacion["Y"].Maximum, modeloSeleccionado.Rotation.y));
            if (slidersRotacion.ContainsKey("Z"))
                slidersRotacion["Z"].Value = (int)Math.Max(slidersRotacion["Z"].Minimum, Math.Min(slidersRotacion["Z"].Maximum, modeloSeleccionado.Rotation.z));

            if (slidersEscala.ContainsKey("X"))
                slidersEscala["X"].Value = (int)Math.Max(slidersEscala["X"].Minimum, Math.Min(slidersEscala["X"].Maximum, modeloSeleccionado.Scale.x * 10));
            if (slidersEscala.ContainsKey("Y"))
                slidersEscala["Y"].Value = (int)Math.Max(slidersEscala["Y"].Minimum, Math.Min(slidersEscala["Y"].Maximum, modeloSeleccionado.Scale.y * 10));
            if (slidersEscala.ContainsKey("Z"))
                slidersEscala["Z"].Value = (int)Math.Max(slidersEscala["Z"].Minimum, Math.Min(slidersEscala["Z"].Maximum, modeloSeleccionado.Scale.z * 10));
        }

        void AnadirFigura()
        {
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

            nuevoModelo.Nombre = GenerarNombreUnico(tipoNovo);

            nuevoModelo.Position = pos;
            nuevoModelo.Rotation = rot;
            nuevoModelo.Scale = scl;
            nuevoModelo.Selected = true;

            int index = escena.Modelos.IndexOf(modeloSeleccionado);
            if (index != -1)
            {
                escena.Modelos[index] = nuevoModelo;
                modeloSeleccionado = nuevoModelo;
            }

            ActualizarListaObjetos();
            pictureBox1.Invalidate();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            float s = camara.Speed;
            switch (e.KeyCode)
            {
                case Keys.W: camara.Position.z += s; break;
                case Keys.S: camara.Position.z -= s; break;
                case Keys.A: camara.Position.x -= s; break;
                case Keys.D: camara.Position.x += s; break;
                case Keys.Q: camara.Position.y += s; break;
                case Keys.E: camara.Position.y -= s; break;
            }
            pictureBox1.Invalidate();
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
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

        private void SeleccionarObjetoPorMouse(Point mousePos)
        {
            centroPantalla = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);

            Modelo3D objetoSeleccionado = null;
            float menorDistancia = float.MaxValue;

            var modelosOrdenados = escena.Modelos.OrderByDescending(m =>
            {
                return Math.Sqrt(Math.Pow(m.Position.x - camara.Position.x, 2) +
                                 Math.Pow(m.Position.y - camara.Position.y, 2) +
                                 Math.Pow(m.Position.z - camara.Position.z, 2));
            }).ToList();

            foreach (var modelo in modelosOrdenados)
            {
                foreach (var face in modelo.Faces)
                {
                    PointF[] puntosProyectados = new PointF[face.Corners3D.Length];
                    bool visible = true;
                    float profundidadPromedio = 0;

                    for (int i = 0; i < face.Corners3D.Length; i++)
                    {
                        Math3D.Vector3D worldPoint = AplicarTransformacion(face.Corners3D[i], modelo);

                        Math3D.Vector3D viewPoint = new Math3D.Vector3D(
                            worldPoint.x - camara.Position.x,
                            worldPoint.y - camara.Position.y,
                            worldPoint.z - camara.Position.z
                        );

                        viewPoint = RotarPunto(viewPoint, -camara.Rotation.x, -camara.Rotation.y, 0);

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

                        if (PuntoEnPoligono(mousePos, puntosProyectados))
                        {
                            if (profundidadPromedio < menorDistancia)
                            {
                                menorDistancia = profundidadPromedio;
                                objetoSeleccionado = modelo;
                            }
                        }
                    }
                }

                if (objetoSeleccionado != null) break;
            }

            if (objetoSeleccionado != null)
            {
                SeleccionarModelo(objetoSeleccionado);
            }
        }

        private Math3D.Vector3D AplicarTransformacion(Math3D.Vector3D vertice, Modelo3D modelo)
        {
            float vx = vertice.x * modelo.Scale.x;
            float vy = vertice.y * modelo.Scale.y;
            float vz = vertice.z * modelo.Scale.z;

            Math3D.Vector3D rotado = RotarPunto(new Math3D.Vector3D(vx, vy, vz),
                                                 modelo.Rotation.x, modelo.Rotation.y, modelo.Rotation.z);

            rotado.x += modelo.Position.x;
            rotado.y += modelo.Position.y;
            rotado.z += modelo.Position.z;

            return rotado;
        }

        private Math3D.Vector3D RotarPunto(Math3D.Vector3D punto, float rx, float ry, float rz)
        {
            Math3D.Vector3D resultado = punto;
            if (rx != 0) resultado = Math3D.RotateX(resultado, rx);
            if (ry != 0) resultado = Math3D.RotateY(resultado, ry);
            if (rz != 0) resultado = Math3D.RotateZ(resultado, rz);
            return resultado;
        }

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

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.FromArgb(20, 20, 20));
            centroPantalla = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);

            DrawGrid(e.Graphics);

            escena.Draw(e.Graphics, centroPantalla, camara);
        }

        private void DrawGrid(Graphics g)
        {
            int size = 2000; int step = 200;
            Pen gridPen = new Pen(Color.FromArgb(60, 60, 60));

            for (int i = -size; i <= size; i += step)
            {
                DrawLine3D(g, gridPen, new Math3D.Vector3D(-size, 0, i), new Math3D.Vector3D(size, 0, i));
                DrawLine3D(g, gridPen, new Math3D.Vector3D(i, 0, -size), new Math3D.Vector3D(i, 0, size));
            }
            DrawLine3D(g, Pens.Red, new Math3D.Vector3D(-size, 0, 0), new Math3D.Vector3D(size, 0, 0));
            DrawLine3D(g, Pens.Blue, new Math3D.Vector3D(0, 0, -size), new Math3D.Vector3D(0, 0, size));
        }

        private void DrawLine3D(Graphics g, Pen p, Math3D.Vector3D p1, Math3D.Vector3D p2)
        {
            PointF? s = ProjectPoint(p1);
            PointF? e = ProjectPoint(p2);
            if (s.HasValue && e.HasValue) g.DrawLine(p, s.Value, e.Value);
        }

        private PointF? ProjectPoint(Math3D.Vector3D p)
        {
            float vx = p.x - camara.Position.x;
            float vy = p.y - camara.Position.y;
            float vz = p.z - camara.Position.z;

            Math3D.Vector3D temp = Math3D.RotateY(new Math3D.Vector3D(vx, vy, vz), -camara.Rotation.y);
            temp = Math3D.RotateX(temp, -camara.Rotation.x);

            if (temp.z > -1) return null;
            float zoom = 600f;
            float zDepth = -temp.z;
            return new PointF((temp.x * zoom) / zDepth + centroPantalla.X, (-temp.y * zoom) / zDepth + centroPantalla.Y);
        }

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

        private Bitmap LoadBitmapNoLock(string path)
        {
            using (var fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                return new Bitmap(fs);
            }
        }

        private void SetModelTexture(Bitmap bmp)
        {
            if (modeloSeleccionado == null) return;
            if (modeloSeleccionado.Material == null) modeloSeleccionado.Material = new Material(Color.LightGray);

            var prev = modeloSeleccionado.Material.Texture;
            modeloSeleccionado.Material.Texture = bmp;
            prev?.Dispose();
        }

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