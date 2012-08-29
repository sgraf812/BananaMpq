using System;
using System.Collections.Generic;
using SharpDX;

namespace BananaMpq.View.Views
{
    public interface ITileSelectionView
    {
        event EventHandler ContinentChanged;
        event EventHandler Loaded;
        event EventHandler<TileSelectionEventArgs> TileSelected;
        WowContinent Continent { get; set; }
        void ShowModal();
        void DisplayTiles(IEnumerable<Vector2> tileCoordinates, Vector2 minimum, Vector2 maximum);
    }
}