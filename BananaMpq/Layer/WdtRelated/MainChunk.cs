using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BananaMpq.Layer.Chunks;

namespace BananaMpq.Layer.WdtRelated
{
    public class MainChunk : Chunk
    {
        public int[,] Flags { get; private set; }

        internal unsafe MainChunk(ChunkHeader* header) : base(header)
        {
            ParseFlags((SMAreaInfo*)ChunkHeader.ChunkBegin(header));
        }

        private unsafe void ParseFlags(SMAreaInfo* areaInfos)
        {
            Flags = new int[Wdt.AdtsPerSide, Wdt.AdtsPerSide];
            for (int r = 0; r < Wdt.AdtsPerSide; r++)
            {
                for (int c = 0; c < Wdt.AdtsPerSide; c++)
                {
                    var index = r*Wdt.AdtsPerSide + c;
                    Flags[r, c] = areaInfos[index].flags;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 8)]
        private struct SMAreaInfo
        {
            // ReSharper disable MemberCanBePrivate.Local, FieldCanBeMadeReadOnly.Local
            public int flags;
            private int unused;
            // ReSharper restore MemberCanBePrivate.Local, FieldCanBeMadeReadOnly.Local
        }

        public override IEnumerable<System.Reflection.PropertyInfo> VisualizableProperties
        {
            get
            {
                return base.VisualizableProperties.Concat(new[]
                {
                    typeof(MainChunk).GetProperty("Flags")
                });
            }
        }
    }
}