using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BananaMpq.Layer.Chunks;
using BananaMpq.Visualization;
using SharpDX;

namespace BananaMpq.Layer.AdtRelated
{
    public class MapChunk : Chunk, IChunkCollector
    {
        public const float ChunkWidth = Adt.AdtWidth / 16.0f;
        public const int TilesPerChunk = 8;
        public const float TileSize = ChunkWidth / TilesPerChunk;
        public float[,] HeightMap { get; private set; }
        public uint Flags { get; private set; }
        public uint Holes { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        private BoundingBox _bounds;
        public BoundingBox Bounds
        {
            get { return _bounds; }
        }

        public ChunkLiquid Liquid { get; set; }
        public int[] WmoReferences { get; private set; }
        public int[] DoodadReferences { get; private set; }

        internal unsafe MapChunk(ChunkHeader* header)
            : base(header)
        {
            header->ValidateMagic("MCNK");

            var cur = (byte*)header + sizeof(ChunkHeader);
            ParseMcnkHeader(cur);

            Chunks = ChunkCollector.CreateChunks(cur + sizeof(McnkHeader), cur + header->Size, CreateChunks);
        }

        public bool HasHole(int col, int row)
        {
            var holeIndex = (row / 2) * 4 + col / 2;
            return Holes != 0 && ((Holes >> holeIndex) & 1) == 1;
        }

        private unsafe void ParseMcnkHeader(byte* cur)
        {
            var mcnk = (McnkHeader*)cur;

            _bounds = new BoundingBox(
                mcnk->Position + new Vector3(-ChunkWidth, -ChunkWidth, 0.0f),
                mcnk->Position
            );

            Holes = mcnk->Holes;
            Flags = mcnk->Flags;
            X = mcnk->X;
            Y = mcnk->Y;
        }

        private unsafe Chunk CreateChunks(ChunkHeader* header)
        {
            switch (header->Magic)
            {
                case "MCVT":
                    var mcvt = new McvtChunk(header);
                    HeightMap = mcvt.HeightMap;
                    _bounds.Maximum.Z += HeightMap.Cast<float>().Max();
                    return mcvt;
                case "MCRD":
                    var mcrd = new OffsetChunk(header);
                    DoodadReferences = mcrd.Offsets;
                    return mcrd;
                case "MCRW":
                    var mcrw = new OffsetChunk(header);
                    WmoReferences = mcrw.Offsets;
                    return mcrw;
                default:
                    return new Chunk(header);
            }
        }

        internal unsafe void ParseOptionalData(ChunkHeader* header)
        {
            header->ValidateMagic("MCNK");

            var cur = (byte*)header + sizeof(ChunkHeader);
            Chunks = Chunks.Concat(ChunkCollector.CreateChunks(cur, cur + header->Size, CreateChunks));
        }

        #region Implementation of IChunkCollector

        public IEnumerable<Chunk> Chunks { get; private set; }

        #endregion

#pragma warning disable 169, 649
        private struct McnkHeader
        {
            public uint Flags;
            public int X;
            public int Y;
            int nLayers;
            int nDoodadRefs;
            int offMcvt;
            int offMcnr;
            int offMcly;
            int offMcrf;
            int offMcal;
            int sizeAlpha;
            int offMcsh;
            int sizeShadow;
            int areaId;
            int nMapObjRefs;
            public uint Holes;
            unsafe fixed short lowQualityTexturingMap [8];
            int predTex;
            int noEffectDoodad;
            int offMcse;
            int nSoundEmitters;
            int offMclq;
            int sizeLiquid;
            public Vector3 Position;
            int offMccv;
            unsafe fixed int pad [2];
        }
#pragma warning restore 169, 649

        public override IEnumerable<PropertyInfo> VisualizableProperties
        {
            get
            {
                var type = typeof(MapChunk);
                return base.VisualizableProperties.Concat(new[]
                {
                    type.GetProperty("Bounds"),
                    type.GetProperty("X"),
                    type.GetProperty("Y"),
                    type.GetProperty("Flags"),
                    type.GetProperty("Holes"),
                    type.GetProperty("HeightMap"),
                    type.GetProperty("DoodadReferences"),
                    type.GetProperty("WmoReferences"),
                });
            }
        }
    }
}