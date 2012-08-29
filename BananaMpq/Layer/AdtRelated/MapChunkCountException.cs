using System;
using System.Runtime.Serialization;
using BananaMpq.Layer.Chunks;

namespace BananaMpq.Layer.AdtRelated
{
    [Serializable]
    public class MapChunkCountException : ChunkValidationException<int>
    {
        public MapChunkCountException(int expected, int actual)
            : base(expected, actual)
        {
        }

        public MapChunkCountException(string message) : base(message)
        {
        }

        public MapChunkCountException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MapChunkCountException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}