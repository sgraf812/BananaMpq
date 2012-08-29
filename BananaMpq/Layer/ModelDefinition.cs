using System.Collections.Generic;
using System.Reflection;
using BananaMpq.Layer.Chunks;
using BananaMpq.Visualization;
using SharpDX;

namespace BananaMpq.Layer
{
    public abstract class ModelDefinition : IHasVisualizableProperties, IModelDefinition
    {
        public int? Id { get; set; }
        public Vector3 Position { get; set; }
        public float Scale { get; set; }

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
                };
            }
        }

        protected abstract Matrix RotationMatrix { get; }
        public abstract string GetModelReference(IList<StringReference> references);

        public Matrix GetTranform()
        {
            return Matrix.Scaling(Scale) * RotationMatrix * Matrix.Translation(Position);
        }
    }
}