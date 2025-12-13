using Motor3D_Educativo_P2.Core;
using System.Collections.Generic;

namespace Motor3D_Educativo_P2.Core
{
    public class Mesh
    {
        public Vertex[] Vertices { get; private set; }
        public int[] Indices { get; private set; }

        public Mesh(Vertex[] vertices, int[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}