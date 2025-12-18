using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motor3D_Educativo_P2
{
    public class Material
    {
        public Color DiffuseColor { get; set; }
        public Color SpecularColor { get; set; }
        public float Shininess { get; set; }

        // Textura opcional
        public Bitmap Texture { get; set; }
        public bool HasTexture => Texture != null;

        public Material(Color diffuseColor)
        {
            DiffuseColor = diffuseColor;
            SpecularColor = Color.White;
            Shininess = 32f;
        }
    }
}
