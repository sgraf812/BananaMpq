using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BananaMpq.Visualization;

namespace BananaMpq.View.Models
{
    public class ChunkHierarchyModel
    {
        private readonly string _label;
        private readonly object _node;

        public string Description
        {
            get
            {
                return _label != null ? string.Format("{0}: {1}", _label, _node) : _node.ToString();
            }
        }

        public IEnumerable<ChunkHierarchyModel> Children { get; private set; }

        public ChunkHierarchyModel(string label, object node)
        {
            _label = label;
            _node = node;
            Children = Enumerable.Empty<ChunkHierarchyModel>();

            var exposer = node as IHasVisualizableProperties;
            if (exposer != null)
            {
                Children = Children.Concat(exposer.VisualizableProperties
                    .Select(p => new ChunkHierarchyModel(p.Name, p.GetValue(exposer, null))));
            }

            var list = node as IEnumerable;
            if (list != null)
            {
                var i = 0;

                Children = Children.Concat(list.Cast<object>()
                    .Select(v => new ChunkHierarchyModel(string.Format("[{0}]", i++), v)));
            }

            var collector = node as IChunkCollector;
            if (collector != null)
            {
                Children = Children.Concat(collector.Chunks
                    .Select(c => new ChunkHierarchyModel(null, c)));
            }
        }
    }
}