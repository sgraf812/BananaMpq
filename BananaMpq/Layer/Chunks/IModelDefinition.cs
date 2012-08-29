using System.Collections.Generic;
using SharpDX;

namespace BananaMpq.Layer.Chunks
{
    public interface IModelDefinition
    {
        int? Id { get; }
        Vector3 Position { get; }
        Matrix GetTranform();
        string GetModelReference(IList<StringReference> references);
    }
}