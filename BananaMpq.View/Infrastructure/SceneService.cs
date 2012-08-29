using System;
using BananaMpq.Geometry;
using BananaMpq.Geometry.Builders;
using BananaMpq.Layer.WmoRelated;
using CrystalMpq.DataFormats;

namespace BananaMpq.View.Infrastructure
{
    public class SceneService
    {
        public static readonly SceneService Instance = new SceneService();
        private readonly AdtGeometryBuilder _builder;
        private readonly KeyedClientDatabase<int, LiquidTypeRecord> _liquidTypeDatabase;

        public Scene Scene { get; private set; }
        public bool HasLoadedTile { get { return Scene != null; } }
        public FilePool Files { get; private set; }
        public event EventHandler TileLoaded = delegate { };

        private SceneService()
        {
            var fileReader = new MpqFileReader();
            Files = new FilePool(fileReader);
            _liquidTypeDatabase = InitializeDatabase(fileReader);
            _builder = new AdtGeometryBuilder(Files, MapLiquidType);
        }

        public void LoadTile(WowContinent continent, int x, int y)
        {
            Scene = _builder.BuildTile(continent, x, y);
            TileLoaded(this, EventArgs.Empty);
        }

        private KeyedClientDatabase<int, LiquidTypeRecord> InitializeDatabase(MpqFileReader fileReader)
        {
            using (var stream = fileReader.Open("DBFilesClient\\LiquidType.dbc"))
            {
                return new KeyedClientDatabase<int, LiquidTypeRecord>(stream);
            }
        }

        private LiquidTypeClass MapLiquidType(int liquidType)
        {
            var record = _liquidTypeDatabase[liquidType];
            return (LiquidTypeClass)record.TypeClass;
        }
    }
}