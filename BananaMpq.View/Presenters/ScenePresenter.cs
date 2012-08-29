using System;
using System.Linq;
using BananaMpq.View.Infrastructure;
using BananaMpq.View.Views;
using SharpDX;
using SharpDX.Direct3D9;

namespace BananaMpq.View.Presenters
{
    public class ScenePresenter
    {
        private const int InitialHeight = 600;
        private const int InitialWidth = 800;
        private readonly ISceneView _view;
        private readonly FirstPersonCamera _camera;
        private readonly SceneService _service = SceneService.Instance;
        private Device _device;
        private Surface _renderTarget;
        private TimeSpan _lastRenderingTime;
        private ISceneRenderer _sceneRenderer;
        private Surface _zBuffer;
        private Format _depthBufferFormat;

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
            _service.TileLoaded += (s, e) => SetInitialCameraView();
        }

        private void SetInitialCameraView()
        {
            _camera.Position = _service.Scene.Terrain.First().Bounds.Maximum;
            _camera.Chassis.Yaw = 3.0f * (float)Math.PI / 4.0f;
        }

        public void HandleDeviceReset()
        {
            InizializeDevice();
            Resize(InitialWidth, InitialHeight);
            _sceneRenderer = new SceneGeometryRenderer(_device);
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
                SceneService.Instance.LoadTile(continent, x, y);
            }
            else
            {
                _view.OpenTileSelectionDialog();
                //SceneService.Instance.LoadTile(WowContinent.EasternKingdoms, 30, 48);
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
            _device.SetRenderState(RenderState.Lighting, false);
            _device.SetRenderState(RenderState.AlphaBlendEnable, true);
            _device.SetRenderState(RenderState.AlphaTestEnable, true);
            _device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            _device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            _device.SetRenderState(RenderState.AlphaFunc, Compare.GreaterEqual);
            _device.SetRenderState(RenderState.CullMode, Cull.Clockwise);
            _device.VertexFormat = VertexPositionNormal.Format;

            var l = new Light
            {
                Specular = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                Type = LightType.Directional,
                Direction = new Vector3(1, 1, -1)
            };
            _device.SetLight(0, ref l);
            _device.EnableLight(0, true);
            _device.SetRenderState(RenderState.Ambient, new Color4(0.3f, 0.3f, 0.3f, 1.0f).ToArgb());
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
            using (_view.StartRenderPass(_renderTarget.NativePointer))
            {
                _device.BeginScene();

                _camera.Sample(dt);
                var view = _camera.WorldView;
                _device.SetTransform(TransformState.View, ref view);

                _device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new Color4(0.25f, 0.25f, 0.25f, 1.0f), 1.0f, 0);
                if (_service.HasLoadedTile) _sceneRenderer.Render(_service.Scene);

                _device.EndScene();

                _lastRenderingTime = renderingTime;
            }
        }
    }
}