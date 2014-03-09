using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BananaMpq.Layer.Chunks;
using SharpDX;

namespace BananaMpq.Layer.AdtRelated
{
    public class RootModelDefinition : ModelDefinition
    {
        public int ReferenceIndex { get; set; }
        public Vector3 Rotation { get; set; }
        public short Flags { get; set; }

        protected override Matrix RotationMatrix
        {
            get
            {
                var rz = Matrix.RotationZ(ToRadians(Rotation.Y + 180.0f));
                var ry = Matrix.RotationY(ToRadians(Rotation.X));
                var rx = Matrix.RotationX(ToRadians(Rotation.Z));
                return rx*ry*rz;
            }
        }

        public override string GetModelReference(IList<StringReference> references)
        {
            return references[ReferenceIndex].String;
        }

        private static float ToRadians(float degree)
        {
            return (float)(degree*Math.PI/180.0);
        }

        #region Implementation of IHasVisualizableProperties

        public override IEnumerable<PropertyInfo> VisualizableProperties
        {
            get
            {
                var t = typeof(RootModelDefinition);
                return base.VisualizableProperties.Concat(new[]
                {
                    t.GetProperty("ReferenceIndex"),
                    t.GetProperty("Rotation"),
                });
            }
        }

        #endregion
    }
}