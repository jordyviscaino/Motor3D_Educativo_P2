using Motor3D_Educativo_P2.Core;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using Motor3D_Educativo_P2;

namespace Motor3D_Educativo_P2.Core
{
    public class Mesh
    {
        public Vertex[] Vertices { get; private set; }
        public int[] Indices { get; private set; }

        public Material Material { get; set; }

        public Mesh(Vertex[] vertices, int[] indices)
            : this(vertices, indices, new Material(Color.LightGray))
        {
        }

        public Mesh(Vertex[] vertices, int[] indices, Material material)
        {
            Vertices = vertices;
            Indices = indices;
            Material = material ?? new Material(Color.LightGray);

            ApplyMaterialToVertexColors();
        }

        private void ApplyMaterialToVertexColors()
        {
            if (Vertices == null || Material == null) return;

            Color c = Material.DiffuseColor;
            Vector3 colorVec = new Vector3(c.R / 255f, c.G / 255f, c.B / 255f);

            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertex v = Vertices[i];
                v.Color = colorVec;
                Vertices[i] = v;
            }
        }
    }
}