using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
// Tus namespaces
using Motor3D_Educativo_P2.Core;
using Motor3D_Educativo_P2.Geometry;

namespace Motor3D_Educativo_P2
{
    public partial class Form1 : Form
    {
        Transform _transform = new Transform();
        Mesh _mesh;
        bool _loaded = false;

        public Form1()
        {
            InitializeComponent();

            // --- CORRECCIÓN: Conectar eventos AQUÍ, no en Load ---
            glControl1.Load += GlControl1_Load;
            glControl1.Paint += GlControl1_Paint;
            glControl1.Resize += GlControl1_Resize;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Cargar geometría
            _mesh = MeshFactory.CrearCubo();

            // Configurar controles UI
            trackRotX.Scroll += (s, ev) => {
                _transform.Rotation = new Vector3(trackRotX.Value, trackRotY.Value, 0);
                glControl1.Invalidate();
            };
            trackRotY.Scroll += (s, ev) => {
                _transform.Rotation = new Vector3(trackRotX.Value, trackRotY.Value, 0);
                glControl1.Invalidate();
            };

            if (btnCambiar != null)
            {
                btnCambiar.Click += (s, ev) => {
                    _mesh = MeshFactory.CrearEsfera(2.0f, 20, 20);
                    glControl1.Invalidate();
                };
            }

            // Iniciar bucle de redibujado
            Application.Idle += (s, ev) => {
                if (_loaded) glControl1.Invalidate();
            };
        }

        private void GlControl1_Load(object sender, EventArgs e)
        {
            // PRUEBA DE VIDA: Si no ves este mensaje, el GLControl no existe o no está conectado.
            // MessageBox.Show("¡OpenGL ha iniciado correctamente!", "Diagnóstico"); 
            // (Descomenta la línea de arriba si sigues sin ver nada)

            try
            {
                glControl1.MakeCurrent(); // Asegurar contexto
                _loaded = true;

                // Usamos un color ROJO FURIOSO para saber si funciona
                GL.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);

                GL.Enable(EnableCap.DepthTest);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al iniciar OpenGL: " + ex.Message);
            }
        }

        private void GlControl1_Resize(object sender, EventArgs e)
        {
            if (!_loaded) return;
            try
            {
                glControl1.MakeCurrent();
                GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
                Matrix4 p = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), glControl1.Width / (float)glControl1.Height, 0.1f, 100f);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadMatrix(ref p);
            }
            catch { }
        }

        private void GlControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!_loaded) return;

            glControl1.MakeCurrent();

            // 1. Limpiar pantalla (Fondo Naranja)
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // 2. Configurar la Cámara manualmente
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 0, 3), Vector3.Zero, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref view);

            // --- CORRECCIÓN AQUÍ ---
            // Creamos una variable local para poder pasarla por referencia
            Matrix4 identidad = Matrix4.Identity;
            GL.MultMatrix(ref identidad);
            // -----------------------

            // 4. Dibujar triángulo de prueba
            GL.Begin(PrimitiveType.Triangles);

            GL.Color3(Color.Blue);
            GL.Vertex3(-1.0f, -1.0f, 0.0f);

            GL.Color3(Color.White);
            GL.Vertex3(1.0f, -1.0f, 0.0f);

            GL.Color3(Color.Green);
            GL.Vertex3(0.0f, 1.0f, 0.0f);

            GL.End();

            glControl1.SwapBuffers();
        }

        private void glControl1_Load_1(object sender, EventArgs e)
        {

        }
    }
}