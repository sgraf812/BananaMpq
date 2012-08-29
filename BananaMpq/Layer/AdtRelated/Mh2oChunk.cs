using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BananaMpq.Layer.Chunks;

namespace BananaMpq.Layer.AdtRelated
{
    public class Mh2oChunk : Chunk
    {
        internal unsafe Mh2oChunk(ChunkHeader* header) : base(header)
        {
            var begin = (byte*)ChunkHeader.ChunkBegin(header);
            var currentChunk = (SMLiquidChunk*)begin;
            Liquids = new List<ChunkLiquid>(Adt.McnksPerAdt);
            for (int i = 0; i < Adt.McnksPerAdt; i++)
            {
                Liquids.Add(new ChunkLiquid(begin, currentChunk++));
            }
        }

        public IList<ChunkLiquid> Liquids { get; private set; }

        public override IEnumerable<PropertyInfo>  VisualizableProperties
        {
            get
            {
                var type = typeof(Mh2oChunk);
                return base.VisualizableProperties.Concat(new[] {type.GetProperty("Liquids")});
            }
        }
    }
}