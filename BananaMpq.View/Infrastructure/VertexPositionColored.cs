using SharpDX;
using SharpDX.Direct3D9;

namespace BananaMpq.View.Infrastructure
{
    public struct VertexPositionColored
    {
        public const VertexFormat Format = VertexFormat.Position | VertexFormat.Diffuse;
        public Vector3 Position;
        public int Color;
    }
}