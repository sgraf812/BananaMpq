using BananaMpq.Layer.Chunks;

namespace BananaMpq.Layer.AdtRelated
{
    public class McvtChunk : Chunk
    {
        private const int NonDetailVertices = 9;
        private const int DetailColumns = 8;
        private const int ColumnsPerRow = NonDetailVertices + DetailColumns;

        internal unsafe McvtChunk(ChunkHeader* header)
            : base(header)
        {
            header->ValidateMagic("MCVT");
            HeightMap = new float[NonDetailVertices, NonDetailVertices];
            var values = (float*)((byte*)header + sizeof(ChunkHeader));
            for (int r = 0; r < NonDetailVertices; r++)
            {
                for (int c = 0; c < NonDetailVertices; c++)
                {
                    HeightMap[r, c] = values[r * ColumnsPerRow + c];
                }
            }
        }

        public float[,] HeightMap { get; private set; }
    }
}