using System.Collections.Generic;
using BananaMpq.Layer.Chunks;
using SharpDX;

namespace BananaMpq.Layer.AdtRelated
{
    public class MddfChunk : ModelDefinitionChunk
    {
        internal unsafe MddfChunk(ChunkHeader* header) : base(header)
        {
            var begin = (byte*)ChunkHeader.ChunkBegin(header);
            var end = begin + header->Size;
            Definitions = new List<IModelDefinition>(header->Size/sizeof(MddfEntry));

            for (var entry = (MddfEntry*)begin; entry < end; entry++)
            {
                Definitions.Add(new RootModelDefinition
                {
                    Id = entry->uniqueId,
                    ReferenceIndex = entry->mmidIndex,
                    Position = new Vector3(
                        MapOriginOffset - entry->position.Z,
                        MapOriginOffset - entry->position.X,
                        entry->position.Y
                    ),
                    Rotation = entry->rotation,
                    Scale = entry->scale / 1024.0f
                });
            }
        }

#pragma warning disable 169, 649
        struct MddfEntry
        {
            public int mmidIndex;
            public int uniqueId;
            public Vector3 position;
            public Vector3 rotation;
            public short scale;
            public short flags;
        }
#pragma warning restore 169, 649
    }
}