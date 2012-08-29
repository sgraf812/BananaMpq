using System.Collections.Generic;
using BananaMpq.Geometry;
using SharpDX;
using SharpDX.Direct3D9;

namespace BananaMpq.View.Infrastructure
{
    public class SceneBoundsRenderer : ISceneRenderer
    {
        private static readonly short[] _boxIndices = new short[]
        {
            0, 1, 0, 2, 0, 4, 
            7, 5, 7, 6, 7, 3, 
            2, 3, 2, 6, 4, 5,
            4, 6, 3, 1, 5, 1
        };

        private readonly Device _device;
        private readonly IDictionary<SceneObject, IRenderBatch> _meshCache = new Dictionary<SceneObject, IRenderBatch>(); 

        public SceneBoundsRenderer(Device device)
        {
            _device = device;
        }

        public void Render(Scene scene)
        {
            foreach (var sceneObject in scene.Terrain)
            {
                IRenderBatch batch;
                if (!_meshCache.TryGetValue(sceneObject, out batch))
                {
                    batch = GetBoundingBoxBatch(sceneObject.Bounds);
                    _meshCache[sceneObject] = batch;
                }
                batch.Render();
            }
        }

        private IRenderBatch GetBoundingBoxBatch(BoundingBox bounds)
        {
            var l = bounds.Minimum;
            var u = bounds.Maximum;
            var vertices = new[]
            {
                new VertexPositionColored { Position = new Vector3(l.X, l.Y, l.Z), Color = Colors.LimeGreen },
                new VertexPositionColored { Position = new Vector3(l.X, l.Y, u.Z), Color = Colors.LimeGreen },
                new VertexPositionColored { Position = new Vector3(l.X, u.Y, l.Z), Color = Colors.LimeGreen },
                new VertexPositionColored { Position = new Vector3(l.X, u.Y, u.Z), Color = Colors.LimeGreen },
                new VertexPositionColored { Position = new Vector3(u.X, l.Y, l.Z), Color = Colors.LimeGreen },
                new VertexPositionColored { Position = new Vector3(u.X, l.Y, u.Z), Color = Colors.LimeGreen },
                new VertexPositionColored { Position = new Vector3(u.X, u.Y, l.Z), Color = Colors.LimeGreen },
                new VertexPositionColored { Position = new Vector3(u.X, u.Y, u.Z), Color = Colors.LimeGreen },
            };

            var batch = new DirectX9RenderBatch(_device, PrimitiveType.LineList);
            batch.Record(_boxIndices, vertices);
            return batch;
        }
    }
}