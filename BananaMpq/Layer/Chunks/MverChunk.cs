using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BananaMpq.Layer.Chunks
{
    public class MverChunk : Chunk
    {
        public int Version { get; private set; }

        internal unsafe MverChunk(ChunkHeader* header) : base(header)
        {
            Version = *(int*)(header + 1);
        }

        public override IEnumerable<PropertyInfo> VisualizableProperties
        {
            get
            {
                var type = typeof(MverChunk);
                return base.VisualizableProperties.Concat(new[]{type.GetProperty("Version")});
            }
        }
    }
}