using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace BananaMpq.Geometry.Builders
{
    public class ModelBuilder
    {
        protected FilePool Files { get; private set; }

        public ModelBuilder(FilePool files)
        {
            Files = files;
        }

        protected static SceneObject BuildModelFromTransform(IEnumerable<Vector3> vertices, IEnumerable<IndexedTriangle> triangles, Matrix transform)
        {
            var transformedVertices = vertices.Transform(transform).ToArray();

            return new SceneObject
            {
                Bounds = BoundingBox.FromPoints(transformedVertices),
                Geometry = new TriangleMesh
                {
                    Vertices = transformedVertices,
                    Triangles = triangles.Select(t => IndexedTriangleWithNormal.CreateFromVertices(t, transformedVertices)).ToArray()
                }
            };
        }
    }
}