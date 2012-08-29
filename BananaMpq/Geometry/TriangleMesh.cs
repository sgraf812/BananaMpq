using SharpDX;

namespace BananaMpq.Geometry
{
    public class TriangleMesh
    {
        public Vector3[] Vertices { get; set; }
        public IndexedTriangleWithNormal[] Triangles { get; set; }
    }
}