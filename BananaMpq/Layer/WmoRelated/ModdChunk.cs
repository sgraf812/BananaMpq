using System.Collections.Generic;
using System.Runtime.InteropServices;
using BananaMpq.Layer.Chunks;
using SharpDX;

namespace BananaMpq.Layer.WmoRelated
{
    public class ModdChunk : ModelDefinitionChunk
    {
        internal unsafe ModdChunk(ChunkHeader* header) : base(header)
        {
            ParseDefinitions((Modd*)ChunkHeader.ChunkBegin(header), header->Size/sizeof(Modd));
        }

        private unsafe void ParseDefinitions(Modd* modd, int definitionCount)
        {
            Definitions = new List<IModelDefinition>(definitionCount);
            for (int i = 0; i < definitionCount; i++)
            {
                Definitions.Add(new SubDoodadModelDefinition
                {
                    Id = null,
                    ReferenceOffset = (modd->referenceOffset & 0xFFFFFF),
                    Position = modd->position,
                    Rotation = modd->rotation,
                    Scale = modd->scale
                });
                modd++;
            }
        }

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        [StructLayout(LayoutKind.Sequential, Size = 0x28)]
        private struct Modd
        {
            public int referenceOffset;
            public Vector3 position;
            public Quaternion rotation;
            public float scale;
        }
        // ReSharper restore FieldCanBeMadeReadOnly.Local
    }
}