using Motor3D_Educativo_P2.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Motor3D_Educativo_P2
{
    // Clase base para objetos 3D
    public class Modelo3D
    {
        public string Nombre { get; set; } = "Objeto"; // Nombre para mostrar en la lista
        public List<Math3D.Face> Faces = new List<Math3D.Face>();
        public Math3D.Vector3D Position = new Math3D.Vector3D(0, 0, 0);
        public Math3D.Vector3D Rotation = new Math3D.Vector3D(0, 0, 0);
        public Math3D.Vector3D Scale = new Math3D.Vector3D(1, 1, 1);
        public bool Selected = false; // Propiedad para indicar si el modelo está seleccionado

        // --- MÉTODO DRAW CORREGIDO ---
        public void Draw(Graphics g, Point viewCenter, Camara cam)
        {
            if (Faces.Count == 0) return;

            // Ordenamiento (Z-Sorting) para que las caras lejanas se dibujen primero
            var sortedFaces = Faces.OrderByDescending(f => {
                var c = f.Center;
                // Transformamos el centro para saber su profundidad real
                var worldPos = ApplyTransform(c, Position, Rotation, Scale);
                // Distancia a la cámara
                return Math.Sqrt(Math.Pow(worldPos.x - cam.Position.x, 2) +
                                 Math.Pow(worldPos.y - cam.Position.y, 2) +
                                 Math.Pow(worldPos.z - cam.Position.z, 2));
            }).ToList();

            foreach (var face in sortedFaces)
            {
                face.Corners2D = new PointF[face.Corners3D.Length];
                bool isVisible = true;

                for (int i = 0; i < face.Corners3D.Length; i++)
                {
                    // 1. Transformación de Modelo (Local -> Mundo)
                    Math3D.Vector3D worldPoint = ApplyTransform(face.Corners3D[i], Position, Rotation, Scale);

                    // 2. Transformación de Cámara (Mundo -> Vista)
                    // Mover el punto en dirección opuesta a la cámara
                    Math3D.Vector3D viewPoint = new Math3D.Vector3D(
                        worldPoint.x - cam.Position.x,
                        worldPoint.y - cam.Position.y,
                        worldPoint.z - cam.Position.z
                    );

                    // Rotar el punto alrededor de la cámara (Orbitar)
                    viewPoint = RotatePointHelper(viewPoint, -cam.Rotation.x, -cam.Rotation.y, 0);

                    // 3. Proyección (Vista -> Pantalla 2D)
                    float zoom = 600f;

                    // Si el punto está detrás de la cámara (Z > -1), no se dibuja
                    if (viewPoint.z > -1) { isVisible = false; break; }

                    float zDepth = -viewPoint.z; // Invertimos Z para profundidad positiva
                    float x2d = (viewPoint.x * zoom) / zDepth + viewCenter.X;
                    float y2d = (-viewPoint.y * zoom) / zDepth + viewCenter.Y;

                    face.Corners2D[i] = new PointF(x2d, y2d);
                }

                if (isVisible && face.Corners2D.Length >= 3)
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(200, face.Color)))
                    {
                        g.FillPolygon(brush, face.Corners2D);
                    }
                    // Dibujar borde: Rojo si está seleccionado, Negro si no
                    g.DrawPolygon(Selected ? Pens.Red : Pens.Black, face.Corners2D);
                }
            }
        }

        // --- FUNCIONES AUXILIARES (Aquí estaba el error antes) ---

        // Aplica Escala, Rotación y Traslación
        private Math3D.Vector3D ApplyTransform(Math3D.Vector3D v, Math3D.Vector3D pos, Math3D.Vector3D rot, Math3D.Vector3D scl)
        {
            // 1. Escala
            float vx = v.x * scl.x;
            float vy = v.y * scl.y;
            float vz = v.z * scl.z;

            // 2. Rotación
            Math3D.Vector3D temp = RotatePointHelper(new Math3D.Vector3D(vx, vy, vz), rot.x, rot.y, rot.z);

            // 3. Traslación
            temp.x += pos.x;
            temp.y += pos.y;
            temp.z += pos.z;
            return temp;
        }

        // Esta es la función que te faltaba:
        private Math3D.Vector3D RotatePointHelper(Math3D.Vector3D p, float rx, float ry, float rz)
        {
            Math3D.Vector3D r = p;
            if (rx != 0) r = Math3D.RotateX(r, rx);
            if (ry != 0) r = Math3D.RotateY(r, ry);
            if (rz != 0) r = Math3D.RotateZ(r, rz);
            return r;
        }
    }

    // Clase para manejar la escena con múltiples modelos
    public class Escena
    {
        public List<Modelo3D> Modelos = new List<Modelo3D>();

        public void Draw(Graphics g, Point viewCenter, Camara cam)
        {
            // Ordenar modelos por distancia a la cámara para un dibujado básico correcto (Painter's Algorithm a nivel de objeto)
            var sortedModels = Modelos.OrderByDescending(m => {
                // Distancia desde el centro del modelo a la cámara
                return Math.Sqrt(Math.Pow(m.Position.x - cam.Position.x, 2) +
                                 Math.Pow(m.Position.y - cam.Position.y, 2) +
                                 Math.Pow(m.Position.z - cam.Position.z, 2));
            }).ToList();

            foreach (var modelo in sortedModels)
            {
                modelo.Draw(g, viewCenter, cam);
            }
        }
    }

    // --- MESH FACTORY (Sin cambios, solo para contexto) ---
    public static class MeshFactory
    {
        // Helper para crear caras
        private static Math3D.Face CrearCara(Math3D.Vector3D a, Math3D.Vector3D b, Math3D.Vector3D c, Math3D.Vector3D d, Color color)
        {
            Math3D.Face f = new Math3D.Face();
            f.Color = color;
            if (d == null) f.Corners3D = new Math3D.Vector3D[] { a, b, c };
            else f.Corners3D = new Math3D.Vector3D[] { a, b, c, d };
            f.Center = a; // Centro aproximado
            return f;
        }

        public static Modelo3D CrearCubo(float size)
        {
            Modelo3D m = new Modelo3D();
            float s = size / 2;
            var p = new Math3D.Vector3D[] {
                new Math3D.Vector3D(-s,-s,s), new Math3D.Vector3D(s,-s,s), new Math3D.Vector3D(s,s,s), new Math3D.Vector3D(-s,s,s),
                new Math3D.Vector3D(-s,-s,-s), new Math3D.Vector3D(s,-s,-s), new Math3D.Vector3D(s,s,-s), new Math3D.Vector3D(-s,s,-s)
            };
            // Caras
            m.Faces.Add(CrearCara(p[0], p[1], p[2], p[3], Color.Red));
            m.Faces.Add(CrearCara(p[5], p[4], p[7], p[6], Color.Orange));
            m.Faces.Add(CrearCara(p[4], p[0], p[3], p[7], Color.Green));
            m.Faces.Add(CrearCara(p[1], p[5], p[6], p[2], Color.Blue));
            m.Faces.Add(CrearCara(p[3], p[2], p[6], p[7], Color.Yellow));
            m.Faces.Add(CrearCara(p[4], p[5], p[1], p[0], Color.Gray));
            return m;
        }

        public static Modelo3D CrearPiramide(float size, float height)
        {
            Modelo3D m = new Modelo3D();
            float s = size / 2; float h = height / 2;
            var tip = new Math3D.Vector3D(0, h, 0);
            var b1 = new Math3D.Vector3D(-s, -h, s);
            var b2 = new Math3D.Vector3D(s, -h, s);
            var b3 = new Math3D.Vector3D(s, -h, -s);
            var b4 = new Math3D.Vector3D(-s, -h, -s);
            m.Faces.Add(CrearCara(tip, b1, b2, null, Color.Red));
            m.Faces.Add(CrearCara(tip, b2, b3, null, Color.Green));
            m.Faces.Add(CrearCara(tip, b3, b4, null, Color.Blue));
            m.Faces.Add(CrearCara(tip, b4, b1, null, Color.Yellow));
            m.Faces.Add(CrearCara(b4, b3, b2, b1, Color.White));
            return m;
        }

        public static Modelo3D CrearCilindro(float r, float h, int seg)
        {
            Modelo3D m = new Modelo3D();
            float hh = h / 2;
            for (int i = 0; i < seg; i++)
            {
                double a1 = i * 2 * Math.PI / seg;
                double a2 = (i + 1) * 2 * Math.PI / seg;
                var v1 = new Math3D.Vector3D(r * (float)Math.Cos(a1), -hh, r * (float)Math.Sin(a1));
                var v2 = new Math3D.Vector3D(r * (float)Math.Cos(a2), -hh, r * (float)Math.Sin(a2));
                var v3 = new Math3D.Vector3D(r * (float)Math.Cos(a2), hh, r * (float)Math.Sin(a2));
                var v4 = new Math3D.Vector3D(r * (float)Math.Cos(a1), hh, r * (float)Math.Sin(a1));
                m.Faces.Add(CrearCara(v1, v2, v3, v4, Color.Cyan));
                m.Faces.Add(CrearCara(new Math3D.Vector3D(0, hh, 0), v4, v3, null, Color.LightBlue));
                m.Faces.Add(CrearCara(new Math3D.Vector3D(0, -hh, 0), v2, v1, null, Color.DarkBlue));
            }
            return m;
        }

        public static Modelo3D CrearCono(float r, float h, int seg)
        {
            Modelo3D m = new Modelo3D();
            float hh = h / 2;
            var tip = new Math3D.Vector3D(0, hh, 0);
            var bot = new Math3D.Vector3D(0, -hh, 0);
            for (int i = 0; i < seg; i++)
            {
                double a1 = i * 2 * Math.PI / seg;
                double a2 = (i + 1) * 2 * Math.PI / seg;
                var v1 = new Math3D.Vector3D(r * (float)Math.Cos(a1), -hh, r * (float)Math.Sin(a1));
                var v2 = new Math3D.Vector3D(r * (float)Math.Cos(a2), -hh, r * (float)Math.Sin(a2));
                m.Faces.Add(CrearCara(tip, v1, v2, null, Color.Magenta));
                m.Faces.Add(CrearCara(bot, v2, v1, null, Color.Purple));
            }
            return m;
        }

        public static Modelo3D CrearEsfera(float r, int rings, int seg)
        {
            Modelo3D m = new Modelo3D();
            for (int lat = 0; lat < rings; lat++)
            {
                double phi1 = lat * Math.PI / rings;
                double phi2 = (lat + 1) * Math.PI / rings;
                for (int lon = 0; lon < seg; lon++)
                {
                    double th1 = lon * 2 * Math.PI / seg;
                    double th2 = (lon + 1) * 2 * Math.PI / seg;
                    Math3D.Vector3D GetP(double phi, double th) => new Math3D.Vector3D(r * (float)(Math.Sin(phi) * Math.Cos(th)), r * (float)Math.Cos(phi), r * (float)(Math.Sin(phi) * Math.Sin(th)));
                    m.Faces.Add(CrearCara(GetP(phi1, th1), GetP(phi1, th2), GetP(phi2, th2), GetP(phi2, th1), ((lat + lon) % 2 == 0) ? Color.Cyan : Color.Teal));
                }
            }
            return m;
        }
    }
}