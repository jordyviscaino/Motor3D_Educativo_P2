using OpenTK;

namespace Motor3D_Educativo_P2.Core
{
    public class Transform
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;

        // CORRECCIÓN CRÍTICA: Escala debe ser 1, no 0.
        public Vector3 Scale { get; set; } = new Vector3(1, 1, 1);
        public Matrix4 GetModelMatrix()
        {
            // 1. Escala
            Matrix4 matScale = Matrix4.CreateScale(Scale);

            // 2. Rotación (Convertir Grados a Radianes)
            Matrix4 matRotX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X));
            Matrix4 matRotY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y));
            Matrix4 matRotZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z));
            Matrix4 matRotation = matRotX * matRotY * matRotZ;

            // 3. Traslación
            Matrix4 matTranslation = Matrix4.CreateTranslation(Position);

            // 4. Multiplicación T * R * S
            return matScale * matRotation * matTranslation;
        }
    }
}