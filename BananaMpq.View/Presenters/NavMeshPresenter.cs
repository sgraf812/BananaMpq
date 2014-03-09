using System.Collections.Generic;
using System.Linq;
using BananaMpq.View.Infrastructure;
using BananaMpq.View.Rendering;
using BananaMpq.View.Views;

namespace BananaMpq.View.Presenters
{
    public class NavMeshPresenter
    {
        private readonly INavigationMeshSettingsView _view;
        private readonly SceneService _service = SceneService.Instance;

        public NavMeshPresenter(INavigationMeshSettingsView view)
        {
            _view = view;
        }

        public void HandleNewNavigationMeshRenderers(IList<INavigationMeshRenderer> renderers)
        {
            _view.Renderers = renderers;
            var renderer = _service.CurrentNavigationMeshRenderer;
            INavigationMeshRenderer newRenderer = null;
            if (renderer != null)
            {
                newRenderer = renderers.FirstOrDefault(r => r.GetType() == renderer.GetType());
            }
            SelectRenderer(newRenderer);
        }

        public void SelectRenderer(INavigationMeshRenderer renderer)
        {
            _service.CurrentNavigationMeshRenderer = renderer;
            _view.SelectedRenderer = renderer;
        }

        public void Build()
        {
            _service.BuildNavMesh(_view.BuildConfig);
        }
    }
}