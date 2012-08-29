using System;
using System.Runtime.Serialization;
using BananaMpq.Layer.Chunks;

namespace BananaMpq.Layer.WmoRelated
{
    [Serializable]
    public class WmoVersionException : ChunkValidationException<int>
    {
        public WmoVersionException(int expected, int actual) : base(expected,actual)
        {
        }

        public WmoVersionException(string message) : base(message)
        {
        }

        public WmoVersionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected WmoVersionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}