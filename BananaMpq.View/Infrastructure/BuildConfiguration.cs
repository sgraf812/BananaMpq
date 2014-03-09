using System;
using BananaMpq.Layer;
using SharpDX;

namespace BananaMpq.View.Infrastructure
{
    public class BuildConfiguration
    {
        private int _tilesPerSide;
        private int _cellsPerTileSide;
        private int _cellsPerAgentHeight;
        private float _agentRadius;
        private float _agentHeight;
        private float _walkableClimb;
        private float _borderSize;

        #region Voxelization

        public float TileSize { get { return Adt.AdtWidth/TilesPerSide; } }
        public int TilesPerSide { get { return _tilesPerSide; } set { _tilesPerSide = value; ComputeCellSizes(); } }
        public int CellsPerTileSide { get { return _cellsPerTileSide; } set { _cellsPerTileSide = value; ComputeCellSizes(); } }
        public float AgentRadius { get { return _agentRadius; } set { _agentRadius = value; ComputeCellSizes(); } }
        public float AgentHeight { get { return _agentHeight; } set { _agentHeight = value; ComputeCellSizes(); } }
        public int CellsPerAgentHeight { get { return _cellsPerAgentHeight; } set { _cellsPerAgentHeight = value; ComputeCellSizes(); } }
        public float WalkableClimb { get { return _walkableClimb; } set { _walkableClimb = value; ComputeCellSizes(); } }
        public float BorderSize { get { return _borderSize; } set { _borderSize = value; ComputeCellSizes(); } }
        public AngleSingle WalkableSlope { get; set; }

        public float CellHeight { get; private set; }
        public float CellSize { get; private set; }
        public float InvertedCellHeight { get; private set; }
        public float InvertedCellWidth { get; private set; }
        public int MaxCellStep { get; private set; }
        public int BorderCells { get; private set; }
        public int CellRadius { get; private set; }

        public int CellsPerTileSideIncludingBorder { get { return CellsPerTileSide + 2 * BorderCells; } }

        #endregion

        #region Contours

        public int MinRegionArea { get; set; }
        public int MergeRegionArea { get; set; }
        public bool TesselateSolidEdges { get; set; }
        public bool TesselateEdgesBetweenAreas { get; set; }
        public int MaxEdgeLength { get { return CellRadius*8; } }
        public int MaxEdgeDeviation { get; set; }

        #endregion

        #region PolyMesh

        public int MaxVertsPerPoly { get; set; }

        #endregion

        #region DetailMesh

        public float ContourSampleDistance { get; set; }
        public float MaxContourDeviation { get; set; }

        #endregion

        private void ComputeCellSizes()
        {
            CellHeight = AgentHeight / CellsPerAgentHeight;
            CellSize = TileSize / CellsPerTileSide;
            InvertedCellHeight = CellsPerAgentHeight / AgentHeight;
            InvertedCellWidth = CellsPerTileSide / TileSize;
            CellRadius = (int)Math.Ceiling(_agentRadius * InvertedCellWidth);
            MaxCellStep = (int)(WalkableClimb * InvertedCellHeight);
            BorderCells = (int)(BorderSize * InvertedCellWidth);
        }
    }
}