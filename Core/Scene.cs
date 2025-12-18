using System.Drawing;
using Motor3D_Educativo_P2.Geometry;

namespace Motor3D_Educativo_P2.Core
{
    // Contenedor global simple para parámetros de escena (luz, cámara shared settings, etc.)
    public static class Scene
    {
        // Dirección de la luz direccional (espacio mundo). Por defecto: desde arriba-atrás
        public static Math3D.Vector3D LightDirection = new Math3D.Vector3D(0, -1, -1);

        // Color e intensidad de la luz
        public static Color LightColor = Color.White;
        public static float LightIntensity = 1.0f;
    }
}
