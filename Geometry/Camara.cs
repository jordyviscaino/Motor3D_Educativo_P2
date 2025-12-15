using Motor3D_Educativo_P2.Geometry;
using System;

namespace Motor3D_Educativo_P2
{
    public class Camara
    {
        // Posición inicial: Un poco arriba (Y=100) y atrás (Z=-500)
        public Math3D.Vector3D Position = new Math3D.Vector3D(0, 100, -500);
        public Math3D.Vector3D Rotation = new Math3D.Vector3D(0, 0, 0);

        public float Speed = 10.0f;
    }
}