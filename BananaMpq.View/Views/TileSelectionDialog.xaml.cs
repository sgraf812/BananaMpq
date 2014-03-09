using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using BananaMpq.View.Infrastructure;
using SharpDX;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace BananaMpq.View.Views
{
    public partial class TileSelectionDialog : Window, ITileSelectionView
    {
        private const double ZoomStep = 1.25;
        private const int TileSize = 32;
        private static readonly SolidColorBrush StrokeBrush = new SolidColorBrush(Colors.Black);
        private static readonly SolidColorBrush DefaultFillBrush = new SolidColorBrush(Colors.Green);
        private static readonly SolidColorBrush SelectedFillBrush = new SolidColorBrush(Colors.OrangeRed);
        private Vector2? _selectedTile;
        private Rectangle _selectedRectangle;
        private Vector2 _mapSize;
        private double _scale;

        public TileSelectionDialog()
        {
            InitializeComponent();
            _cboContinent.SelectionChanged += (s, e) => ContinentChanged(this, EventArgs.Empty);
            _btnOk.Click += OpenSelectedTile;
            _btnCancel.Click += (s, e) => Close();
            _blueContainer.SizeChanged += (s, e) => ResetScale();
            _tileContainer.MouseWheel += ZoomOneStep;
            _tileContainer.MouseLeave += (s, e) => _lblHover.Content = null;
        }

        private void ZoomOneStep(object sender, MouseWheelEventArgs e)
        {
            _scale *= Math.Pow(ZoomStep, e.Delta/360.0);
            if (_scale < FullScreenScale) _scale = FullScreenScale;
            _tileContainer.LayoutTransform = new ScaleTransform(_scale, _scale);
            e.Handled = true;
        }

        private void OpenSelectedTile(object s, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            if (_selectedTile.HasValue)
            {
                TileSelected(this, new TileSelectionEventArgs(Continent, (int)_selectedTile.Value.X, (int)_selectedTile.Value.Y));
            }
        }

        #region Implementation of ITileSelectionView

        public event EventHandler ContinentChanged = (s, e) => { };

        event EventHandler ITileSelectionView.Loaded
        {
            add { Loaded += value.ConvertTo<RoutedEventHandler>(); }
            remove { Loaded -= value.ConvertTo<RoutedEventHandler>(); }
        }

        public event EventHandler<TileSelectionEventArgs> TileSelected = (s, e) => { };

        public WowContinent Continent
        {
            get { return (WowContinent)_cboContinent.SelectedItem; }
            set { _cboContinent.SelectedItem = value; }
        }

        public void ShowModal()
        {
            ShowDialog();
        }

        public void DisplayTiles(IEnumerable<Vector2> tileCoordinates, Vector2 minimum, Vector2 maximum)
        {
            var tileCount = (maximum - minimum) + Vector2.One;
            _tileContainer.Children.Clear();

            foreach (var tile in tileCoordinates)
            {
                _tileContainer.Children.Add(CreateRectangle(minimum, tile));
            }
            _mapSize = tileCount*(TileSize - 1) + Vector2.One;
            ResetScale();
        }

        private Rectangle CreateRectangle(Vector2 minimum, Vector2 tile)
        {
            var position = tile - minimum;
            var rectangle = new Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Fill = DefaultFillBrush,
                Stroke = StrokeBrush,
                Width = TileSize,
                Height = TileSize,
                Margin = new Thickness(position.X*(TileSize - 1), position.Y*(TileSize - 1), 0, 0)
            };

            var currentTile = tile;
            rectangle.MouseLeftButtonUp += (s, e) =>
            {
                if (_selectedRectangle != null)
                {
                    _selectedRectangle.Fill = DefaultFillBrush;
                    _selectedRectangle = rectangle;
                }
                rectangle.StrokeThickness = 1;
                rectangle.Fill = SelectedFillBrush;
                _selectedRectangle = rectangle;
                _selectedTile = currentTile;
                _lblSelected.Content = currentTile;
            };

            rectangle.MouseEnter += (s, e) => _lblHover.Content = currentTile;
            return rectangle;
        }

        private void ResetScale()
        {
            _scale = FullScreenScale;
            if (double.IsNaN(_scale) || double.IsPositiveInfinity(_scale)) _scale = 1;
            _tileContainer.LayoutTransform = new ScaleTransform(_scale, _scale);
        }

        private double FullScreenScale
        {
            get { return Math.Min(_blueContainer.ActualWidth/_mapSize.X, _blueContainer.ActualHeight/_mapSize.Y); }
        }

        #endregion
    }
}
