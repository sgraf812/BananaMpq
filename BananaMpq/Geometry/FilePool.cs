using System;
using System.Collections.Generic;
using System.Linq;
using BananaMpq.Layer;
using BananaMpq.Layer.WmoRelated;

namespace BananaMpq.Geometry
{
    public class FilePool
    {
        private readonly IFileReader _reader;
        private readonly IDictionary<string, M2Bounds> _doodadCache = new Dictionary<string, M2Bounds>();
        private readonly IDictionary<string, Wmo> _wmoCache = new Dictionary<string, Wmo>();
        private readonly IDictionary<Tuple<string, int>, WmoGroup> _groupCache = new Dictionary<Tuple<string, int>, WmoGroup>();
        private readonly IDictionary<Tuple<string, int, int>, Adt> _adtCache = new Dictionary<Tuple<string, int, int>, Adt>();
        private readonly IDictionary<string, Wdt> _wdtCache = new Dictionary<string, Wdt>();

        public FilePool(IFileReader reader)
        {
            _reader = reader;
        }

        public Wdt GetWdt(string continent)
        {
            return TryGetOrCreate(_wdtCache, continent, c => new Wdt(_reader.Read(MpqFilePaths.GetWdtFileName(c))));
        }

        public Adt GetAdt(string continent, int x, int y)
        {
            return TryGetOrCreate(_adtCache, Tuple.Create(continent, x, y), t =>
            {
                var files = MpqFilePaths.GetRelevantAdtFileNames(continent, x, y);
                var adt = new Adt(_reader.Read(files.First()));
                foreach (var secondaryFile in files.Skip(1))
                {
                    adt.ParseSecondaryData(_reader.Read(secondaryFile));
                }
                return adt;
            });
        }

        public M2Bounds GetDoodad(string name)
        {
            return TryGetOrCreate(_doodadCache, name, n => new M2Bounds(_reader.Read(n)));
        }

        public Wmo GetWmo(string name)
        {
            return TryGetOrCreate(_wmoCache, name, n => new Wmo(_reader.Read(n), i => GetWmoGroup(n, i)));
        }

        public WmoGroup GetWmoGroup(string rootName, int index)
        {
            return TryGetOrCreate(_groupCache, Tuple.Create(rootName, index), t =>
            {
                var groupFile = rootName.Insert(t.Item1.Length - 4, string.Format("_{0}", t.Item2.ToString("D3")));
                return new WmoGroup(_reader.Read(groupFile));
            });
        }

        private static V TryGetOrCreate<K, V>(IDictionary<K, V> dictionary, K key, Func<K, V> factory)
        {
            lock (dictionary)
            {
                V value;
                if (!dictionary.TryGetValue(key, out value))
                {
                    value = factory(key);
                    dictionary[key] = value;
                }
                return value;
            }
        }

        public void Clear()
        {
            lock (_doodadCache) _doodadCache.Clear();
            lock (_wmoCache) _wmoCache.Clear();
            lock (_groupCache) _groupCache.Clear();
            lock (_wdtCache) _wdtCache.Clear();
            lock (_adtCache) _adtCache.Clear();
        }
    }
}