using System;
using SharpDX;

namespace BananaMpq.Geometry
{
    public class BVHNode : IBVHNode
    {
        private readonly SceneObject _sceneObject;
        private readonly BoundingBox _bounds;
        private readonly int _triangleIndexOrChildCount;

        public TriangleWithNormal Triangle
        {
            get
            {
                if (!IsLeaf)
                {
                    throw new InvalidOperationException("Has to be a leaf node to get the triangle");
                }
                var vertices = _sceneObject.Geometry.Vertices;
                var triangles = _sceneObject.Geometry.Triangles;
                return triangles[_triangleIndexOrChildCount].ToTriangleWithNormal(vertices);
            }
        }

        public MaterialFlags Flags
        {
            get
            {
                if (!IsLeaf)
                {
                    throw new InvalidOperationException("Has to be a leaf node to get the triangle");
                }
                return _sceneObject.MaterialProperties;
            }
        }

        public BoundingBox Bounds
        {
            get { return _bounds; }
        }

        public bool IsLeaf  
        {
            get { return _sceneObject != null; }
        }

        public int ChildCount
        {
            get
            {
                if (IsLeaf)
                {
                    throw new InvalidOperationException("Leaf nodes don't have any children");
                }
                return _triangleIndexOrChildCount;
            }
        }

        public SceneObject SceneObject
        {
            get { return _sceneObject; }
        }

        private BVHNode(SceneObject sceneObject, int triangleIndexOrChildCount, ref BoundingBox bounds)
        {
            _bounds = bounds;
            _triangleIndexOrChildCount = triangleIndexOrChildCount;
            _sceneObject = sceneObject;
        }

        public Vector3? GetIntersectingPoint(ref Ray ray)
        {
            float dist;
            var t = Triangle;
            if (!ray.Intersects(ref t.A, ref t.B, ref t.C, out dist)) return null;
            return ray.Position + dist*ray.Direction;
        }

        public static BVHNode CreateLeaf(SceneObject sceneObject, int firstTriangleIndex, ref BoundingBox bounds)
        {
            return new BVHNode(sceneObject, firstTriangleIndex, ref bounds);
        }

        public static BVHNode CreateBranch(int childCount, ref BoundingBox bounds)
        {
            return new BVHNode(null, childCount, ref bounds);
        }
    }
}