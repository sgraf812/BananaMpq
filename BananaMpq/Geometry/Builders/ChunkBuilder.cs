using System;
using System.Collections.Generic;
using BananaMpq.Layer;
using BananaMpq.Layer.AdtRelated;
using SharpDX;
using System.Linq;

namespace BananaMpq.Geometry.Builders
{
    public class ChunkBuilder
    {
        private readonly Func<int, MaterialFlags> _getLiquidMaterial;

        public ChunkBuilder(Func<int, MaterialFlags> getLiquidMaterial)
        {
            _getLiquidMaterial = getLiquidMaterial;
        }

        public IEnumerable<SceneObject> BuildLiquid(Adt adt, RectangleF bounds)
        {
            var chunksWithLiquids = from chunk in adt.MapChunks
                                    where chunk.Liquid != null && chunk.Liquid.HeightMap != null
                                    select chunk;

            foreach (var chunk in chunksWithLiquids)
            {
                var offset = chunk.Bounds.Maximum;
                offset.Z = 0;
                var liquid = chunk.Liquid;
                var meshBuilder = new SquareMeshBuilder(liquid.HeightMap, offset, -MapChunk.TileSize);
                var materialProperties = _getLiquidMaterial(liquid.Type);
                var sceneObject = meshBuilder.BuildSquareMesh((c, r) => !liquid.ExistsTable[r, c], materialProperties, bounds);
                if (sceneObject != null) yield return sceneObject;
            }
        }

        public IEnumerable<SceneObject> BuildTerrain(Adt adt, RectangleF bounds)
        {
            foreach (var chunk in adt.MapChunks)
            {
                var offset = chunk.Bounds.Maximum;
                offset.Z = chunk.Bounds.Minimum.Z;
                var meshBuilder = new SquareMeshBuilder(chunk.HeightMap, offset, -MapChunk.TileSize);
                var sceneObject = meshBuilder.BuildSquareMesh(chunk.HasHole, MaterialFlags.None, bounds);
                if (sceneObject != null) yield return sceneObject;
            }
        }
    }
}