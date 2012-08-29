using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace BananaMpq
{
    public static class MathUtil
    {
        public static Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = (b - a);
            var ac = (c - a);
            return Vector3.Normalize(Vector3.Cross(ab, ac));
        }

        public static IEnumerable<Vector3> Transform(this IEnumerable<Vector3> vertices, Matrix transform)
        {
            return vertices.Select(v => (Vector3)Vector3.Transform(v, transform));
        }

        public static bool Contains(this RectangleF rect, Vector2 v)
        {
            return !(v.X < rect.Left || v.X > rect.Right || v.Y < rect.Bottom || v.Y > rect.Top);
        }

        public static bool Contains(this RectangleF rect, Vector3 v)
        {
            return Contains(rect, (Vector2)v);
        }

        public static bool Intersects(this RectangleF rect, BoundingBox box)
        {
            return rect.Contains(box.Minimum) || rect.Contains(box.Maximum);
        }
    }
}