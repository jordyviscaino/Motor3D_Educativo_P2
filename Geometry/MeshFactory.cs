using System;
using System.Collections.Generic;
using OpenTK;
using Motor3D_Educativo_P2.Core;

namespace Motor3D_Educativo_P2.Geometry
{
    public static class MeshFactory
    {
        public static Mesh CrearCubo(float size = 1.0f)
        {
            float s = size / 2.0f;
            // Caras simples con normales básicas
            Vertex[] vertices =
            {
               new Vertex(new Vector3(-s, -s,  s), Vector3.UnitZ, new Vector3(1, 0, 0)), // 0
               new Vertex(new Vector3( s, -s,  s), Vector3.UnitZ, new Vector3(0, 1, 0)), // 1
               new Vertex(new Vector3( s,  s,  s), Vector3.UnitZ, new Vector3(0, 0, 1)), // 2
               new Vertex(new Vector3(-s,  s,  s), Vector3.UnitZ, new Vector3(1, 1, 0)), // 3
               new Vertex(new Vector3(-s, -s, -s), -Vector3.UnitZ, new Vector3(0, 1, 1)),// 4
               new Vertex(new Vector3( s, -s, -s), -Vector3.UnitZ, new Vector3(1, 0, 1)),// 5
               new Vertex(new Vector3( s,  s, -s), -Vector3.UnitZ, new Vector3(1, 1, 1)),// 6
               new Vertex(new Vector3(-s,  s, -s), -Vector3.UnitZ, new Vector3(0, 0, 0)) // 7
            };

            int[] indices =
            {
                0, 1, 2, 0, 2, 3, // Frente
                5, 4, 7, 5, 7, 6, // Atrás
                4, 0, 3, 4, 3, 7, // Izq
                1, 5, 6, 1, 6, 2, // Der
                3, 2, 6, 3, 6, 7, // Arriba
                4, 5, 1, 4, 1, 0  // Abajo
            };

            return new Mesh(vertices, indices);
        }

        public static Mesh CrearEsfera(float radius, int bands, int slices)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<int> indices = new List<int>();

            for (int lat = 0; lat <= bands; lat++)
            {
                float theta = lat * (float)Math.PI / bands;
                float sinTheta = (float)Math.Sin(theta);
                float cosTheta = (float)Math.Cos(theta);

                for (int lon = 0; lon <= slices; lon++)
                {
                    float phi = lon * 2.0f * (float)Math.PI / slices;
                    float sinPhi = (float)Math.Sin(phi);
                    float cosPhi = (float)Math.Cos(phi);

                    float x = cosPhi * sinTheta;
                    float y = cosTheta;
                    float z = sinPhi * sinTheta;

                    Vector3 pos = new Vector3(x * radius, y * radius, z * radius);
                    vertices.Add(new Vertex(pos, pos.Normalized(), new Vector3(1, 1, 1)));
                }
            }

            for (int lat = 0; lat < bands; lat++)
            {
                for (int lon = 0; lon < slices; lon++)
                {
                    int first = (lat * (slices + 1)) + lon;
                    int second = first + slices + 1;
                    indices.Add(first); indices.Add(second); indices.Add(first + 1);
                    indices.Add(second); indices.Add(second + 1); indices.Add(first + 1);
                }
            }

            return new Mesh(vertices.ToArray(), indices.ToArray());
        }
    }
}