using SharpDX;
using SharpDX.Direct3D9;

namespace BananaMpq.View.Rendering
{
    public struct VertexPositionNormal
    {
        public const VertexFormat Format = VertexFormat.Position | VertexFormat.Normal;
        public Vector3 Position;
        public Vector3 Normal;
    }
}