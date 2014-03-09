using System;
using System.Collections.Generic;
using System.Linq;
using BananaMpq.Layer;
using BananaMpq.Layer.AdtRelated;
using BananaMpq.Layer.Chunks;
using BananaMpq.Layer.WmoRelated;
using SharpDX;

namespace BananaMpq.Geometry.Builders
{
    public class AdtGeometryBuilder
    {
        private readonly FilePool _files;
        private readonly Func<int, LiquidTypeClass> _liquidTypeMapper;

        public AdtGeometryBuilder(FilePool files, Func<int, LiquidTypeClass> liquidTypeMapper)
        {
            _files = files;
            _liquidTypeMapper = liquidTypeMapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="continent"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="padding">You can pad from 0 up to an mcnk sized strip around the adt.</param>
        /// <returns></returns>
        public Scene BuildTile(string continent, int x, int y, float padding = MapChunk.TileSize)
        {
            var chunkBuilder = new ChunkBuilder(GetLiquidMaterialProperties);
            var doodadBuilder = new DoodadBuilder(_files);
            var wmoBuilder = new WmoBuilder(_files, doodadBuilder, GetLiquidMaterialProperties);
            var terrain = new List<SceneObject>();
            var liquids = new List<SceneObject>();
            var doodads = new List<SceneObject>();
            var wmos = new List<SceneObject>();

            var centerAdt = _files.GetAdt(continent, x, y);
            var bounds = GetSceneBounds(centerAdt, padding);

            foreach (var p in AdtRegion(continent, x, y))
            {
                var curX = (int)p.X;
                var curY = (int)p.Y;
                var adt = _files.GetAdt(continent, curX, curY);
                terrain.AddRange(chunkBuilder.BuildTerrain(adt, bounds));
                liquids.AddRange(chunkBuilder.BuildLiquid(adt, bounds));
                doodads.AddRange(doodadBuilder.BuildDoodads(DefinedDoodads(adt), adt.DoodadReferences, bounds));
                var wmoResults = wmoBuilder.BuildWmos(DefinedWmos(adt), adt.WmoReferences, bounds);
                wmos.AddRange(wmoResults.GroupObjects);

                doodads.AddRange(wmoResults.Doodads);
                liquids.AddRange(wmoResults.Liquids);
            }

            return MergeIntoScene(centerAdt, terrain, liquids, doodads, wmos);
        }

        private static IEnumerable<IModelDefinition> DefinedDoodads(Adt adt)
        {
            var allRefs = new HashSet<int>(adt.MapChunks.Where(m => m.DoodadReferences != null).SelectMany(m => m.DoodadReferences));
            return adt.DoodadDefinitions.Where((d, i) => allRefs.Contains(i));
        }

        private static IEnumerable<IModelDefinition> DefinedWmos(Adt adt)
        {
            var allRefs = new HashSet<int>(adt.MapChunks.Where(m => m.WmoReferences != null).SelectMany(m => m.WmoReferences));
            return adt.WmoDefinitions.Where((d, i) => allRefs.Contains(i));
        }

        private static RectangleF GetSceneBounds(Adt adt, float padding)
        {
            return new RectangleF
            {
                Top = adt.Bounds.Maximum.Y + padding,
                Right = adt.Bounds.Maximum.X + padding,
                Bottom = adt.Bounds.Minimum.Y - padding,
                Left = adt.Bounds.Minimum.X - padding,
            };
        }

        private IEnumerable<Vector2> AdtRegion(string continent, int x, int y)
        {
            for (int currentX = x - 1; currentX <= x + 1; currentX++)
            {
                for (int currentY = y - 1; currentY <= y + 1; currentY++)
                {
                    if (AdtExists(continent, currentX, currentY))
                    {
                        yield return new Vector2(currentX, currentY);
                    }
                }
            }
        }

        private bool AdtExists(string continent, int x, int y)
        {
            return _files.GetWdt(continent).AdtExistsForTile(x, y);
        }

        private static Scene MergeIntoScene(Adt adt, IEnumerable<SceneObject> terrain, IEnumerable<SceneObject> liquids, 
            IEnumerable<SceneObject> doodads, IEnumerable<SceneObject> wmos)
        {
            return new Scene
            {
                Adt = adt,
                Terrain = terrain,
                Liquids = liquids,
                Doodads = doodads,
                Wmos = wmos
            };
        }

        private MaterialFlags GetLiquidMaterialProperties(int liquidType)
        {
            var typeClass = _liquidTypeMapper(liquidType);
            var materialProperties = MaterialFlags.Liquid;
            if (typeClass == LiquidTypeClass.Slime || typeClass == LiquidTypeClass.Magma)
                materialProperties |= MaterialFlags.DamageOverTime;
            return materialProperties;
        }
    }
}