using System;
using System.Collections.Generic;
using System.Linq;
using BananaMpq.Layer;
using BananaMpq.Layer.AdtRelated;
using SharpDX;

namespace BananaMpq.Geometry.Builders
{
    public class ChunkBuilder
    {
        private const string DescriptionTemplate = "{0} ({1}, {2})";
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
                var description = string.Format(DescriptionTemplate, "Liquid", chunk.X, chunk.Y);
                var sceneObject = meshBuilder.BuildSquareMesh(liquid.HasHole, materialProperties, bounds, description);
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
                var description = string.Format(DescriptionTemplate, "Terrain", chunk.X, chunk.Y);
                var sceneObject = meshBuilder.BuildSquareMesh(chunk.HasHole, MaterialFlags.None, bounds, description);
                if (sceneObject != null) yield return sceneObject;
            }
        }
    }
}