using System.Collections.Generic;
using System.Windows.Controls;
using BananaMpq.View.Models;

namespace BananaMpq.View.Views
{
    public partial class TreeChunkHierarchyView : UserControl, IChunkHierarchyView
    {
        public TreeChunkHierarchyView()
        {
            InitializeComponent();
        }

        #region Implementation of IChunkHierarchyView

        public IEnumerable<ChunkHierarchyModel> Hierarchy
        {
            set { _treeView.ItemsSource = value; }
        }

        #endregion
    }
}
