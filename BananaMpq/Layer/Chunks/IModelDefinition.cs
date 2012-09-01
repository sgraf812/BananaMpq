using System.Collections.Generic;
using BananaMpq.Layer.WmoRelated;
using SharpDX;

namespace BananaMpq.Layer.Chunks
{
    public interface IModelDefinition
    {
        int? Id { get; }
        Vector3 Position { get; }
        Matrix GetTranform();
        string GetModelReference(IList<StringReference> references);
        IEnumerable<IModelDefinition> FilterDoodadSetDefinitions(IList<DoodadSet> sets, IEnumerable<IModelDefinition> doodadDefs);
    }
}