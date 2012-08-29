using System.Collections.Generic;
using BananaMpq.Layer.Chunks;
using SharpDX;

namespace BananaMpq.Geometry.Builders
{
    public class DoodadBuilder : ModelBuilder
    {
        private readonly ISet<int> _builtIds = new SortedSet<int>(); 

        public DoodadBuilder(FilePool files) : base(files)
        {
        }

        public IEnumerable<SceneObject> BuildDoodads(IEnumerable<IModelDefinition> definitions, IList<StringReference> references, 
            RectangleF bounds, Matrix? rootTransform = null)
        {
            foreach (var definition in definitions)
            {
                if (definition.Id != null && !_builtIds.Add(definition.Id.Value)) continue;
                var doodad = Files.GetDoodad(definition.GetModelReference(references));
                if (doodad.Triangles.Length == 0) continue;

                var transform = definition.GetTranform();
                if (rootTransform.HasValue) transform = transform * rootTransform.Value;

                var sceneObject = BuildModelFromTransform(doodad.Vertices, doodad.Triangles, transform);
                if (bounds.Intersects(sceneObject.Bounds)) yield return sceneObject;
            }
        }
    }
}