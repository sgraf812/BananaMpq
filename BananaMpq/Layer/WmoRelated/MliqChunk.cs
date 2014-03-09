using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BananaMpq.Layer.Chunks;
using SharpDX;

namespace BananaMpq.Layer.WmoRelated
{
    public class MliqChunk : Chunk
    {
        public Vector3 Position { get; private set; }
        public float[,] HeightMap { get; private set; }
        public bool[,] ExistsTable { get; private set; }

        internal unsafe MliqChunk(ChunkHeader* header) : base(header)
        {
            var mliq = (Mliq*)ChunkHeader.ChunkBegin(header);
            Position = mliq->position;
            var entries = (HeightMapEntry*)(mliq + 1);
            var columns = mliq->vertexColumns;
            var rows = mliq->vertexRows;
            ParseHeightMap(entries, columns, rows);
            var flags = (byte*)(entries + columns*rows);
            ParseExistsTable(flags, columns - 1, rows - 1);
        }

        /// <summary>
        /// Both ExistsTable and heightmap seem to be stored in column major order
        /// </summary>
        private unsafe void ParseExistsTable(byte* flags, int columns, int rows)
        {
            ExistsTable = new bool[columns, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    var cur = flags[r*columns + c];
                    ExistsTable[c, r] = (cur & 15) != 15;
                }
            }
        }

        /// <summary>
        /// Both ExistsTable and heightmap seem to be stored in column major order
        /// </summary>
        private unsafe void ParseHeightMap(HeightMapEntry* entries, int columns, int rows)
        {
            HeightMap = new float[columns, rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    var height = entries[r*columns + c].value;
                    HeightMap[c, r] = height;
                }
            }
        }

        // ReSharper disable FieldCanBeMadeReadOnly.Local
        [StructLayout(LayoutKind.Sequential, Size = 0x1E)]
        private struct Mliq
        {
            public int vertexColumns;
            public int vertexRows;
            private int tileCountX;
            private int tileCountY;
            public Vector3 position;
            private short materialId;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x8)]
        private struct HeightMapEntry
        {
            private short unknown1;
            private short unknown2;
            public float value;
        }
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        public override IEnumerable<System.Reflection.PropertyInfo> VisualizableProperties
        {
            get
            {
                var t = typeof(MliqChunk);
                return base.VisualizableProperties.Concat(new[]
                {
                    t.GetProperty("Position"),
                    t.GetProperty("HeightMap"),
                    t.GetProperty("ExistsTable"),
                });
            }
        }
    }
}