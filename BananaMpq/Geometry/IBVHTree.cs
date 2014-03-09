using System.Collections.Generic;
using SharpDX;

namespace BananaMpq.Geometry
{
    public interface IBVHTree
    {
        BoundingBox Bounds { get; }
        IEnumerable<IBVHNode> GetIntersections(ref BoundingBox bounds);
        IBVHNode GetIntersectedNode(ref Ray ray); 
    }
}