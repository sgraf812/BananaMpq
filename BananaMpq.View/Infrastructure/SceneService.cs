using System;
using System.Linq;
using BananaMpq.Geometry;
using BananaMpq.Geometry.Builders;
using BananaMpq.Layer.WmoRelated;
using BananaMpq.View.Rendering;
using CrystalMpq.DataFormats;
using CrystalMpq.WoW;

namespace BananaMpq.View.Infrastructure
{
    public class SceneService
    {
        public static readonly SceneService Instance = new SceneService();
        private readonly AdtGeometryBuilder _builder;
        private readonly KeyedClientDatabase<int, LiquidTypeRecord> _liquidTypeDatabase;

        public Scene Scene { get; private set; }
        public IBVHTree BVHTree { get; private set; }
        public object BuildResult { get; private set; }
        public FilePool Files { get; private set; }
        public INavigationMeshRenderer CurrentNavigationMeshRenderer { get; set; }

        public bool HasLoadedTile { get { return Scene != null; } }
        public bool HasBuiltNavMesh { get { return BuildResult != null; } }

        public event EventHandler TileLoaded = delegate { };
        public event EventHandler NavMeshLoaded = delegate { }; 

        private SceneService()
        {
            var fileReader = new MpqFileReader();
            // var fileReader = new MpqFileReader(WoWInstallation.AssumeAt("Z:\\World of Warcraft"));
            Files = new FilePool(fileReader);
            _liquidTypeDatabase = InitializeDatabase(fileReader);
            _builder = new AdtGeometryBuilder(Files, MapLiquidType);
        }

        public void LoadTile(WowContinent continent, int x, int y)
        {
            Scene = _builder.BuildTile(MpqFilePaths.MapToInternalName(continent), x, y);
            BVHTree = new BVHTree(
                Scene.Terrain
                .Concat(Scene.Liquids)
                .Concat(Scene.Doodads)
                .Concat(Scene.Wmos));
            BuildResult = null;
            if (CurrentNavigationMeshRenderer != null)
                CurrentNavigationMeshRenderer.ClearCache();
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

        public void BuildNavMesh(BuildConfiguration config)
        {
            if (PluginLoader.PluginExists)
            {
                BuildResult = PluginLoader.NavMeshPlugin.BuildNavMesh(config);
                NavMeshLoaded(this, EventArgs.Empty);
            }
        }
    }
}