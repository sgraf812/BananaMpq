using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX.Direct3D9;

namespace BananaMpq.View.Infrastructure
{
    public class DirectX9RenderBatch : IRenderBatch
    {
        private readonly Device _device;
        private readonly PrimitiveType _primiteType;
        private IndexBuffer _indices;
        private VertexBuffer _vertices;
        private int _vertexCount;
        private int _indexCount;
        private int _stride;

        public DirectX9RenderBatch(Device device, PrimitiveType primiteType)
        {
            _primiteType = primiteType;
            _device = device;
        }

        #region Implementation of IRenderBatch<T>

        public void Record<TIndex, TVertex>(TIndex[] indices, TVertex[] vertices)
            where TIndex : struct
            where TVertex : struct
        {
            RecordIndices(indices);
            RecordVertices(vertices);
        }

        private void RecordVertices<TVertex>(TVertex[] vertices) where TVertex : struct
        {
            var size = InitializeVertexBuffer(vertices);

            using (var stream = _vertices.Lock(0, size, LockFlags.Discard))
                stream.WriteRange(vertices);
            _vertices.Unlock();
        }

        private int InitializeVertexBuffer<TVertex>(ICollection<TVertex> vertices) where TVertex : struct
        {
            var vertexType = typeof(TVertex);
            _stride = Marshal.SizeOf(vertexType);
            _vertexCount = vertices.Count;
            var size = _stride*_vertexCount;
            var vertexFormat = (VertexFormat)vertexType.GetField("Format").GetValue(null);
            _vertices = new VertexBuffer(_device, size, Usage.WriteOnly, vertexFormat, Pool.Default);
            return size;
        }

        private void RecordIndices<TIndex>(TIndex[] indices) where TIndex : struct
        {
            var size = InitializeIndexBuffer(indices);

            using (var stream = _indices.Lock(0, size, LockFlags.Discard))
                stream.WriteRange(indices);
            _indices.Unlock();
        }

        private int InitializeIndexBuffer<TIndex>(ICollection<TIndex> indices) where TIndex : struct
        {
            var indexType = typeof(TIndex);
            int size;
            if (indexType == typeof(int) || indexType == typeof(uint))
            {
                size = indices.Count*sizeof(int);
                _indices = new IndexBuffer(_device, size, Usage.WriteOnly, Pool.Default, false);
            }
            else if (indexType == typeof(short) || indexType == typeof(ushort))
            {
                size = indices.Count*sizeof(short);
                _indices = new IndexBuffer(_device, size, Usage.WriteOnly, Pool.Default, true);
            }
            else
            {
                throw new NotSupportedException(string.Format("ReferenceIndex type {0}", indexType));
            }
            _indexCount = indices.Count;
            return size;
        }

        public void Render()
        {
            _device.Indices = _indices;
            _device.SetStreamSource(0, _vertices, 0, _stride);
            _device.DrawIndexedPrimitive(_primiteType, 0, 0, _vertexCount, 0, PrimitiveCount);
        }

        private int PrimitiveCount
        {
            get
            {
                switch (_primiteType)
                {
                    case PrimitiveType.PointList:
                        return _indexCount;
                    case PrimitiveType.LineList:
                        return _indexCount/2;
                    case PrimitiveType.LineStrip:
                        return _indexCount - 1;
                    case PrimitiveType.TriangleList:
                        return _indexCount/3;
                    case PrimitiveType.TriangleStrip:
                        return (_indexCount-1)/2;
                    case PrimitiveType.TriangleFan:
                        return _indexCount - 2;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion
    }
}