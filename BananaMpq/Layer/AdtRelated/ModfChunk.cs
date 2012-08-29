using System.Collections.Generic;
using BananaMpq.Layer.Chunks;
using SharpDX;

namespace BananaMpq.Layer.AdtRelated
{
    public class ModfChunk : ModelDefinitionChunk
    {
        internal unsafe ModfChunk(ChunkHeader* header) : base(header)
        {
            var begin = (byte*)ChunkHeader.ChunkBegin(header);
            var end = begin + header->Size;
            Definitions = new List<IModelDefinition>(header->Size / sizeof(ModfEntry));
            
            for (var entry = (ModfEntry*)begin; entry < end; entry++)
            {
                Definitions.Add(new RootModelDefinition
                {
                    Id = entry->uniqueId,
                    ReferenceIndex = entry->mwidEntry,
                    Position = new Vector3(
                        MapOriginOffset - entry->position.Z,
                        MapOriginOffset - entry->position.X,
                        entry->position.Y
                    ),
                    Rotation = entry->rotation,
                    Scale = 1.0f
                });
            }
        }

#pragma warning disable 169, 649
        struct ModfEntry
        {
            public int mwidEntry; 
            public int uniqueId;   
            public Vector3 position;
            public Vector3 rotation; 
            public Vector3 lowerBounds; 
            public Vector3 upperBounds;  
            public short flags;  
            public short doodadSet;
            public short nameSet;             
            public short padding;
        }
#pragma warning restore 169, 649
    }
}