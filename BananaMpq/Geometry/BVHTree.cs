using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpDX;

namespace BananaMpq.Geometry
{
    public class BVHTree : IBVHTree
    {
        private const int MinimumLeafCountForParallelism = 512;
        private static readonly BoundingBox Nothing = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
        private readonly IList<BVHNode> _nodes;
        private readonly ParallelOptions _parallelOptions;

        public BoundingBox Bounds
        {
            get { return _nodes[0].Bounds; }
        }

        public BVHTree(IEnumerable<SceneObject> scene, ParallelOptions parallelOptions = null)
            : this(ReduceToBVHNodes(scene), parallelOptions)
        {
        }

        private static BVHNode[] ReduceToBVHNodes(IEnumerable<SceneObject> scene)
        {
            return scene
                .SelectMany(s => s.Geometry.Triangles
                                     .Select((t, i) =>
                                     {
                                         var bounds = t.BoundsIn(s.Geometry.Vertices);
                                         return BVHNode.CreateLeaf(s, i, ref bounds);
                                     }))
                .ToArray();
        }

        private BVHTree(BVHNode[] nodes, ParallelOptions parallelOptions = null)
        {
            _parallelOptions = parallelOptions ?? new ParallelOptions
            {
                MaxDegreeOfParallelism = int.MaxValue,
                TaskScheduler = TaskScheduler.Default
            };

            _nodes = new BVHNode[ChildCountFor(nodes.Length)];

            Subdivide(nodes, nodes.Length, 0);
        }

        private void Subdivide(BVHNode[] leafs, int leafCount, int addNext)
        {
            if (leafCount == 1)
            {
                _nodes[addNext] = leafs.First();
                return;
            }
            
            var bounds = leafs.Aggregate(Nothing, (cur, n) => BoundingBox.Merge(cur, n.Bounds));
            var comparer = new AxisBVHNodeComparer(bounds.Maximum - bounds.Minimum);
            Array.Sort(leafs, comparer);

            _nodes[addNext++] = BVHNode.CreateBranch(ChildCountFor(leafCount), ref bounds);

            var leftCount = leafCount / 2;
            var rightCount = leafCount - leftCount;
            if (leafCount >= MinimumLeafCountForParallelism)
            {
                Parallel.Invoke(_parallelOptions,
                    () => Subdivide(leafs.Take(leftCount).ToArray(), leftCount, addNext),
                    () => Subdivide(leafs.Skip(leftCount).ToArray(), rightCount, addNext + ChildCountFor(leftCount)));
            }
            else
            {
                Subdivide(leafs.Take(leftCount).ToArray(), leftCount, addNext);
                Subdivide(leafs.Skip(leftCount).ToArray(), rightCount, addNext + ChildCountFor(leftCount));
            }
        }

        private static int ChildCountFor(int leafCount)
        {
            return 2 * leafCount - 1;
        }

        public IEnumerable<IBVHNode> GetIntersections(ref BoundingBox bounds)
        {
            var intersections = new List<BVHNode>();
            for (int i = 0; i < _nodes.Count; )
            {
                var node = _nodes[i];
                var isIntersecting = node.Bounds.Intersects(ref bounds);

                if (node.IsLeaf && isIntersecting)
                {
                    intersections.Add(node);
                }

                if (node.IsLeaf || isIntersecting)
                {
                    ++i;
                }
                else
                {
                    i += node.ChildCount;
                }
            }
            return intersections;
        }

        public IBVHNode GetIntersectedNode(ref Ray ray)
        {
            var dist = float.PositiveInfinity;
            IBVHNode bestNode = null;
            for (int i = 0; i < _nodes.Count; )
            {
                var node = _nodes[i];
                var isIntersecting = node.Bounds.Intersects(ref ray);

                if (node.IsLeaf && isIntersecting)
                {
                    var t = node.Triangle;
                    float tmp;
                    if (ray.Intersects(ref t.A, ref t.B, ref t.C, out tmp) && tmp < dist)
                    {
                        dist = tmp;
                        bestNode = node;
                    }
                }

                if (node.IsLeaf || isIntersecting)
                {
                    ++i;
                }
                else
                {
                    i += node.ChildCount;
                }
            }

            return bestNode;
        }

        private class AxisBVHNodeComparer : IComparer<BVHNode>
        {
            private readonly int _indexToCompare;

            public AxisBVHNodeComparer(Vector3 extent)
            {
                _indexToCompare = extent.X > extent.Y ? extent.X > extent.Z ? -1 : 1 : extent.Y > extent.Z ? 0 : 1;
            }

            #region IComparer members

            public int Compare(BVHNode x, BVHNode y)
            {
                if (_indexToCompare == -1)
                {
                    return x.Bounds.Minimum.X.CompareTo(y.Bounds.Minimum.X);
                }
                if (_indexToCompare == 0)
                {
                    return x.Bounds.Minimum.Y.CompareTo(y.Bounds.Minimum.Y);
                }
                return x.Bounds.Minimum.Z.CompareTo(y.Bounds.Minimum.Z);
            }

            #endregion

        }
    }
}