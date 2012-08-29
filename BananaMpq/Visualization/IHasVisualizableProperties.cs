using System.Collections.Generic;
using System.Reflection;

namespace BananaMpq.Visualization
{
    public interface IHasVisualizableProperties
    {
        IEnumerable<PropertyInfo> VisualizableProperties { get; }
    }
}