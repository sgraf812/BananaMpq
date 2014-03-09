using System.Collections.Generic;
using BananaMpq.View.Infrastructure;
using BananaMpq.View.Rendering;

namespace BananaMpq.View.Views
{
    public interface INavigationMeshSettingsView
    {
        IList<INavigationMeshRenderer> Renderers { set; }
        INavigationMeshRenderer SelectedRenderer { set; }
        BuildConfiguration BuildConfig { get; }
        void BindTo(BuildConfiguration configuration);
    }
}