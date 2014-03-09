using System;
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
        private static readonly byte[] NoHoles = Enumerable.Repeat((byte)0, 8).ToArray();
        public float[,] HeightMap { get; private set; }
        public bool HasHighResHoles { get { return _flags.HasFlag(McnkHeaderFlags.HighResHoleMap); } }
        public byte[] Holes { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        private McnkHeaderFlags _flags;
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
            return Holes != NoHoles && ((Holes[row] >> col) & 1) == 1;
        }

        private unsafe void ParseMcnkHeader(byte* cur)
        {
            var mcnk = (McnkHeader*)cur;

            _bounds = new BoundingBox(
                mcnk->Position + new Vector3(-ChunkWidth, -ChunkWidth, 0.0f),
                mcnk->Position
            );

            _flags = mcnk->Flags;
            Holes = _flags.HasFlag(McnkHeaderFlags.HighResHoleMap) ? mcnk->HighResHoles : TransformToHighRes(mcnk->Holes);
            if (Holes.All(b => b == 0))
                Holes = NoHoles; // easier to check for

            X = mcnk->X;
            Y = mcnk->Y;
        }

        private static byte[] TransformToHighRes(ushort holes)
        {
            var ret = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int holeIdxL = (i/2)*4 + (j/2);
                    if (((holes >> holeIdxL) & 1) == 1)
                        ret[i] |= (byte)(1 << j);
                }
            }
            return ret;
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

        [Flags]
        private enum McnkHeaderFlags : uint
        {
            HighResHoleMap = 0x10000
        }

#pragma warning disable 169, 649
        private struct McnkHeader
        {
            public McnkHeaderFlags Flags;
            public int X;
            public int Y;
            int nLayers;
            int nDoodadRefs;
            uint offMcvt; // 0x14
            uint offMcnr; // 0x18
            int offMcly;
            int offMcrf;
            int offMcal;
            int sizeAlpha;
            int offMcsh;
            int sizeShadow;
            int areaId;
            int nMapObjRefs;
            public ushort Holes;
            ushort HolesAlign;
            unsafe fixed short lowQualityTexturingMap [8];
            int predTex;
            int noEffectDoodad;
            int offMcse;
            int nSoundEmitters;
            int offMclq;
            int sizeLiquid;
            public Vector3 Position;
            int offMccv;
            unsafe fixed int pad[2];
            public byte[] HighResHoles { get { return BitConverter.GetBytes(offMcvt + ((ulong)offMcnr << 32)); } } // 0x14
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
                    type.GetProperty("HasHighResHoles"),
                    type.GetProperty("Holes"),
                    type.GetProperty("HeightMap"),
                    type.GetProperty("DoodadReferences"),
                    type.GetProperty("WmoReferences"),
                });
            }
        }
    }
}