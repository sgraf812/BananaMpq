using System.Collections.Generic;
using System.Linq;
using BananaMpq.Layer.Chunks;
using SharpDX;

namespace BananaMpq.Layer.WmoRelated
{
    public class MovtChunk : Chunk
    {
        public IList<Vector3> Vertices { get; private set; }

        internal unsafe MovtChunk(ChunkHeader* header)
            : base(header)
        {
            ParseVertices((Vector3*)ChunkHeader.ChunkBegin(header), header->Size/sizeof(Vector3));
        }

        private unsafe void ParseVertices(Vector3* vertices, int vertexCount)
        {
            Vertices = new List<Vector3>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
            {
                Vertices.Add(vertices[i]);
            }
        }

        public override IEnumerable<System.Reflection.PropertyInfo> VisualizableProperties
        {
            get
            {
                return base.VisualizableProperties.Concat(new[]
                {
                    typeof(MovtChunk).GetProperty("Vertices")
                });
            }
        }
    }
}