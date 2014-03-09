using SharpDX;

namespace BananaMpq.Geometry
{
    public interface IBVHNode
    {
        TriangleWithNormal Triangle { get; }
        MaterialFlags Flags { get; }
        BoundingBox Bounds { get; }
        Vector3? GetIntersectingPoint(ref Ray ray);
    }
}