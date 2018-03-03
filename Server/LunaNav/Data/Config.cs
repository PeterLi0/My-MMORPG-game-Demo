namespace LunaNav
{
    public class Config
    {
        public float CellSize { get; set; }
        public float CellHeight { get; set; }
        public float WalkableSlopeAngle { get; set; }
        public int WalkableHeight { get; set; }
        public int WalkableClimb { get; set; }
        public int WalkableRadius { get; set; }
        public int MaxEdgeLength { get; set; }
        public float MaxSimplificationError { get; set; }
        public int MinRegionArea { get; set; }
        public int MergeRegionArea { get; set; }
        public int MaxVertexesPerPoly { get; set; }
        public float DetailSampleDistance { get; set; }
        public float DetailSampleMaxError { get; set; }
        public RecastVertex MinBounds { get; set; }
        public RecastVertex MaxBounds { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BorderSize { get; set; }
        public int TileSize { get; set; }

        public Config()
        {
        }

        public void CalculateGridSize(Geometry geom)
        {
            Width = (int) ((geom.MaxBounds.X - geom.MinBounds.X)/CellSize + 0.5f);
            Height = (int) ((geom.MaxBounds.Z - geom.MinBounds.Z)/CellSize + 0.5f);
            MaxBounds = geom.MaxBounds;
            MinBounds = geom.MinBounds;
        }
    }
}