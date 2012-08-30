using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BananaMpq.Layer.Chunks;
using BananaMpq.Layer.WmoRelated;
using BananaMpq.Visualization;

namespace BananaMpq.Layer
{
    public class Wmo : IChunkCollector, IHasVisualizableProperties
    {
        private const int Version = 17;
        private MohdChunk _mohd;

        public IList<WmoGroup> Groups { get; private set; }
        public IList<DoodadSet> DoodadSets { get; private set; }
        public IList<StringReference> DoodadReferences { get; private set; } 
        public IList<IModelDefinition> DoodadDefinitions { get; private set; } 

        public unsafe Wmo(byte[] data, Func<int, WmoGroup> groupFactory)
        {
            fixed (byte* p = data)
            {
                Chunks = ChunkCollector.CreateChunks(p, p + data.Length, CreateChunk);
            }

            Groups = Enumerable.Range(0, _mohd.GroupCount).Select(groupFactory).ToArray();
        }

        private unsafe Chunk CreateChunk(ChunkHeader* header)
        {
            switch (header->Magic)
            {
                case "MVER":
                    return HandleMver(header);
                case "MOHD":
                    return _mohd = new MohdChunk(header);
                case "MODS":
                    return HandleMods(header);
                case "MODN":
                    return HandleModn(header);
                case "MODD":
                    return HandleModd(header);
                default:
                    return new Chunk(header);
            }
        }

        private unsafe Chunk HandleMods(ChunkHeader* header)
        {
            var mods = new ModsChunk(header);
            DoodadSets = mods.DoodadSets;
            return mods;
        }

        private unsafe Chunk HandleModn(ChunkHeader* header)
        {
            var modn = new StringReferenceChunk(header);
            DoodadReferences = modn.Strings;
            return modn;
        }

        private unsafe Chunk HandleModd(ChunkHeader* header)
        {
            var modd = new ModdChunk(header);
            DoodadDefinitions = modd.Definitions;
            return modd;
        }

        private static unsafe Chunk HandleMver(ChunkHeader* header)
        {
            var mver = new MverChunk(header);
            if (mver.Version != Version)
                throw new WmoVersionException(Version, mver.Version);
            return mver;
        }

        #region Implementation of IChunkCollector

        public IEnumerable<Chunk> Chunks { get; private set; }

        #endregion

        #region Implementation of IHasVisualizableProperties

        public IEnumerable<PropertyInfo> VisualizableProperties
        {
            get
            {
                var t = typeof(Wmo);
                return new[]
                {
                    t.GetProperty("Groups"),
                    t.GetProperty("DoodadSets"),
                    t.GetProperty("DoodadReferences"),
                    t.GetProperty("DoodadDefinitions"),
                };
            }
        }

        #endregion
    }
}