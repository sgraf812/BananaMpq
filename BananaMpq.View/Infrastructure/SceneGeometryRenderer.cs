using System.Collections.Generic;
using System.Linq;
using BananaMpq.Geometry;
using SharpDX;
using SharpDX.Direct3D9;

namespace BananaMpq.View.Infrastructure
{
    public class SceneGeometryRenderer : ISceneRenderer
    {
        private readonly Device _device;
        private readonly IDictionary<SceneObject, IRenderBatch> _meshCache = new Dictionary<SceneObject, IRenderBatch>();
        private readonly Material _terrainMaterial = CreateColorMaterial(Colors.Green);
        private readonly Material _waterMaterial = CreateColorMaterial(Colors.Blue);
        private readonly Material _harmingLiquidMaterial = CreateColorMaterial(Colors.Indigo);
        private readonly Material _doodadMaterial = CreateColorMaterial(Colors.Red);
        private readonly Material _wmoMaterial = CreateColorMaterial(Colors.Yellow);

        public SceneGeometryRenderer(Device device)
        {
            _device = device;
        }

        private static Material CreateColorMaterial(Color4 color)
        {
            return new Material
            {
                Diffuse = color,
                Specular = color,
                Ambient = color,
                Power = 1.0f
            };
        }

        #region Implementation of ISceneRenderer

        public void Render(Scene scene)
        {
            _device.VertexFormat = VertexPositionNormal.Format;
            _device.SetRenderState(RenderState.CullMode, Cull.None);
            RenderSceneObjects(scene.Liquids.Where(l=>(l.MaterialProperties & MaterialFlags.DamageOverTime)!= 0), _harmingLiquidMaterial);
            RenderSceneObjects(scene.Liquids.Where(l => (l.MaterialProperties & MaterialFlags.DamageOverTime) == 0), _waterMaterial);
            _device.SetRenderState(RenderState.CullMode, Cull.Clockwise);
            RenderSceneObjects(scene.Terrain, _terrainMaterial);
            RenderSceneObjects(scene.Doodads, _doodadMaterial);
            RenderSceneObjects(scene.Wmos, _wmoMaterial);
        }

        private void RenderSceneObjects(IEnumerable<SceneObject> sceneObjects, Material material)
        {
            _device.Material = material;
            foreach (var sceneObject in sceneObjects)
            {
                RenderSceneObject(sceneObject);
            }
        }

        private void RenderSceneObject(SceneObject sceneObject)
        {
            IRenderBatch batch;
            if (!_meshCache.TryGetValue(sceneObject, out batch))
            {
                batch = GetRenderBatch(sceneObject);
                _meshCache[sceneObject] = batch;
            }
            batch.Render();
        }

        private IRenderBatch GetRenderBatch(SceneObject sceneObject)
        {
            var batch = new DirectX9RenderBatch(_device, PrimitiveType.TriangleList);
            var indices = sceneObject.Geometry.Triangles.SelectMany(t => new[] { t.A, t.B, t.C }).ToArray();
            var vertices = sceneObject.Geometry.Vertices.Select(
                v => new VertexPositionNormal { Position = new Vector3(v.ToArray()), Normal = Vector3.Zero }).ToArray();
            ComputeVertexNormals(vertices, sceneObject.Geometry.Triangles);
            batch.Record(indices, vertices);
            return batch;
        }

        private static void ComputeVertexNormals(VertexPositionNormal[] vertices, IEnumerable<IndexedTriangleWithNormal> triangles)
        {
            foreach (var triangle in triangles)
            {
                var floats = triangle.Normal.ToArray();
                vertices[triangle.A].Normal += new Vector3(floats);
                vertices[triangle.B].Normal += new Vector3(floats);
                vertices[triangle.C].Normal += new Vector3(floats);
            }

            foreach (var v in vertices)
            {
                v.Normal.Normalize();
            }
        }

        #endregion
    }
}