using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BananaMpq.Layer.WmoRelated;
using BananaMpq.Visualization;
using SharpDX;

namespace BananaMpq.Layer.Chunks
{
    public abstract class ModelDefinition : IHasVisualizableProperties, IModelDefinition
    {
        public int? Id { get; set; }
        public Vector3 Position { get; set; }
        public float Scale { get; set; }
        public int? UsedDoodadSetIndex { private get; set; }

        protected abstract Matrix RotationMatrix { get; }
        public abstract string GetModelReference(IList<StringReference> references);

        public IEnumerable<IModelDefinition> FilterDoodadSetDefinitions(IList<DoodadSet> sets, IEnumerable<IModelDefinition> doodadDefs)
        {
            if (!UsedDoodadSetIndex.HasValue) return new IModelDefinition[0];
            var first = sets[UsedDoodadSetIndex.Value].FirstDefinition;
            var count = sets[UsedDoodadSetIndex.Value].DefinitionCount;
            return doodadDefs.Skip(first).Take(count);
        }

        public Matrix GetTranform()
        {
            return Matrix.Scaling(Scale) * RotationMatrix * Matrix.Translation(Position);
        }

        public virtual IEnumerable<PropertyInfo> VisualizableProperties
        {
            get
            {
                var t = typeof(ModelDefinition);
                return new[]
                {
                    t.GetProperty("Id"),
                    t.GetProperty("Position"),
                    t.GetProperty("Scale"),
                    t.GetProperty("UsedDoodadSetIndex"),
                };
            }
        }
    }
}