using Motor3D_Educativo_P2.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Motor3D_Educativo_P2
{
    // Clase base para cualquier objeto 3D
    public class Modelo3D
    {
        public List<Math3D.Face> Faces = new List<Math3D.Face>();
        public Math3D.Vector3D Position = new Math3D.Vector3D(0, 0, 0);
        public float RotX = 0, RotY = 0, RotZ = 0;
        public float Scale = 1.0f;

        public void Draw(Graphics g, Point viewCenter)
        {
            if (Faces.Count == 0) return; // Si no hay caras, no hacemos nada

            foreach (var face in Faces)
            {
                face.Corners2D = new PointF[face.Corners3D.Length];

                for (int i = 0; i < face.Corners3D.Length; i++)
                {
                    // 1. Aplicar Rotación y Escala
                    Math3D.Vector3D v = face.Corners3D[i];

                    // Escala manual
                    float vx = v.x * Scale;
                    float vy = v.y * Scale;
                    float vz = v.z * Scale;

                    // Rotación (Usando las funciones estáticas de tu Math3D)
                    Math3D.Vector3D temp = new Math3D.Vector3D(vx, vy, vz);
                    temp = Math3D.RotateX(temp, RotX);
                    temp = Math3D.RotateY(temp, RotY);
                    temp = Math3D.RotateZ(temp, RotZ);

                    // Traslación
                    temp.x += Position.x;
                    temp.y += Position.y;
                    temp.z += Position.z;

                    // 2. Proyección 3D a 2D (Perspectiva simple)
                    float zoom = 600f;
                    float dist = 1000f - temp.z;
                    if (dist < 1) dist = 1; // Evitar división por cero

                    float x2d = (temp.x * zoom) / dist + viewCenter.X;
                    float y2d = (-temp.y * zoom) / dist + viewCenter.Y;

                    face.Corners2D[i] = new PointF(x2d, y2d);
                }
            }

            // Ordenar caras (Dibujar las lejanas primero)
            // Nota: Ordenamiento simplificado
            var sortedFaces = Faces.OrderByDescending(f => f.Corners3D[0].z).ToList();

            // Dibujar
            foreach (var face in sortedFaces)
            {
                if (face.Corners2D == null || face.Corners2D.Length < 3) continue;

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(200, face.Color)))
                {
                    g.FillPolygon(brush, face.Corners2D);
                }
                g.DrawPolygon(Pens.White, face.Corners2D); // Bordes blancos para ver mejor
            }
        }
    }

    // FÁBRICA DE FIGURAS
    public static class MeshFactory
    {
        // --- CUBO ---
        public static Modelo3D CrearCubo(float size)
        {
            Modelo3D m = new Modelo3D();
            float s = size / 2;

            // Definir vértices manualmente para evitar errores
            var p0 = new Math3D.Vector3D(-s, -s, s);
            var p1 = new Math3D.Vector3D(s, -s, s);
            var p2 = new Math3D.Vector3D(s, s, s);
            var p3 = new Math3D.Vector3D(-s, s, s);
            var p4 = new Math3D.Vector3D(-s, -s, -s);
            var p5 = new Math3D.Vector3D(s, -s, -s);
            var p6 = new Math3D.Vector3D(s, s, -s);
            var p7 = new Math3D.Vector3D(-s, s, -s);

            // Agregar caras (Cuadrados)
            m.Faces.Add(CrearCara(p0, p1, p2, p3, Color.Red));    // Frente
            m.Faces.Add(CrearCara(p5, p4, p7, p6, Color.Orange)); // Atrás
            m.Faces.Add(CrearCara(p4, p0, p3, p7, Color.Green));  // Izquierda
            m.Faces.Add(CrearCara(p1, p5, p6, p2, Color.Blue));   // Derecha
            m.Faces.Add(CrearCara(p3, p2, p6, p7, Color.Yellow)); // Arriba
            m.Faces.Add(CrearCara(p4, p5, p1, p0, Color.Gray));   // Abajo

            return m;
        }

        // --- PIRÁMIDE (Versión Simplificada) ---
        public static Modelo3D CrearPiramide(float size, float height)
        {
            Modelo3D m = new Modelo3D();
            float s = size / 2;
            float h = height / 2;

            // 5 Puntos Clave
            var tip = new Math3D.Vector3D(0, h, 0);          // Punta
            var b1 = new Math3D.Vector3D(-s, -h, s);  // Base Frente Izq
            var b2 = new Math3D.Vector3D(s, -h, s);   // Base Frente Der
            var b3 = new Math3D.Vector3D(s, -h, -s);  // Base Atrás Der
            var b4 = new Math3D.Vector3D(-s, -h, -s); // Base Atrás Izq

            // 4 Triángulos (Lados)
            m.Faces.Add(CrearCara(tip, b1, b2, null, Color.Red));    // Frente
            m.Faces.Add(CrearCara(tip, b2, b3, null, Color.Green));  // Derecha
            m.Faces.Add(CrearCara(tip, b3, b4, null, Color.Blue));   // Atrás
            m.Faces.Add(CrearCara(tip, b4, b1, null, Color.Yellow)); // Izquierda

            // 1 Cuadrado (Base)
            m.Faces.Add(CrearCara(b4, b3, b2, b1, Color.White));

            return m;
        }

        // --- CILINDRO (Versión Simplificada) ---
        public static Modelo3D CrearCilindro(float radius, float height, int segments)
        {
            Modelo3D m = new Modelo3D();
            float h = height / 2;

            for (int i = 0; i < segments; i++)
            {
                double angle1 = i * 2 * Math.PI / segments;
                double angle2 = (i + 1) * 2 * Math.PI / segments;

                // Coordenadas
                float x1 = radius * (float)Math.Cos(angle1);
                float z1 = radius * (float)Math.Sin(angle1);
                float x2 = radius * (float)Math.Cos(angle2);
                float z2 = radius * (float)Math.Sin(angle2);

                // 4 Vértices del segmento lateral
                var v1 = new Math3D.Vector3D(x1, -h, z1);
                var v2 = new Math3D.Vector3D(x2, -h, z2);
                var v3 = new Math3D.Vector3D(x2, h, z2);
                var v4 = new Math3D.Vector3D(x1, h, z1);

                // Cara Lateral
                m.Faces.Add(CrearCara(v1, v2, v3, v4, Color.Cyan));

                // Tapas (Triángulos)
                var centerTop = new Math3D.Vector3D(0, h, 0);
                var centerBottom = new Math3D.Vector3D(0, -h, 0);

                m.Faces.Add(CrearCara(centerTop, v4, v3, null, Color.LightBlue)); // Tapa Arriba
                m.Faces.Add(CrearCara(centerBottom, v2, v1, null, Color.DarkBlue)); // Tapa Abajo
            }

            return m;
        }

        // Helper para crear caras de 3 o 4 puntos
        private static Math3D.Face CrearCara(Math3D.Vector3D a, Math3D.Vector3D b, Math3D.Vector3D c, Math3D.Vector3D d, Color color)
        {
            Math3D.Face f = new Math3D.Face();
            f.Color = color;
            if (d == null) // Triángulo
                f.Corners3D = new Math3D.Vector3D[] { a, b, c };
            else // Cuadrilátero
                f.Corners3D = new Math3D.Vector3D[] { a, b, c, d };

            // Calculamos centro simple para ordenamiento
            f.Center = a;
            return f;
        }
    }
}