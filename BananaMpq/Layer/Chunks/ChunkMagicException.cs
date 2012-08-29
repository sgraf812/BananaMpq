using System;
using System.Runtime.Serialization;

namespace BananaMpq.Layer.Chunks
{
    [Serializable]
    public class ChunkMagicException : ChunkValidationException<string>
    {
        public ChunkMagicException(string expected, string actual)
            : base(expected, actual)
        {
        }

        public ChunkMagicException(string message) : base(message)
        {
        }

        public ChunkMagicException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ChunkMagicException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}