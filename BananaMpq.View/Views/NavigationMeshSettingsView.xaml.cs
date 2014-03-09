using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using BananaMpq.View.Infrastructure;
using BananaMpq.View.Presenters;
using BananaMpq.View.Rendering;
using SharpDX;

namespace BananaMpq.View.Views
{
    /// <summary>
    /// Interaktionslogik für NavigationMeshSettingsView.xaml
    /// </summary>
    public partial class NavigationMeshSettingsView : UserControl, INavigationMeshSettingsView
    {
        private const string RendererButtonGroupName = "RendererButtons";
        private readonly NavMeshPresenter _presenter;
        private bool _rebinding;

        public NavigationMeshSettingsView()
        {
            _presenter = new NavMeshPresenter(this);
            InitializeComponent();
            _tilesPerSide.ValueChanged += ValuesChanged;
            _cellsPerTileSide.ValueChanged += ValuesChanged;
            _agentRadius.ValueChanged += ValuesChanged;
            _agentHeight.ValueChanged += ValuesChanged;
            _cellsPerAgentHeight.ValueChanged += ValuesChanged;
            _walkableClimb.ValueChanged += ValuesChanged;
            _walkableSlope.ValueChanged += ValuesChanged;
            _borderSize.ValueChanged += ValuesChanged;
            _minRegionArea.ValueChanged += ValuesChanged;
            _mergeRegionArea.ValueChanged += ValuesChanged;
            _tesselateSolidEdges.Checked += ValuesChanged;
            _tesselateEdgesBetweenAreas.Checked += ValuesChanged;
            _maxEdgeDeviation.ValueChanged += ValuesChanged;
            _vertsPerPoly.ValueChanged += ValuesChanged;
            _contourSampleDistance.ValueChanged += ValuesChanged;
            _maxContourDeviation.ValueChanged += ValuesChanged;
        }

        private void ValuesChanged(object sender, EventArgs e)
        {
            if (_rebinding) return;
            _rebinding = true;
            BindTo(BuildConfig);
            _rebinding = false;
        }

        #region Implementation of INavigationMeshSettingsView

        public IList<INavigationMeshRenderer> Renderers
        {
            set
            {
                _rendererPanel.Children.Clear();
                foreach (var renderer in value)
                {
                    var radioButton = new RadioButton
                    {
                        GroupName = RendererButtonGroupName,
                        Tag = renderer,
                        Focusable = false,
                        Content = renderer.ToString(),
                    };
                    radioButton.Checked += (s, e) => _presenter.SelectRenderer(renderer);
                    _rendererPanel.Children.Add(radioButton);
                }
            }
        }

        public INavigationMeshRenderer SelectedRenderer
        {
            set
            {
                var button = _rendererPanel.Children.OfType<RadioButton>().FirstOrDefault(r => r.Tag == value);
                button = button ?? _rendererPanel.Children.OfType<RadioButton>().FirstOrDefault();
                if (button != null)
                    button.IsChecked = true;
            }
        }

        public BuildConfiguration BuildConfig
        {
            get
            {
                return new BuildConfiguration
                {
                    TilesPerSide = (int)_tilesPerSide.CurrentValue,
                    CellsPerTileSide = (int)_cellsPerTileSide.CurrentValue,
                    AgentRadius = (float)_agentRadius.CurrentValue,
                    AgentHeight = (float)_agentHeight.CurrentValue,
                    CellsPerAgentHeight = (int)_cellsPerAgentHeight.CurrentValue,
                    WalkableClimb = (float)_walkableClimb.CurrentValue,
                    WalkableSlope = new AngleSingle((float)_walkableSlope.CurrentValue, AngleType.Degree),
                    BorderSize = (float)_borderSize.CurrentValue,
                    MinRegionArea = (int)_minRegionArea.CurrentValue,
                    MergeRegionArea = (int)_mergeRegionArea.CurrentValue,
                    TesselateSolidEdges = (bool)_tesselateSolidEdges.IsChecked,
                    TesselateEdgesBetweenAreas = (bool)_tesselateEdgesBetweenAreas.IsChecked,
                    MaxEdgeDeviation = (int)_maxEdgeDeviation.CurrentValue,
                    MaxVertsPerPoly = (int)_vertsPerPoly.CurrentValue,
                    ContourSampleDistance = (float)_contourSampleDistance.CurrentValue,
                    MaxContourDeviation = (float)_maxContourDeviation.CurrentValue,
                };
            }
        }

        public void BindTo(BuildConfiguration configuration)
        {
            _tilesPerSide.CurrentValue = configuration.TilesPerSide;
            _cellsPerTileSide.CurrentValue = configuration.CellsPerTileSide;
            _agentRadius.CurrentValue = configuration.AgentRadius;
            _agentHeight.CurrentValue = configuration.AgentHeight;
            _cellsPerAgentHeight.CurrentValue = configuration.CellsPerAgentHeight;
            _walkableClimb.CurrentValue = configuration.WalkableClimb;
            _walkableSlope.CurrentValue = configuration.WalkableSlope.Degrees;
            _borderSize.CurrentValue = configuration.BorderSize;
            _minRegionArea.CurrentValue = configuration.MinRegionArea;
            _mergeRegionArea.CurrentValue = configuration.MergeRegionArea;
            _tesselateSolidEdges.IsChecked = configuration.TesselateSolidEdges;
            _tesselateEdgesBetweenAreas.IsChecked = configuration.TesselateEdgesBetweenAreas;
            _maxEdgeLength.Text = configuration.MaxEdgeLength.ToString("F1");
            _maxEdgeDeviation.CurrentValue = configuration.MaxEdgeDeviation;
            _vertsPerPoly.CurrentValue = configuration.MaxVertsPerPoly;
            _contourSampleDistance.CurrentValue = configuration.ContourSampleDistance;
            _maxContourDeviation.CurrentValue = configuration.MaxContourDeviation;
        }

        #endregion

        public void HandleNewNavigationMeshRenderers(IList<INavigationMeshRenderer> renderers)
        {
            _presenter.HandleNewNavigationMeshRenderers(renderers);
        }

        public void BuildNavMesh()
        {
            _presenter.Build();
        }
    }
}
