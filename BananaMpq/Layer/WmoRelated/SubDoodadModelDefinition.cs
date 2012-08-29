using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BananaMpq.Layer.Chunks;
using SharpDX;

namespace BananaMpq.Layer.WmoRelated
{
    public class SubDoodadModelDefinition : ModelDefinition
    {
        public Quaternion Rotation { get; set; }
        public int ReferenceOffset { get; set; }

        protected override Matrix RotationMatrix
        {
            get { return Matrix.RotationQuaternion(Rotation); }
        }

        public override string GetModelReference(IList<StringReference> references)
        {
            return FixMdxToM2(references.First(r => r.Offset == ReferenceOffset).String);
        }

        private static string FixMdxToM2(string doodadRef)
        {
            return doodadRef.ToUpper().Split('.')[0] + ".M2";
        }

        #region Implementation of IHasVisualizableProperties

        public override IEnumerable<PropertyInfo> VisualizableProperties
        {
            get
            {
                var t = typeof(SubDoodadModelDefinition);
                return base.VisualizableProperties.Concat(new[]
                {
                    t.GetProperty("ReferenceOffset"),
                    t.GetProperty("Rotation"),
                });
            }
        }


        #endregion
    }
}