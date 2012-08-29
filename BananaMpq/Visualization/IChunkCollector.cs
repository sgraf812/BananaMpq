using System.Collections.Generic;
using BananaMpq.Layer.Chunks;

namespace BananaMpq.Visualization
{
    public interface IChunkCollector
    {
        IEnumerable<Chunk> Chunks { get; }
    }
}