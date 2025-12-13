using OpenTK; // En v3.1, Vector3 está aquí directamente
using System.Drawing; // Necesario para colores a veces, pero usaremos Vector3 para color por ahora



namespace Motor3D_Educativo_P2.Core
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Color;

        public Vertex(Vector3 position, Vector3 normal, Vector3 color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }

        // Tamaño en bytes (3 floats * 3 vectores = 36 bytes)
        public const int SizeInBytes = 9 * sizeof(float);
    }
}
