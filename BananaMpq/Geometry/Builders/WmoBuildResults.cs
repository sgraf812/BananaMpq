using System.Collections.Generic;

namespace BananaMpq.Geometry.Builders
{
    public class WmoBuildResults
    {
        public IEnumerable<SceneObject> GroupObjects { get; set; }
        public IEnumerable<SceneObject> Liquids { get; set; }
        public IEnumerable<SceneObject> Doodads { get; set; }
    }
}