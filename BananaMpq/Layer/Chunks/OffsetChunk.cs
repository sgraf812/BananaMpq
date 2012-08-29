using System.Collections.Generic;
using System.Linq;

namespace BananaMpq.Layer.Chunks
{
    public class OffsetChunk : Chunk
    {
        public int[] Offsets { get; private set; }

        internal unsafe OffsetChunk(ChunkHeader* header) : base(header)
        {
            Offsets = new int[header->Size/sizeof(int)];
            var cur = (int*)ChunkHeader.ChunkBegin(header);

            for (var i = 0; i < Offsets.Length; i++)
            {
                Offsets[i] = *cur++;
            }
        }

        public override IEnumerable<System.Reflection.PropertyInfo> VisualizableProperties
        {
            get
            {
                return base.VisualizableProperties.Concat(new[]
                {
                    typeof(OffsetChunk).GetProperty("Offsets")
                });
            }
        }
    }
}