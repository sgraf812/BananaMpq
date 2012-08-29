using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using BananaMpq.Geometry;
using BananaMpq.Visualization;
using SharpDX;

namespace BananaMpq.Layer
{
    public class M2Bounds : IHasVisualizableProperties
    {
        public BoundingBox Bounds { get; private set; }
        public float BoundingRadius { get; private set; }
        public Vector3[] Vertices { get; private set; }
        public IndexedTriangle[] Triangles { get; private set; }

        public unsafe M2Bounds(byte[] data)
        {
            fixed (byte* p = data)
            {
                var header = (M2Header*)p;
                Bounds = header->bounds;
                BoundingRadius = header->boundingRadius;
                ParseVertices((Vector3*)(p + header->offBoundingVertices), header->nBoundingVertices);
                ParseTriangles((ushort*)(p + header->offBoundingTriangles), header->nBoundingTriangles);
            }
        }

        private unsafe void ParseVertices(Vector3* vertices, int vertexCount)
        {
            Vertices = new Vector3[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                Vertices[i] = vertices[i];
            }
        }

        private unsafe void ParseTriangles(ushort* triangles, int triangleCount)
        {
            Triangles = new IndexedTriangle[triangleCount/3];
            for (int i = 0; i < triangleCount; i += 3)
            {
                Triangles[i/3].A = triangles[i];
                Triangles[i / 3].B = triangles[i + 1];
                Triangles[i / 3].C = triangles[i + 2];
            }
        }

#pragma warning disable 169, 649
        [StructLayout(LayoutKind.Sequential, Size = 0x138)]
        unsafe struct M2Header
        {
            private fixed byte redundant[0xBC];
            public BoundingBox bounds;
            public float boundingRadius;
            public int nBoundingTriangles;
            public int offBoundingTriangles;
            public int nBoundingVertices;
            public int offBoundingVertices;
        }
#pragma warning restore 169, 649

        #region Implementation of IHasVisualizableProperties

        public IEnumerable<PropertyInfo> VisualizableProperties
        {
            get
            {
                var t = typeof(M2Bounds);
                return new[]
                {
                    t.GetProperty("Bounds"),
                    t.GetProperty("BoundingRadius"),
                    t.GetProperty("Vertices"),
                    t.GetProperty("Triangles"),
                };
            }
        }

        #endregion
    }
}