using System.Collections.Generic;
using BananaMpq.Layer.AdtRelated;
using BananaMpq.Layer.Chunks;
using BananaMpq.Layer.WdtRelated;
using BananaMpq.Visualization;

namespace BananaMpq.Layer
{
    public class Wdt : IChunkCollector
    {
        public const int AdtsPerSide = 64;
        private MainChunk _main;

        public int Flags { get; private set; }

        public unsafe Wdt(byte[] data)
        {
            fixed (byte* p = data)
            {
                Chunks = ChunkCollector.CreateChunks(p, p + data.Length, CreateChunk);
            }
        }

        public bool AdtExistsForTile(int x, int y)
        {
            return (_main.Flags[y, x] & 1) != 0;
        }

        private unsafe Chunk CreateChunk(ChunkHeader* header)
        {
            switch (header->Magic)
            {
                case "MPHD":
                    var mphd = new MphdChunk(header);
                    Flags = mphd.Flags;
                    return mphd;
                case "MAIN":
                    _main = new MainChunk(header);
                    return _main;
                case "MWMO":
                    return new StringReferenceChunk(header);
                case "MODF":
                    return new ModfChunk(header);
                default:
                    return new Chunk(header);
            }
        }

        #region Implementation of IChunkCollector

        public IEnumerable<Chunk> Chunks { get; private set; }

        #endregion
    }
}