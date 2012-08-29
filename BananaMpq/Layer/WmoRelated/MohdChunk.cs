using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BananaMpq.Layer.Chunks;
using SharpDX;

namespace BananaMpq.Layer.WmoRelated
{
    public class MohdChunk: Chunk
    {
        public BoundingBox Bounds { get; private set; }
        public int GroupCount { get; private set; }
        public int DoodadCount { get; private set; }

        internal unsafe MohdChunk(ChunkHeader* header) : base(header)
        {
            var mohd = (Mohd*)(header + 1);
            Bounds = mohd->bounds;
            GroupCount = mohd->groupCount;
            DoodadCount = mohd->doodadCount;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x40)]
        private struct Mohd
        {
            private int textureCount;		
            public int groupCount;
            private int portalCount;
            private int lightCount;
            public int doodadCount;
            private int modelFileCount;
            private int doodadSetCount;
            private byte colR;
            private byte colG;
            private byte colB;
            private byte colX;
            private int wmoID;
            public BoundingBox bounds;
        }

        public override IEnumerable<System.Reflection.PropertyInfo> VisualizableProperties
        {
            get
            {
                var t = typeof(MohdChunk);
                return base.VisualizableProperties.Concat(new[]
                {
                    t.GetProperty("Bounds"),
                    t.GetProperty("GroupCount"),
                    t.GetProperty("DoodadCount"),
                });
            }
        }
    }
}