using System.Collections.Generic;
using BananaMpq.View.Rendering;

namespace BananaMpq.View.Presenters
{
    class NavigationMeshRenderersEventArgs
    {
        public NavigationMeshRenderersEventArgs(IList<INavigationMeshRenderer> navMeshRenderers)
        {
            NavMeshRenderers = navMeshRenderers;
        }

        public IList<INavigationMeshRenderer> NavMeshRenderers { get; private set; }
    }
}