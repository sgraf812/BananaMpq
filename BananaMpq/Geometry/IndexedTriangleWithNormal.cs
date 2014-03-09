using System.Collections.Generic;
using SharpDX;

namespace BananaMpq.Geometry
{
    public struct IndexedTriangleWithNormal
    {
        public int A, B, C;
        public Vector3 Normal;

        public static IndexedTriangleWithNormal CreateFromVertices(int a, int b, int c, IList<Vector3> vertices)
        {
            return new IndexedTriangleWithNormal
            {
                A = a,
                B = b,
                C = c,
                Normal = MathUtil.TriangleNormal(vertices[a], vertices[b], vertices[c])
            };
        }

        public static IndexedTriangleWithNormal CreateFromVertices(IndexedTriangle triangle, IList<Vector3> vertices)
        {
            return CreateFromVertices(triangle.A, triangle.B, triangle.C, vertices);
        }

        public BoundingBox BoundsIn(IList<Vector3> vertices)
        {
            return BoundingBox.FromPoints(new[] { vertices[A], vertices[B], vertices[C] });
        }

        public TriangleWithNormal ToTriangleWithNormal(IList<Vector3> vertices)
        {
            return new TriangleWithNormal
            {
                A = vertices[A],
                B = vertices[B],
                C = vertices[C],
                Normal = Normal
            };
        }
    }
}