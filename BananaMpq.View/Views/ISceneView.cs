using System;

namespace BananaMpq.View.Views
{
    public interface ISceneView
    {
        IntPtr WindowHandle { get; }
        IDisposable StartRenderPass(IntPtr surfacePointer);
        void OpenTileSelectionDialog();
    }
}