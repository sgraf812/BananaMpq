using System.Collections.Generic;
using System.Runtime.InteropServices;
using BananaMpq.Layer.Chunks;

namespace BananaMpq.Layer.WmoRelated
{
    public class ModsChunk : Chunk
    {
        internal unsafe ModsChunk(ChunkHeader* header)
            : base(header)
        {
            ParseSets((Mods*)ChunkHeader.ChunkBegin(header), header->Size / sizeof(Mods));
        }

        private unsafe void ParseSets(Mods* set, int count)
        {
            DoodadSets = new List<DoodadSet>(count);
            for (int i = 0; i < count; i++)
            {
                DoodadSets.Add(new DoodadSet
                {
                    Name = MiscUtils.ConvertToAscii(set->name, Mods.NameLength),
                    FirstDefinition = set->firstInstanceIndex,
                    DefinitionCount = set->instanceCount
                });
                set++;
            }
        }

        public IList<DoodadSet> DoodadSets { get; private set; }

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        [StructLayout(LayoutKind.Sequential, Size = 0x20, CharSet = CharSet.Ansi)]
        private unsafe struct Mods
        {
            public const int NameLength = 20;
            public fixed byte name[NameLength];
            public int firstInstanceIndex;
            public int instanceCount;
        }
        // ReSharper restore FieldCanBeMadeReadOnly.Local
    }
}