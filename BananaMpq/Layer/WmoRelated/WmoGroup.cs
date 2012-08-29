using System;
using System.Collections.Generic;
using System.Reflection;
using BananaMpq.Geometry;
using BananaMpq.Layer.Chunks;
using BananaMpq.Visualization;
using SharpDX;

namespace BananaMpq.Layer.WmoRelated
{
    public class WmoGroup : IChunkCollector, IHasVisualizableProperties
    {
        private MogpChunk _mogp;
        public IList<int> TriangleFlags { get; private set; }
        public IList<IndexedTriangle> Triangles { get; private set; }
        public IList<Vector3> Vertices { get; private set; }
        public MliqChunk Liquid { get; private set; }

        public unsafe WmoGroup(byte[] data)
        {
            fixed (byte* p = data)
            {
                Chunks = ChunkCollector.CreateChunks(p, p + data.Length, CreateChunk);
            }
        }

        private unsafe Chunk CreateChunk(ChunkHeader* header)
        {
            switch (header->Magic)
            {
                case "MOGP":
                    return HandleMogp(header);
                default:
                    return new Chunk(header);
            }
        }

        private unsafe Chunk HandleMogp(ChunkHeader* header)
        {
            _mogp = new MogpChunk(header);
            TriangleFlags = _mogp.TriangleFlags;
            Vertices = _mogp.Vertices;
            Triangles = _mogp.Triangles;
            Liquid = _mogp.Liquid;
            return _mogp;
        }

        public int DetermineLiquidType()
        {
            var type = _mogp.LiquidType;
            if (type < 21)
            {
                var typeClass = MapToLiquidTypeClass(type);
                switch(typeClass)
                {
                    case LiquidTypeClass.Water:
                        return (_mogp.GroupFlags & MogpChunk.IsOceanFlag) == 0 ? 13 : 14;
                    case LiquidTypeClass.Ocean:
                        return 14;
                    case LiquidTypeClass.Magma:
                        return 19;
                    case LiquidTypeClass.Slime:
                        return 20;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return type;
        }

        public static LiquidTypeClass MapToLiquidTypeClass(int type)
        {
            if (type > 20)
                throw new ArgumentOutOfRangeException("Algorithm does not generally support types greater than 20 (Naxxramas slime)");
            return (LiquidTypeClass)((type - 1) & 3);
        }

        #region Implementation of IChunkCollector

        public IEnumerable<Chunk> Chunks { get; private set; }

        #endregion

        #region Implementation of IHasVisualizableProperties

        public IEnumerable<PropertyInfo> VisualizableProperties
        {
            get
            {
                return new[]
                {
                    typeof(WmoGroup).GetProperty("TriangleFlags"),
                    typeof(WmoGroup).GetProperty("Vertices"),
                    typeof(WmoGroup).GetProperty("Triangles"),
                    typeof(WmoGroup).GetProperty("Liquid"),
                };
            }
        }

        #endregion
    }
}