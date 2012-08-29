using System;
using System.Collections.Generic;
using System.Linq;
using BananaMpq.Layer;
using BananaMpq.View.Infrastructure;
using BananaMpq.View.Views;
using SharpDX;

namespace BananaMpq.View.Presenters
{
    public class TileSelectionPresenter
    {
        private readonly SceneService _service = SceneService.Instance;
        private WowContinent? _lastContinent;
        private static readonly IEnumerable<int> WdtCoordinateRange = Enumerable.Range(0, Wdt.AdtsPerSide);

        public void ConductModal(ITileSelectionView view)
        {
            if (_lastContinent.HasValue) view.Continent = _lastContinent.Value;
            view.Loaded += LoadNewWdtMap;
            view.ContinentChanged += LoadNewWdtMap;
            view.TileSelected += LoadSelectedTile;
            view.ShowModal();
            view.TileSelected -= LoadSelectedTile;
            view.ContinentChanged -= LoadNewWdtMap;
        }

        private void LoadSelectedTile(object sender, TileSelectionEventArgs e)
        {
            _lastContinent = e.Continent;
            _service.LoadTile(e.Continent, e.X, e.Y);
        }

        private void LoadNewWdtMap(object sender, EventArgs e)
        {
            var view = (ITileSelectionView)sender;
            var wdt = _service.Files.GetWdt(view.Continent);
            var tileCoordinates = (from x in WdtCoordinateRange
                                   from y in WdtCoordinateRange
                                   where wdt.AdtExistsForTile(x, y)
                                   select new Vector2(x, y)).ToArray();
            var topLeft = new Vector2(tileCoordinates.Min(t => t.X), tileCoordinates.Min(t => t.Y));
            var bottomRight = new Vector2(tileCoordinates.Max(t => t.X), tileCoordinates.Max(t => t.Y));

            view.DisplayTiles(tileCoordinates, topLeft, bottomRight);
        }
    }
}