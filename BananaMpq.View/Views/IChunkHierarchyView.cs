using System.Collections.Generic;
using BananaMpq.View.Models;

namespace BananaMpq.View.Views
{
    public interface IChunkHierarchyView
    {
        IEnumerable<ChunkHierarchyModel> Hierarchy { set; }
    }
}