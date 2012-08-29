using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace BananaMpq.Geometry.Builders
{
    public class SquareMeshBuilder
    {
        private readonly IList<Vector3> _vertices;
        private readonly float[,] _heightMap;
        private readonly Vector3 _offset;
        private readonly int[,] _jumpIndices;
        private readonly Matrix? _rootTransform;
        private readonly float _tileSize;

        public SquareMeshBuilder(float[,] heightMap, Vector3 offset, float tileSize, Matrix? rootTransform = null)
        {
            _tileSize = tileSize;
            _rootTransform = rootTransform;
            _offset = offset;
            _heightMap = heightMap;
            var rows = heightMap.GetLength(0);
            var cols = heightMap.GetLength(1);
            _jumpIndices = ArrayUtil.MakeTwoDimensionalArray(-1, rows, cols);
            _vertices = new List<Vector3>(rows * cols);
        }

        private int MaxTileRows { get { return _heightMap.GetLength(0) - 1; } }

        private int MaxTileColumns { get { return _heightMap.GetLength(1) - 1; } }

        private int GetIndexAt(int row, int col)
        {
            var vertexIndex = _jumpIndices[row, col];

            if (vertexIndex == -1)
            {
                _jumpIndices[row, col] = vertexIndex = _vertices.Count;

                var v = _offset + new Vector3(row * _tileSize, col * _tileSize, _heightMap[row, col]);
                if (_rootTransform.HasValue)
                    v = (Vector3)Vector3.Transform(v, _rootTransform.Value);
                _vertices.Add(v);
            }
            return vertexIndex;
        }

        private IndexedTriangleWithNormal MakeTriangle(int a, int b, int c)
        {
            return IndexedTriangleWithNormal.CreateFromVertices(a, b, c, _vertices);
        }

        public SceneObject BuildSquareMesh(Func<int, int, bool> shouldNotRenderTile, MaterialFlags materialProperties, RectangleF bounds)
        {
            var trianglesPerRow = MaxTileRows;
            var trianglesPerCol = MaxTileColumns;
            var triangles = new List<IndexedTriangleWithNormal>(trianglesPerRow * trianglesPerCol * 2);

            for (var row = 0; row < trianglesPerRow; row++)
            {
                for (var col = 0; col < trianglesPerCol; col++)
                {
                    if (shouldNotRenderTile(col, row)) continue;

                    var topLeft = GetIndexAt(row, col);
                    var topRight = GetIndexAt(row, col + 1);
                    var bottomLeft = GetIndexAt(row + 1, col);
                    var bottomRight = GetIndexAt(row + 1, col + 1);

                    if (IsOutOfBounds(topLeft, bounds)
                        && IsOutOfBounds(topRight, bounds)
                        && IsOutOfBounds(bottomLeft, bounds)
                        && IsOutOfBounds(bottomRight, bounds))
                        continue;

                    triangles.Add(MakeTriangle(bottomLeft, topRight, topLeft));
                    triangles.Add(MakeTriangle(topRight, bottomLeft, bottomRight));
                }
            }

            if (triangles.Count == 0)
            {
                return null;
            }

            var vertices = _vertices.ToArray();
            return new SceneObject
            {
                Bounds = BoundingBox.FromPoints(vertices),
                MaterialProperties = materialProperties,
                Geometry = new TriangleMesh
                {
                    Vertices = vertices,
                    Triangles = triangles.ToArray()
                }
            };
        }

        private bool IsOutOfBounds(int index, RectangleF bounds)
        {
            return !bounds.Contains(_vertices[index]);
        }
    }
}