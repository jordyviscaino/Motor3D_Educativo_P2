using Motor3D_Educativo_P2.Geometry;
using Motor3D_Educativo_P2.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Motor3D_Educativo_P2
{
    // Clase base para objetos 3D
    public class Modelo3D
    {
        public List<Math3D.Face> Faces = new List<Math3D.Face>();
        public Math3D.Vector3D Position = new Math3D.Vector3D(0, 0, 0);
        public Math3D.Vector3D Rotation = new Math3D.Vector3D(0, 0, 0);
        public Math3D.Vector3D Scale = new Math3D.Vector3D(1, 1, 1);

        // Material opcional para el modelo
        public Material Material = new Material(Color.LightGray);

        // Render mode control
        public RenderMode CurrentRenderMode = RenderMode.Lit;

        // --- MÉTODO DRAW CORREGIDO ---
        public void Draw(Graphics g, Point viewCenter, Camara cam)
        {
            if (Faces.Count == 0) return;

            // Luz (dirección en espacio mundo) - vector direccional
            Math3D.Vector3D lightDir = Normalize(new Math3D.Vector3D(0, -1, -1));

            // Ordenamiento (Z-Sorting) para que las caras lejanas se dibujen primero
            var sortedFaces = Faces.OrderByDescending(f => {
                var c = f.Center;
                // Transformamos el centro para saber su profundidad real
                var worldPos = ApplyTransform(c, Position, Rotation, Scale);
                // Distancia a la cámara (podría ser sin sqrt para optimización)
                return Math.Sqrt(Math.Pow(worldPos.x - cam.Position.x, 2) +
                                 Math.Pow(worldPos.y - cam.Position.y, 2) +
                                 Math.Pow(worldPos.z - cam.Position.z, 2));
            }).ToList();

            foreach (var face in sortedFaces)
            {
                face.Corners2D = new PointF[face.Corners3D.Length];
                bool isVisible = true;

                // Transformar todos los puntos al espacio mundo primero (para normal e iluminación)
                Math3D.Vector3D[] worldPoints = new Math3D.Vector3D[face.Corners3D.Length];
                for (int i = 0; i < face.Corners3D.Length; i++)
                {
                    worldPoints[i] = ApplyTransform(face.Corners3D[i], Position, Rotation, Scale);
                }

                // Calcular normal de la cara (usando los primeros 3 vértices)
                Math3D.Vector3D faceNormal = new Math3D.Vector3D(0, 0, 0);
                if (worldPoints.Length >= 3)
                {
                    var a = worldPoints[0];
                    var b = worldPoints[1];
                    var c = worldPoints[2];
                    faceNormal = Normalize(Cross(Subtract(b, a), Subtract(c, a)));
                }

                // --- Backface culling (optimización) ---
                // Calcular centro de la cara en espacio mundo y la dirección desde la cara hacia la cámara
                Math3D.Vector3D worldCenter = ApplyTransform(face.Center, Position, Rotation, Scale);
                Math3D.Vector3D toCamera = Normalize(Subtract(cam.Position, worldCenter));
                float facing = Dot(faceNormal, toCamera);
                // Si facing <= 0 significa que la cara está orientada lejos de la cámara -> cull
                if (facing <= 0f) continue;

                // Cálculo de intensidad (Ley de Lambert)
                // Usamos -lightDir si lightDir apunta desde la luz hacia la escena
                float intensity = Dot(faceNormal, new Math3D.Vector3D(-lightDir.x, -lightDir.y, -lightDir.z));
                intensity = Math.Max(0f, intensity);

                // Aplicar sombreado multiplicando el color de la cara por la intensidad
                Color baseColor = face.Color;
                // Preferir color del material del modelo si existe
                Color materialColor = (Material != null) ? Material.DiffuseColor : baseColor;

                int r = (int)(materialColor.R * intensity);
                int gcol = (int)(materialColor.G * intensity);
                int bcol = (int)(materialColor.B * intensity);
                r = Math.Max(0, Math.Min(255, r));
                gcol = Math.Max(0, Math.Min(255, gcol));
                bcol = Math.Max(0, Math.Min(255, bcol));
                Color shaded = Color.FromArgb(200, r, gcol, bcol);

                // Choose drawing behavior based on RenderMode
                switch (CurrentRenderMode)
                {
                    case RenderMode.Wireframe:
                        DrawWireframe(g, face, worldPoints, cam, ref isVisible, viewCenter);
                        break;

                    case RenderMode.Solid:
                        DrawSolid(g, face, worldPoints, cam, ref isVisible, viewCenter, materialColor);
                        break;

                    case RenderMode.Lit:
                    default:
                        DrawLit(g, face, worldPoints, cam, ref isVisible, viewCenter, shaded);
                        break;
                }
            }
        }

        // Wireframe: dibuja solo aristas
        private void DrawWireframe(Graphics g, Math3D.Face face, Math3D.Vector3D[] worldPoints, Camara cam, ref bool isVisible, Point viewCenter)
        {
            PointF[] pts = new PointF[worldPoints.Length];
            for (int i = 0; i < worldPoints.Length; i++)
            {
                Math3D.Vector3D viewPoint = new Math3D.Vector3D(
                    worldPoints[i].x - cam.Position.x,
                    worldPoints[i].y - cam.Position.y,
                    worldPoints[i].z - cam.Position.z);
                viewPoint = RotatePointHelper(viewPoint, -cam.Rotation.x, -cam.Rotation.y, 0);
                if (viewPoint.z > -1) { isVisible = false; break; }
                float zDepth = -viewPoint.z;
                pts[i] = new PointF((viewPoint.x * 600f) / zDepth + viewCenter.X, (-viewPoint.y * 600f) / zDepth + viewCenter.Y);
            }
            if (isVisible) g.DrawPolygon(Pens.Black, pts);
        }

        // Solid: rellena con color sólido (sin iluminación)
        private void DrawSolid(Graphics g, Math3D.Face face, Math3D.Vector3D[] worldPoints, Camara cam, ref bool isVisible, Point viewCenter, Color color)
        {
            PointF[] pts = new PointF[worldPoints.Length];
            for (int i = 0; i < worldPoints.Length; i++)
            {
                Math3D.Vector3D viewPoint = new Math3D.Vector3D(
                    worldPoints[i].x - cam.Position.x,
                    worldPoints[i].y - cam.Position.y,
                    worldPoints[i].z - cam.Position.z);
                viewPoint = RotatePointHelper(viewPoint, -cam.Rotation.x, -cam.Rotation.y, 0);
                if (viewPoint.z > -1) { isVisible = false; break; }
                float zDepth = -viewPoint.z;
                pts[i] = new PointF((viewPoint.x * 600f) / zDepth + viewCenter.X, (-viewPoint.y * 600f) / zDepth + viewCenter.Y);
            }
            if (isVisible && pts.Length >= 3)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(200, color)))
                {
                    g.FillPolygon(brush, pts);
                }
                g.DrawPolygon(Pens.Black, pts);
            }
        }

        // Lit: usa color sombreado calculado por iluminación
        private void DrawLit(Graphics g, Math3D.Face face, Math3D.Vector3D[] worldPoints, Camara cam, ref bool isVisible, Point viewCenter, Color shaded)
        {
            PointF[] pts = new PointF[worldPoints.Length];
            for (int i = 0; i < worldPoints.Length; i++)
            {
                Math3D.Vector3D viewPoint = new Math3D.Vector3D(
                    worldPoints[i].x - cam.Position.x,
                    worldPoints[i].y - cam.Position.y,
                    worldPoints[i].z - cam.Position.z);
                viewPoint = RotatePointHelper(viewPoint, -cam.Rotation.x, -cam.Rotation.y, 0);
                if (viewPoint.z > -1) { isVisible = false; break; }
                float zDepth = -viewPoint.z;
                pts[i] = new PointF((viewPoint.x * 600f) / zDepth + viewCenter.X, (-viewPoint.y * 600f) / zDepth + viewCenter.Y);
            }
            if (isVisible && pts.Length >= 3)
            {
                using (SolidBrush brush = new SolidBrush(shaded))
                {
                    g.FillPolygon(brush, pts);
                }
                g.DrawPolygon(Pens.Black, pts);
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

        // Vector helpers para iluminación
        private static Math3D.Vector3D Subtract(Math3D.Vector3D a, Math3D.Vector3D b)
        {
            return new Math3D.Vector3D(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        private static Math3D.Vector3D Cross(Math3D.Vector3D a, Math3D.Vector3D b)
        {
            return new Math3D.Vector3D(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x);
        }

        private static float Dot(Math3D.Vector3D a, Math3D.Vector3D b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        private static float Length(Math3D.Vector3D a)
        {
            return (float)Math.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
        }

        private static Math3D.Vector3D Normalize(Math3D.Vector3D a)
        {
            float len = Length(a);
            if (len == 0) return new Math3D.Vector3D(0, 0, 0);
            return new Math3D.Vector3D(a.x / len, a.y / len, a.z / len);
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

            // Calcular centro promedio de los vértices
            float cx = 0, cy = 0, cz = 0;
            for (int i = 0; i < f.Corners3D.Length; i++)
            {
                cx += f.Corners3D[i].x;
                cy += f.Corners3D[i].y;
                cz += f.Corners3D[i].z;
            }
            int count = f.Corners3D.Length;
            f.Center = new Math3D.Vector3D(cx / count, cy / count, cz / count);

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
                for (sbyte lon = 0; lon < seg; lon++)
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