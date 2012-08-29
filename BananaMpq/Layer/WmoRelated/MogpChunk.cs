using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BananaMpq.Geometry;
using BananaMpq.Layer.Chunks;
using BananaMpq.Visualization;
using SharpDX;

namespace BananaMpq.Layer.WmoRelated
{
    public class MogpChunk : Chunk, IChunkCollector
    {
        public const int IsOceanFlag = 0x80000;
        public IList<int> TriangleFlags { get; private set; }
        public IList<Vector3> Vertices { get; private set; }
        public IList<IndexedTriangle> Triangles { get; private set; }
        public int LiquidType { get; private set; }
        public int GroupFlags { get; private set; }
        public MliqChunk Liquid { get; private set; }

        internal unsafe MogpChunk(ChunkHeader* header) : base(header)
        {
            var mogp = (Mogp*)(header + 1);
            LiquidType = mogp->liquidType;
            GroupFlags = mogp->flags;
            var p = (byte*)mogp;
            Chunks = ChunkCollector.CreateChunks(p + sizeof(Mogp), p + header->Size, CreateChunk);
        }

        private unsafe Chunk CreateChunk(ChunkHeader* header)
        {
            switch (header->Magic)
            {
                case "MOPY":
                    return HandleMopy(header);
                case "MOVI":
                    return HandleMovi(header);
                case "MOVT":
                    return HandleMovt(header);
                case "MLIQ":
                    return HandleMliq(header);
                default:
                    return new Chunk(header);
            }
        }

        private unsafe Chunk HandleMliq(ChunkHeader* header)
        {
            var mliq = new MliqChunk(header);
            Liquid = mliq;
            return mliq;
        }

        private unsafe Chunk HandleMovt(ChunkHeader* header)
        {
            var movt = new MovtChunk(header);
            Vertices = movt.Vertices;
            return movt;
        }

        private unsafe Chunk HandleMovi(ChunkHeader* header)
        {
            var movi = new MoviChunk(header);
            Triangles = movi.Triangles;
            return movi;
        }

        private unsafe Chunk HandleMopy(ChunkHeader* header)
        {
            var mopy = new MopyChunk(header);
            TriangleFlags = mopy.TriangleFlags;
            return mopy;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x44)]
        private struct Mogp
        {
            // ReSharper disable FieldCanBeMadeReadOnly.Local
            private int nameOffset;
            private int nameOffsetDescriptive;
            public int flags;
            private BoundingBox bounds;
            private short portalStart;
            private short nPortals;
            private short nBatchesA;
            private short nBatchesB;
            private int nBatchesC;
            private int fogs;
            public int liquidType;
            private int groupId;
            private int unk2;
            private int unk3;
            // ReSharper restore FieldCanBeMadeReadOnly.Local
        }

        public override IEnumerable<System.Reflection.PropertyInfo> VisualizableProperties
        {
            get
            {
                var t = typeof(MogpChunk);
                return base.VisualizableProperties.Concat(new[]
                {
                    t.GetProperty("LiquidType"),
                    t.GetProperty("TriangleFlags"),
                    t.GetProperty("Vertices"),
                    t.GetProperty("Triangles"),
                });
            }
        }

        #region Implementation of IChunkCollector

        public IEnumerable<Chunk> Chunks { get; private set; }

        #endregion
    }
}