using System.Collections.Generic;
using System.Linq;
using BananaMpq.Geometry;
using BananaMpq.Layer.Chunks;

namespace BananaMpq.Layer.WmoRelated
{
    public class MoviChunk : Chunk
    {
        public IList<IndexedTriangle> Triangles { get; private set; }

        internal unsafe MoviChunk(ChunkHeader* header) : base(header)
        {
            ParseTriangles((ushort*)ChunkHeader.ChunkBegin(header), header->Size/sizeof(ushort));
        }

        private unsafe void ParseTriangles(ushort* indices, int indexCount)
        {
            Triangles = new List<IndexedTriangle>(indexCount/3);
            for (var i = 0; i < indexCount; i+=3)
            {
                Triangles.Add(new IndexedTriangle
                {
                    A = indices[i],
                    B = indices[i+1],
                    C = indices[i+2],
                });
            }
        }

        public override IEnumerable<System.Reflection.PropertyInfo> VisualizableProperties
        {
            get
            {
                return base.VisualizableProperties.Concat(new[]
                {
                    typeof(MoviChunk).GetProperty("Triangles")
                });
            }
        }
    }
}