using System;
using System.Collections.Generic;
using BananaMpq.Layer;
using BananaMpq.Layer.AdtRelated;
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
        public Scene BuildTile(WowContinent continent, int x, int y, float padding = MapChunk.TileSize)
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

                doodads.AddRange(doodadBuilder.BuildDoodads(adt.DoodadDefinitions, adt.DoodadReferences, bounds));
                var wmoResults = wmoBuilder.BuildWmos(adt.WmoDefinitions, adt.WmoReferences, bounds);
                wmos.AddRange(wmoResults.GroupObjects);

                doodads.AddRange(wmoResults.Doodads);
                liquids.AddRange(wmoResults.Liquids);
            }

            return MergeIntoScene(centerAdt, terrain, liquids, doodads, wmos);
        }

        private static RectangleF GetSceneBounds(Adt adt, float padding)
        {
            var bounds = adt.Bounds;
            bounds.Top += padding;
            bounds.Right += padding;
            bounds.Bottom -= padding;
            bounds.Left -= padding;
            return bounds;
        }

        private IEnumerable<Vector2> AdtRegion(WowContinent continent, int x, int y)
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

        //private static RectangleF GetSceneBounds(float padding, Adt adt)
        //{
        //    var bounds = adt.Bounds;

        //    var stripOff = Adt.AdtWidth - padding;

        //    if (dx < 0)
        //        bounds.Top -= stripOff;
        //    else if (dx > 0)
        //        bounds.Bottom += stripOff;

        //    if (dy < 0)
        //        bounds.Right -= stripOff;
        //    else if (dy > 0)
        //        bounds.Left += stripOff;

        //    return bounds;
        //}

        private bool AdtExists(WowContinent continent, int x, int y)
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