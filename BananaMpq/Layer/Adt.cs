using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BananaMpq.Layer.AdtRelated;
using BananaMpq.Layer.Chunks;
using BananaMpq.Visualization;
using SharpDX;

namespace BananaMpq.Layer
{
    public class Adt : IChunkCollector, IHasVisualizableProperties
    {
        private static readonly BoundingBox Nothing = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
        private static readonly IList<IModelDefinition> EmptyDefinitions = new IModelDefinition[0];
        private static readonly IList<StringReference> EmptyReferences = new StringReference[0];
        public const int Version = 18;
        public const float AdtWidth = 1600.0f / 3.0f;
        public const int McnksPerSide = 16;
        public const int McnksPerAdt = McnksPerSide*McnksPerSide;
        private Mh2oChunk _mh2o;
        private int _mcnkCounter;
        private StringReferenceChunk _mmdx;
        private StringReferenceChunk _mwmo;
        private MddfChunk _mddf;
        private ModfChunk _modf;
        public int X { get; private set; }
        public int Y { get; private set; }
        public BoundingBox Bounds { get; private set; }
        public IList<MapChunk> MapChunks { get; private set; }
        public IList<IModelDefinition> WmoDefinitions { get; private set; }
        public IList<StringReference> WmoReferences { get; private set; }
        public IList<IModelDefinition> DoodadDefinitions { get; private set; }
        public IList<StringReference> DoodadReferences { get; private set; }

        public unsafe Adt(byte[] data)
        {
            MapChunks = new List<MapChunk>(McnksPerAdt);
            fixed (byte* p = data)
            {
                Chunks = ChunkCollector.CreateChunks(p, p + data.Length, CreateChunk);
            }

            if (MapChunks.Count != McnksPerAdt)
            {
                throw new MapChunkCountException(McnksPerAdt, MapChunks.Count);
            }

            var mid = MapChunks[120].Bounds.Minimum;
            X = (int)Math.Floor(32.0f - mid.Y / AdtWidth);
            Y = (int)Math.Floor(32.0f - mid.X / AdtWidth);
            Bounds = RoundToAdtBounds(MapChunks.Aggregate(Nothing, (b, c) => BoundingBox.Merge(b, c.Bounds)));
            TrySetChunkLiquids();
            WmoDefinitions = DoodadDefinitions = EmptyDefinitions;
            WmoReferences = DoodadReferences = EmptyReferences;
        }

        private static BoundingBox RoundToAdtBounds(BoundingBox bounds)
        {
            bounds.Minimum.X = (float)(Math.Round(bounds.Minimum.X/AdtWidth)*AdtWidth);
            bounds.Minimum.Y = (float)(Math.Round(bounds.Minimum.Y/AdtWidth)*AdtWidth);
            bounds.Maximum.X = (float)(Math.Round(bounds.Maximum.X/AdtWidth)*AdtWidth);
            bounds.Maximum.Y = (float)(Math.Round(bounds.Maximum.Y/AdtWidth)*AdtWidth);
            return bounds;
        }

        private void TrySetChunkLiquids()
        {
            if (_mh2o != null && MapChunks.Count > 0)
            {
                for (int i = 0; i < _mh2o.Liquids.Count; i++)
                {
                    MapChunks[i].Liquid = _mh2o.Liquids[i];
                }
            }
        }

        private unsafe Chunk CreateChunk(ChunkHeader* header)
        {
            switch (header->Magic)
            {
                case "MVER":
                    return HandleMver(header);
                case "MH2O":
                    return HandleMh2o(header);
                case "MCNK":
                    return HandleMcnk(header);
                default:
                    return new Chunk(header);
            }
        }

        private unsafe Chunk HandleMh2o(ChunkHeader* header)
        {
            _mh2o = new Mh2oChunk(header);
            return _mh2o;
        }

        private unsafe Chunk HandleMcnk(ChunkHeader* header)
        {
            var chunk = new MapChunk(header);
            MapChunks.Add(chunk);
            return chunk;
        }

        private static unsafe MverChunk HandleMver(ChunkHeader* header)
        {
            var mver = new MverChunk(header);
            if (mver.Version != Version)
                throw new AdtVersionException(Version, mver.Version);
            return mver;
        }

        public unsafe void ParseSecondaryData(byte[] data)
        {
            fixed (byte* p = data)
            {
                _mcnkCounter = 0;
                Chunks = Chunks.Concat(ChunkCollector.CreateChunks(p, p + data.Length, ExtendChunk));
            }
        }

        private unsafe Chunk ExtendChunk(ChunkHeader* header)
        {
            if (header->Size == 0) return null;

            switch (header->Magic)
            {
                case "MVER":
                    HandleMver(header);
                    return null;
                case "MH2O":
                    return null;
                case "MCNK":
                    MapChunks[_mcnkCounter++].ParseOptionalData(header);
                    return null;
                case "MWID":
                    return new OffsetChunk(header);
                case "MMID":
                    return new OffsetChunk(header);
                case "MMDX":
                    return HandleMmdx(header);
                case "MWMO":
                    return HandleMwmo(header);
                case "MDDF":
                    return HandleMddf(header);
                case "MODF":
                    return HandleModf(header);
                default:
                    return new Chunk(header);
            }
        }

        private unsafe Chunk HandleMwmo(ChunkHeader* header)
        {
            _mwmo = new StringReferenceChunk(header);
            WmoReferences = _mwmo.Strings;
            return _mwmo;
        }

        private unsafe Chunk HandleMmdx(ChunkHeader* header)
        {
            _mmdx = new StringReferenceChunk(header);
            DoodadReferences = _mmdx.Strings;
            return _mmdx;
        }

        private unsafe Chunk HandleModf(ChunkHeader* header)
        {
            _modf = new ModfChunk(header);
            WmoDefinitions = _modf.Definitions;
            return _modf;
        }

        private unsafe Chunk HandleMddf(ChunkHeader* header)
        {
            _mddf = new MddfChunk(header);
            DoodadDefinitions = _mddf.Definitions;
            return _mddf;
        }

        #region Implementation of IChunkCollector

        public IEnumerable<Chunk> Chunks { get; private set; }

        #endregion

        #region Implementation of IHasVisualizableProperties

        public IEnumerable<PropertyInfo> VisualizableProperties
        {
            get
            {
                var type = typeof(Adt);
                return new[]
                {
                    type.GetProperty("X"),
                    type.GetProperty("Y"),
                    type.GetProperty("WmoReferences"),
                    type.GetProperty("WmoDefinitions"),
                    type.GetProperty("DoodadReferences"),
                    type.GetProperty("DoodadDefinitions"),
                };
            }
        }

        #endregion
    }
}