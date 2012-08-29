using System.Collections.Generic;
using System.Runtime.InteropServices;
using BananaMpq.Layer.Chunks;

namespace BananaMpq.Layer.WmoRelated
{
    public class MopyChunk : Chunk
    {
        public const int NoCollision = 4;
        public IList<int> TriangleFlags { get; private set; }

        internal unsafe MopyChunk(ChunkHeader* header) : base(header)
        {
            ParseCollision((Mopy*)ChunkHeader.ChunkBegin(header), header->Size/sizeof(Mopy));
        }

        private unsafe void ParseCollision(Mopy* mopys, int triangleCount)
        {
            TriangleFlags = new List<int>(triangleCount);
            for (int i = 0; i < triangleCount; i++)
            {
                TriangleFlags.Add(mopys[i].flags);
            }
        }

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable MemberCanBePrivate.Local
        [StructLayout(LayoutKind.Sequential, Size = 2)]
        private struct Mopy
        {
            public byte flags;
            public byte materialId;
        }
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        // ReSharper restore MemberCanBePrivate.Local
    }
}