using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D9;

namespace BananaMpq.View.Rendering
{
    class AgentRenderer
    {
        private static readonly Material _material = new Material
        {
            Diffuse = Color.DarkGray,
            Ambient = Color.DarkGray,
            Specular = Color.DarkGray,
            Power = 1.0f
        };
        private readonly Device _device;

        private readonly short[] _indices =
        {
            0, 1, 2, 10, 11, 12, 4, 5, 6, 14, 15, 8,
            0, 7, 6, 14, 13, 12, 4, 3, 2, 10, 9, 8
        };
        private Vector3 _position;
        private float _agentRadius;
        private Vector3[] _vertices;

        public AgentRenderer(Device device)
        {
            _device = device;
        }

        public float AgentRadius
        {
            get { return _agentRadius; }
            set
            {
                _agentRadius = value;
                if (IsPositioned) Update();
            }
        }

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                Update();
            }
        }

        public bool IsPositioned { get { return _vertices != null; } set { if (!value) _vertices = null; } }

        private void Update()
        {
            var pos = Position + new Vector3(0.0f, 0.0f, 0.1f);
            _vertices =
                CircleVertices(pos, AgentRadius)
                    .Concat(CircleVertices(pos + new Vector3(0.0f, 0.0f, 2.0f), AgentRadius))
                    .ToArray();
        }

        private static IEnumerable<Vector3> CircleVertices(Vector3 center, float radius)
        {
            for (int i = 0; i < 8; i++)
            {
                var angle = Math.PI/4.0*i;
                yield return center + new Vector3((float)(Math.Sin(angle)*radius), (float)(Math.Cos(angle)*radius), 0.0f);
            }
        }

        public void Render(Color4 color)
        {
            if (!IsPositioned) return;
            _device.Material = _material;
            _device.DrawIndexedUserPrimitives(PrimitiveType.LineStrip, 0, _vertices.Length, _indices.Length - 1, _indices,
                Format.Index16,
                _vertices);
        }
    }
}