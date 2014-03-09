using System.Collections.Generic;
using System.Reflection;
using BananaMpq.Visualization;

namespace BananaMpq.Layer.AdtRelated
{
    public class ChunkLiquid : IHasVisualizableProperties
    {
        private static readonly bool[,] EmptyExistsTable = ArrayUtil.MakeTwoDimensionalArray(false, 8, 8);
        private static readonly bool[,] FullExistsTable = ArrayUtil.MakeTwoDimensionalArray(true, 8, 8);
        private const int TilesPerRow = 8;
        private const int VerticesPerRow = (TilesPerRow + 1);
        public float[,] HeightMap { get; private set; }
        public bool[,] ExistsTable { get; private set; }
        public int MinX { get; private set; }
        public int MinY { get; private set; }
        public int MaxX { get; private set; }
        public int MaxY { get; private set; }
        public float MinHeight { get; private set; }
        public float MaxHeight { get; private set; }
        public short Flags { get; private set; }
        public short Type { get; private set; }

        internal unsafe ChunkLiquid(byte* mh2oChunk, SMLiquidChunk* chunk)
        {
            if (AnyLiquidLayer(chunk))
            {
                var instance = GetInstance(mh2oChunk, chunk);
                Flags = instance->liquidObjectId;
                Type = instance->liquidType;
                MinX = MinY = 0;
                MaxY = MaxX = 8;
                MinHeight = instance->minWaterHeight;
                MaxHeight = instance->maxWaterHeight;
                if (instance->liquidObjectId < 42) // most probably for wmos...
                {
                    MinX = instance->xOffset;
                    MinY = instance->yOffset;
                    MaxX = instance->xOffset + instance->width;
                    MaxY = instance->yOffset + instance->height;
                }
                ParseExistsTable(mh2oChunk, instance);
                ParseHeightMap(mh2oChunk, instance);
            }
            else
            {
                ExistsTable = EmptyExistsTable;
                HeightMap = null;
            }
        }

        public bool HasHole(int col, int row)
        {
            return !ExistsTable[row, col];
        }

        private unsafe void ParseHeightMap(byte* mh2oChunk, SMLiquidInstance* instance)
        {
            if (HasHeightMapData(instance) && UseExistsTable(instance->liquidObjectId, instance->liquidType))
            {
                var data = (float*)(mh2oChunk + instance->data);
                HeightMap = new float[VerticesPerRow, VerticesPerRow];
                for (int y = MinY; y <= MaxY; y++)
                {
                    for (int x = MinX; x <= MaxX; x++)
                    {
                        int index = y*(MaxX - MinX + 1) + x;
                        HeightMap[y, x] = data[index];
                    }
                }
            }
            else
            {
                HeightMap = ArrayUtil.MakeTwoDimensionalArray(instance->maxWaterHeight, VerticesPerRow, VerticesPerRow);
            }
        }

        private static bool UseExistsTable(short liquidObjectId, short liquidType)
        {
            return !IsOcean(liquidObjectId, liquidType) && IsHeightMapDataFilling(liquidObjectId, liquidType);
        }

        /// <summary>
        /// Compensates for liquids like [46] in Stormwind, where liquidObjectId is 1709
        /// But there is a counter example... [136] in Stormwind
        /// </summary>
        private static bool IsHeightMapDataFilling(short liquidObjectId, short liquidType)
        {
            return true;
        }

        private static bool IsOcean(short liquidObjectId, short liquidType)
        {
            return liquidObjectId == 42 || liquidType == 2 || liquidType == 14;
        }

        private static unsafe bool HasHeightMapData(SMLiquidInstance* instance)
        {
            return instance->data != 0;
        }

        private unsafe void ParseExistsTable(byte* mh2oChunk, SMLiquidInstance* instance)
        {
            if (instance->existsTable != 0 && UseExistsTable(instance->liquidObjectId, instance->liquidType))
            {
                var table = mh2oChunk + instance->existsTable;
                ExistsTable = new bool[TilesPerRow, TilesPerRow];
                for (int x = MinX; x < MaxX; x++)
                {
                    for (int y = MinY; y < MaxY; y++)
                    {
                        int index = y*(MaxX - MinX) + x;
                        ExistsTable[y, x] = (table[index/8] & (1 << (index & 7))) != 0;
                    }
                }
            }
            else
            {
                ExistsTable = FullExistsTable;
            }
        }

        private static unsafe bool AnyLiquidLayer(SMLiquidChunk* chunk)
        {
            return chunk->layerCount > 0;
        }

        private static unsafe SMLiquidInstance* GetInstance(byte* mh2oChunk, SMLiquidChunk* chunk)
        {
            return (SMLiquidInstance*)(mh2oChunk + chunk->instanceOffset);
        }

        #region Implementation of IHasVisualizableProperties

        public IEnumerable<PropertyInfo> VisualizableProperties
        {
            get
            {
                var type = typeof(ChunkLiquid);
                return new[]
                {
                    type.GetProperty("MinX"), 
                    type.GetProperty("MinY"), 
                    type.GetProperty("MaxX"), 
                    type.GetProperty("MaxY"), 
                    type.GetProperty("MinHeight"), 
                    type.GetProperty("MaxHeight"), 
                    type.GetProperty("ExistsTable"), 
                    type.GetProperty("Flags"), 
                    type.GetProperty("Type"), 
                    type.GetProperty("HeightMap")
                };
            }
        }

        #endregion

    }
}