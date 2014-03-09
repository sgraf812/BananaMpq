using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BananaMpq.Geometry;
using BananaMpq.View.Infrastructure;
using BananaMpq.View.Rendering;
using BananaMpq.View.Views;
using SharpDX;
using SharpDX.Direct3D9;

namespace BananaMpq.View.Presenters
{
    class ScenePresenter
    {
        private const int InitialHeight = 600;
        private const int InitialWidth = 800;
        private readonly ISceneView _view;
        private readonly FirstPersonCamera _camera;
        private readonly SceneService _service = SceneService.Instance;
        private readonly List<INavigationMeshRenderer> _navMeshRenderers = new List<INavigationMeshRenderer>();
        private Device _device;
        private Surface _renderTarget;
        private TimeSpan _lastRenderingTime;
        private ISceneRenderer _sceneRenderer;
        private Surface _zBuffer;
        private Format _depthBufferFormat;
        private bool _geometryVisible = true;
        private AgentRenderer _start;
        private AgentRenderer _destination;
        private Vector3[] _path;

        public event EventHandler<NavigationMeshRenderersEventArgs> NewNavigationMeshRenderers = delegate {}; 

        public ScenePresenter(ISceneView view, CameraChassis chassis)
        {
            _view = view;
            chassis.Inertia = 3.0f;
            _camera = new FirstPersonCamera(chassis)
            {
                Forward = Vector3.UnitX,
                Right = -Vector3.UnitY,
                Up = Vector3.UnitZ,
                Velocity = 80.0f,
            };
            _service.TileLoaded += (s, e) => ResetPerTileState();
        }

        public void ToggleGeometryVisibility()
        {
            if (_service.HasBuiltNavMesh && _service.HasLoadedTile)
            {
                _geometryVisible = !_geometryVisible;
            }
        }

        private void ResetPerTileState()
        {
            _camera.Position = _service.Scene.Terrain.First().Bounds.Maximum;
            _camera.Chassis.Yaw = 3.0f * (float)Math.PI / 4.0f;
            _start.IsPositioned = false;
            _destination.IsPositioned = false;
        }

        public void HandleDeviceReset()
        {
            InizializeDevice();
            Resize(InitialWidth, InitialHeight);
            _sceneRenderer = new SceneGeometryRenderer(_device);
            _start = new AgentRenderer(_device);
            _destination = new AgentRenderer(_device);
            _start.AgentRadius = 0.6f;
            _destination.AgentRadius = 0.6f;
            PopulateNavMeshRenderers();
        }

        private void PopulateNavMeshRenderers()
        {
            _navMeshRenderers.Clear();
            if (PluginLoader.PluginExists)
            {
                _navMeshRenderers.AddRange(PluginLoader.NavMeshPlugin.CreateRenderers(_device));
            }
            NewNavigationMeshRenderers(this, new NavigationMeshRenderersEventArgs(_navMeshRenderers));
            GC.Collect();
        }

        public void ParseCommandLine(string[] args)
        {
            WowContinent continent;
            int x, y;
            if (args.Length >= 4
                && Enum.TryParse(args[1], out continent)
                && Int32.TryParse(args[2], out x)
                && Int32.TryParse(args[3], out y))
            {
                _service.LoadTile(continent, x, y);
            }
            else
            {
                _view.OpenTileSelectionDialog();
            }
        }

        private void InizializeDevice()
        {
            var d3d = new Direct3D();
            _depthBufferFormat = GetSupportedDepthBufferFormat(d3d);
            _device = new Device(d3d, 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing,
                new PresentParameters
                {
                    SwapEffect = SwapEffect.Discard,
                    EnableAutoDepthStencil = true,
                    AutoDepthStencilFormat = _depthBufferFormat,
                    DeviceWindowHandle = _view.WindowHandle,
                    Windowed = true,
                });
            _device.SetRenderState(RenderState.ZEnable, true);
            _device.SetRenderState(RenderState.ZWriteEnable, true);
            _device.SetRenderState(RenderState.ZFunc, Compare.LessEqual);
            _device.SetRenderState(RenderState.AlphaBlendEnable, true);
            _device.SetRenderState(RenderState.AlphaTestEnable, true);
            _device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            _device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            _device.SetRenderState(RenderState.AlphaFunc, Compare.GreaterEqual);
            _device.SetRenderState(RenderState.CullMode, Cull.Clockwise);

            var strong = new Light
            {
                Diffuse = new Color4(0.4f, 0.4f, 0.4f, 0.0f),
                Type = LightType.Directional,
                Direction = new Vector3(1, 1, -1)
            };
            var weak = new Light
            {
                Diffuse = new Color4(0.1f, 0.1f, 0.1f, 0.0f),
                Type = LightType.Directional,
                Direction = new Vector3(-1, -1, -1)
            };
            _device.SetLight(0, ref strong);
            _device.SetLight(1, ref weak);
            _device.EnableLight(0, true);
            _device.EnableLight(1, true);
            _device.SetRenderState(RenderState.Ambient, new Color4(0.2f, 0.2f, 0.2f, 0f).ToRgba());
            _device.SetRenderState(RenderState.Lighting, true);
            _device.SetRenderState(RenderState.DiffuseMaterialSource, ColorSource.Material);
            _device.SetRenderState(RenderState.AmbientMaterialSource, ColorSource.Material);
            _device.SetRenderState(RenderState.SpecularMaterialSource, ColorSource.Material);
        }

        private static Format GetSupportedDepthBufferFormat(Direct3D d3d)
        {
            var adapterFormat = d3d.Adapters[0].CurrentDisplayMode.Format;
            var formats = new[] { Format.D32, Format.D32SingleLockable, Format.D24SingleS8, Format.D24S8, Format.D24X8, Format.D16 };
            return formats.First(f => 
                d3d.CheckDeviceFormat(0, DeviceType.Hardware, adapterFormat, Usage.DepthStencil, ResourceType.Surface, f));
        }

        public void Resize(int width, int height)
        {
            if (_device == null) return;
            if (_renderTarget != null) _renderTarget.Dispose();
            _renderTarget = Surface.CreateRenderTarget(_device, width, height, Format.A8R8G8B8, MultisampleType.None, 0, false);
            if (_zBuffer != null) _zBuffer.Dispose();
            _zBuffer = Surface.CreateDepthStencil(_device, width, height, _depthBufferFormat, MultisampleType.None, 0, false);

            _device.SetRenderTarget(0, _renderTarget);
            _device.DepthStencilSurface = _zBuffer;
            _device.Viewport = new Viewport(0, 0, width, height);

            var proj = Matrix.PerspectiveFovRH((float)Math.PI / 4.0f, width / (float)height, 1f, 1000.0f);
            _device.SetTransform(TransformState.Projection, ref proj);
        }

        public void Render(TimeSpan renderingTime)
        {
            if (_device == null) return;
            var dt = (float)(renderingTime - _lastRenderingTime).TotalSeconds;
            if (dt <= 0) return;
            _lastRenderingTime = renderingTime;
            using (_view.StartRenderPass(_renderTarget.NativePointer))
            {
                _device.BeginScene();

                _camera.Sample(dt);
                var view = _camera.WorldView;
                _device.SetTransform(TransformState.View, ref view);

                _device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new ColorBGRA(0.2f, 0.2f, 0.2f, 1.0f), 1.0f, 0);
                if (_service.HasLoadedTile && (!_service.HasBuiltNavMesh || _geometryVisible))
                    _sceneRenderer.Render(_service.Scene);

                if (_service.HasBuiltNavMesh)
                    RenderPath();

                _device.SetRenderState(RenderState.CullMode, Cull.None);
                if (_service.HasBuiltNavMesh && _service.CurrentNavigationMeshRenderer != null) 
                    _service.CurrentNavigationMeshRenderer.Render(_service.BuildResult); 
                _device.SetRenderState(RenderState.CullMode, Cull.Clockwise);

                _device.EndScene();
            }
        }

        private void RenderPath()
        {
            _start.Render(Color.Green);
            _destination.Render(Color.Red);
            if (_path != null)
                _device.DrawUserPrimitives(PrimitiveType.LineStrip, _path.Length - 1, _path);
        }

        public void UpdateAgent(Point point, bool setStartPosition)
        {
            if (!_service.HasBuiltNavMesh) return;

            var ray = GetMouseRay(point);
            var node = _service.BVHTree.GetIntersectedNode(ref ray);
            var position = node != null ? node.GetIntersectingPoint(ref ray) : null;
            position = position.HasValue ? PluginLoader.NavMeshPlugin.FindNearestPositionOnMesh(position.Value) : null;

            if (setStartPosition)
            {
                _start.Position = position ?? _start.Position;
            }
            else
            {
                _destination.Position = position ?? _destination.Position;
            }

            if (position.HasValue && _start.IsPositioned && _destination.IsPositioned)
            {
                _path = PluginLoader.NavMeshPlugin.FindPath(_start.Position, _destination.Position);
            }
        }

        private Ray GetMouseRay(Point p)
        {
            var worldViewProj = Matrix.Multiply(_camera.WorldView, _device.GetTransform(TransformState.Projection));
            var viewport = _device.Viewport;
            var rayStart = Unproject(new Vector3((float)p.X, (float)p.Y, 0.0f), ref worldViewProj, ref viewport);
            var rayEnd = Unproject(new Vector3((float)p.X, (float)p.Y, 1.0f), ref worldViewProj, ref viewport);
            return new Ray(rayStart, rayEnd - rayStart);
        }

        private static Vector3 Unproject(Vector3 v, ref Matrix wvp, ref Viewport viewport)
        {
            Vector3 ret;
            Vector3.Unproject(ref v, viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth, viewport.MaxDepth, ref wvp, out ret);
            return ret;
        }

        public string GetModelFileUnderCursor(Point p)
        {
            if (_service.BVHTree == null) return null;
            var ray = GetMouseRay(p);
            var node = _service.BVHTree.GetIntersectedNode(ref ray);
            var n = node as BVHNode; // need scene object
            return n != null ? n.SceneObject.Description : null;
        }
    }
}