using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motor3D_Educativo_P2.Geometry
{
    // Esta clase cumple tu rol de "Lógica Matemática"
    public class Math3D
    {
        public class Vector3D
        {
            public float x, y, z;
            public Vector3D(float _x, float _y, float _z) { x = _x; y = _y; z = _z; }

            // Operaciones básicas necesarias
            public static Vector3D operator +(Vector3D v1, Vector3D v2) => new Vector3D(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
            public static Vector3D operator -(Vector3D v1, Vector3D v2) => new Vector3D(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
            public static Vector3D operator *(Vector3D v, float f) => new Vector3D(v.x * f, v.y * f, v.z * f);
        }

        public class Face
        {
            public Vector3D[] Corners3D; // Los puntos en el espacio 3D
            public PointF[] Corners2D;   // Los puntos proyectados en pantalla
            public Vector3D Center;      // Para ordenar qué se dibuja primero
            public Color Color;          // Color de la cara
        }

        // --- TU RESPONSABILIDAD: Matrices de Transformación (Simplificadas) ---

        public static Vector3D RotateX(Vector3D point, float angle)
        {
            float rad = angle * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            return new Vector3D(point.x, point.y * cos - point.z * sin, point.y * sin + point.z * cos);
        }

        public static Vector3D RotateY(Vector3D point, float angle)
        {
            float rad = angle * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            return new Vector3D(point.x * cos + point.z * sin, point.y, point.x * -sin + point.z * cos);
        }

        public static Vector3D RotateZ(Vector3D point, float angle)
        {
            float rad = angle * (float)Math.PI / 180f;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            return new Vector3D(point.x * cos - point.y * sin, point.x * sin + point.y * cos, point.z);
        }
    }
}