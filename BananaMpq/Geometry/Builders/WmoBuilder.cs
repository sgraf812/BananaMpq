using System;
using System.Collections.Generic;
using System.Linq;
using BananaMpq.Layer.AdtRelated;
using BananaMpq.Layer.Chunks;
using BananaMpq.Layer.WmoRelated;
using SharpDX;

namespace BananaMpq.Geometry.Builders
{
    public class WmoBuilder : ModelBuilder
    {
        private readonly ISet<int> _builtIds = new HashSet<int>();
        private readonly DoodadBuilder _doodadBuilder;
        private readonly Func<int, MaterialFlags> _getLiquidMaterial;

        public WmoBuilder(FilePool files, DoodadBuilder doodadBuilder, Func<int, MaterialFlags> getLiquidMaterial) : base(files)
        {
            _doodadBuilder = doodadBuilder;
            _getLiquidMaterial = getLiquidMaterial;
        }

        public WmoBuildResults BuildWmos(IEnumerable<IModelDefinition> definitions, IList<StringReference> references, RectangleF bounds)
        {
            var groups = Enumerable.Empty<SceneObject>();
            var liquids = Enumerable.Empty<SceneObject>();
            var doodads = Enumerable.Empty<SceneObject>();
            foreach (var definition in definitions)
            {
                if (definition.Id != null && !_builtIds.Add(definition.Id.Value)) continue;
                var wmo = Files.GetWmo(definition.GetModelReference(references));
                var transform = definition.GetTranform();

                var doodadDefs = definition.FilterDoodadSetDefinitions(wmo.DoodadSets, wmo.DoodadDefinitions);
                doodads = doodads.Concat(from d in _doodadBuilder.BuildDoodads(doodadDefs, wmo.DoodadReferences, bounds, transform)
                                         select d);

                groups = groups.Concat(from g in wmo.Groups
                                       let collisionTriangles =
                                           g.Triangles.Where((t, i) => (g.TriangleFlags[i] & MopyChunk.NoCollision) == 0)
                                       where collisionTriangles.Any()
                                       select BuildModelFromTransform(g.Vertices, collisionTriangles, transform) into sceneObject
                                       where bounds.Intersects(sceneObject.Bounds)
                                       select sceneObject);

                liquids = liquids.Concat(from g in wmo.Groups
                                         where g.Liquid != null
                                         let l = g.Liquid
                                         let meshBuilder = new SquareMeshBuilder(l.HeightMap, new Vector3(l.Position.X, l.Position.Y, 0.0f),
                                             MapChunk.TileSize, transform)
                                         let materialProperties = _getLiquidMaterial(g.DetermineLiquidType())
                                         select meshBuilder.BuildSquareMesh((c, r) => !l.ExistsTable[r, c], materialProperties, bounds)
                                             into sceneObject
                                             where sceneObject != null
                                             select sceneObject);
            }

            return new WmoBuildResults
            {
                GroupObjects = groups,
                Doodads = doodads,
                Liquids = liquids
            };
        }
    }
}