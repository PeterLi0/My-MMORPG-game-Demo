using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
//using System.Threading.Tasks;
using LunaNav;
//using UnityEditor;
//using UnityEngine;
using System.Collections;
using NavMesh = LunaNav.NavMesh;
using Luna3D;

//[AddComponentMenu("Recast/NavMesh")]
//[ExecuteInEditMode]
[Serializable]
public class RecastNavMesh
{

    public bool IsBuilding = false;
    public int Progress = 0;
    public int Total = 0;

    public NavMesh NavMesh { get; set; }
    //public Material Material { get; set; }

    public List<Vector3> NavMeshVerts { get; set; }
    public List<int> NavMeshTriangles { get; set; }
    //public List<Color> NavMeshColors { get; set; }
    public List<Vector2> NavMeshUVs { get; set; }

    public float AgentHeight;
    public float AgentRadius;
    public float AgentMaxClimb;

    public bool MeshActive = false;

    //public GameObject MeshObject;
    //public MeshFilter MeshFilter;
    //public MeshRenderer MeshRenderer;
    //public Mesh RecastMesh;
    //public Material Mat { get; set; }

    public Config Config { get; set; }
    public Geometry Geometry { get; set; }

    private int TileWidth { get; set; }
    private int TileHeight { get; set; }

    public int NumTiles { get { return NavMesh._tiles.Length; } }


    // Multi threading values
    //CancellationTokenSource tokenSource = new CancellationTokenSource();
    //TaskScheduler scheduler = TaskScheduler.Default;
    //List<Task> tasks = new List<Task>();


    // Use this for initialization
    void Start ()
    {
        //Mat = Resources.Load("vertexMat") as Material;
        //if (MeshObject != null && MeshObject.GetComponent<MeshRenderer>() != null)
        //{
        //    MeshObject.GetComponent<MeshRenderer>().materials = new []{Mat};
        //}
    }
	
	// Update is called once per frame
    //void OnDrawGizmos () {
    //    if (ContourSet != null)
    //    {
    //        Vector3 prevVec = new Vector3();
    //        Vector3 thisVec;

    //        //for (int i = 0; i < ContourSet.NConts; i++)
    //        //{
    //        //    Contour c = _contourSet.Conts[i];
    //        //    for (int j = 0; j < c.NRVerts; j++)
    //        //    {
    //        //        int v = j * 4;
    //        //        thisVec = new Vector3(_config.MinBounds.X + c.RVerts[v + 0] * _config.CellSize, _config.MinBounds.Y + (c.RVerts[v + 1] + 1 + (i & 1)) * _config.CellHeight, _config.MinBounds.Z + c.RVerts[v + 2] * _config.CellSize);
    //        //        if (j != 0)
    //        //        {
    //        //            Debug.DrawLine(prevVec, thisVec, duIntToCol(c.Reg, 255));
    //        //        }
    //        //        prevVec = thisVec;
    //        //    }
    //        //    Debug.DrawLine(prevVec, new Vector3(_config.MinBounds.X + c.RVerts[0] * _config.CellSize, _config.MinBounds.Y + (c.RVerts[1] + 1 + (i & 1)) * _config.CellHeight, _config.MinBounds.Z + c.RVerts[2] * _config.CellSize));
    //        //}

    //        for (int i = 0; i < ContourSet.NConts; i++)
    //        {
    //            Contour c = ContourSet.Conts[i];
    //            int nverts = c.NVerts;
    //            if (nverts == 0)
    //                continue;

    //            for (int j = 0, k = nverts - 1; j < nverts; k = j++)
    //            {
    //                int va = k * 4;
    //                int vb = j * 4;
    //                Debug.DrawLine(new Vector3(Config.MinBounds.X + c.Verts[va + 0] * Config.CellSize, Config.MinBounds.Y + (c.Verts[va + 1] + 1 + (i & 1)) * Config.CellHeight, Config.MinBounds.Z + c.Verts[va + 2] * Config.CellSize),
    //                    new Vector3(Config.MinBounds.X + c.Verts[vb + 0] * Config.CellSize, Config.MinBounds.Y + (c.Verts[vb + 1] + 1 + (i & 1)) * Config.CellHeight, Config.MinBounds.Z + c.Verts[vb + 2] * Config.CellSize),
    //                    duIntToCol(c.Reg, 255));
    //            }
    //        }
    //    }
    //}

    int bit(int a, int b)
    {
        return (a & (1 << b)) >> b;
    }

    //Color duIntToCol(int i, int a)
    //{
    //    int r = bit(i, 1) + bit(i, 3) * 2 + 1;
    //    int g = bit(i, 2) + bit(i, 4) * 2 + 1;
    //    int b = bit(i, 0) + bit(i, 5) * 2 + 1;
    //    return duRGBA(r * 63, g * 63, b * 63, a);
    //}

    //void OnDestroy()
    //{
    //    DestroyImmediate(RecastMesh);
    //    if (MeshObject != null && MeshObject.GetComponent<MeshRenderer>() != null)
    //    {
    //        MeshObject.GetComponent<MeshRenderer>().materials = new[] { Mat };
    //    }
    //}

    //public void BuildGeometry()
    //{
    //    BuildNavMeshGeometry();
    //    DrawNavMeshGeometry();
    //}

    //public void RebuildTiles()
    //{
    //    Progress = 0;
    //    IsBuilding = true;
    //    RecastVertex bmin = Geometry.MinBounds;
    //    RecastVertex bmax = Geometry.MaxBounds;
    //    RecastVertex tileBMin = new RecastVertex();
    //    RecastVertex tileBMax = new RecastVertex();
    //    float tcs = Config.TileSize * Config.CellSize;
    //    Total = TileWidth * TileHeight;
    //    for (int y = 0; y < TileHeight; y++)
    //    {
    //        for (int x = 0; x < TileWidth; x++)
    //        {
    //            Progress = y * TileWidth + x;
    //            tileBMin.X = bmin.X + x * tcs;
    //            tileBMin.Y = bmin.Y;
    //            tileBMin.Z = bmin.Z + y * tcs;

    //            tileBMax.X = bmin.X + (x + 1) * tcs;
    //            tileBMax.Y = bmax.Y;
    //            tileBMax.Z = bmin.Z + (y + 1) * tcs;

    //            EditorUtility.DisplayProgressBar("Generating...", "Generating Tile " + Progress + " of " + Total, Progress / (float)Total);
    //            var builder = BuildTileMesh(x, y, tileBMin, tileBMax);

    //            // remove/add new tile?
    //            if (builder != null)
    //            {
    //                LunaNav.NavMeshBuilder outBuilder;
    //                // nav mesh remove tile
    //                NavMesh.RemoveTile(NavMesh.GetTileRefAt(x, y, 0), out outBuilder);
    //                // nav mesh add tile
    //                long result = 0;
    //                NavMesh.AddTile(builder, NavMesh.TileFreeData, 0, ref result);
    //            }
    //        }
    //    }

    //    EditorUtility.ClearProgressBar();
    //    IsBuilding = false;
    //    BuildGeometry();
    //}

    /// <summary>
    /// Builds the entire NavMesh from the Data gathered by BuildGeometry through the Detail Mesh
    /// Then it creates a GameObject that has the RecastNavMesh.
    /// </summary>
    /// <returns></returns>
    //public long BuildAllTiles(Config config, Geometry geom, int tileWidth, int tileHeight, int maxPolysPerTile, int maxTiles)
    //{
    //    NavMesh = new LunaNav.NavMesh();
    //    NavMeshParams param = new NavMeshParams()
    //    {
    //        Orig = geom.MinBounds.ToArray(),
    //        MaxPolys = maxPolysPerTile,
    //        MaxTiles = maxTiles,
    //        TileWidth = config.TileSize * config.CellSize,
    //        TileHeight = config.TileSize * config.CellSize

    //    };

    //    NavMesh.Init(param);
    //    TileWidth = tileWidth;
    //    TileHeight = tileHeight;
    //    Config = config;
    //    Geometry = geom;
    //    Progress = 0;
    //    IsBuilding = true;
    //    Stopwatch timer = new Stopwatch();
    //    timer.Start();
    //    RecastVertex bmin = geom.MinBounds;
    //    RecastVertex bmax = geom.MaxBounds;
    //    float tcs = config.TileSize * config.CellSize;
    //    Total = TileWidth * TileHeight;
    //    bool canceled = false;
    //    for (int y = 0; y < TileHeight; y++)
    //    {
    //        YLoop(y, tcs, bmin, bmax);
    //    }

    //    if (!canceled)
    //    {
    //        while (Progress != Total)
    //        {
    //            canceled = EditorUtility.DisplayCancelableProgressBar("Generating...", "Generating Tile " + Progress + " of " + Total, Progress/(float) Total);
    //            if (canceled)
    //            {
    //                //tokenSource.Cancel();
    //                break;
    //            }
    //        }
    //    }

    //    //Task.WaitAll(tasks.ToArray());

    //    timer.Stop();

    //    EditorUtility.ClearProgressBar();
    //    IsBuilding = false;
    //    BuildGeometry();
    //    return timer.ElapsedMilliseconds;
    //}

    //private void YLoop(int y, float tcs, RecastVertex bmin, RecastVertex bmax)
    //{
    //    bool canceled = false;
    //    for (int x = 0; x < TileWidth; x++)
    //    {
    //        canceled = EditorUtility.DisplayCancelableProgressBar("Generating...",
    //                                                              "Generating Tile " + Progress + " of " + Total,
    //                                                              Progress / (float)Total);
    //        if (canceled)
    //        {
    //            //tokenSource.Cancel();
    //            break;
    //        }
    //        Xloop(y, x, tcs, bmin, bmax);
    //    }
    //}

    //private void Xloop(int y, int x, float tcs, RecastVertex bmin, RecastVertex bmax)
    //{
    //    RecastVertex tileBMin = new RecastVertex();
    //    RecastVertex tileBMax = new RecastVertex();
    //    tileBMin.X = bmin.X + x*tcs;
    //    tileBMin.Y = bmin.Y;
    //    tileBMin.Z = bmin.Z + y*tcs;

    //    tileBMax.X = bmin.X + (x + 1)*tcs;
    //    tileBMax.Y = bmax.Y;
    //    tileBMax.Z = bmin.Z + (y + 1)*tcs;
    //    bool canceled = EditorUtility.DisplayCancelableProgressBar("Generating...",
    //                                                          "Generating Tile " + Progress + " of " + Total,
    //                                                          Progress / (float)Total);
    //    if (canceled)
    //    {
    //        //tokenSource.Cancel();
    //    }
    //    //var t = Task.Factory.StartNew(() => BuildTile(x, y, tileBMin, tileBMax), tokenSource.Token, TaskCreationOptions.LongRunning, scheduler);
    //    //tasks.Add(t);

    //    BuildTile(x, y, tileBMin, tileBMax);
    //}

    //public void BuildTile(int x, int y, RecastVertex tileBMin, RecastVertex tileBMax)
    //{
    //        var builder = BuildTileMesh(x, y, tileBMin, tileBMax);

    //        // remove/add new tile?
    //        if (builder != null)
    //        {
    //            lock (this)
    //            {
    //                LunaNav.NavMeshBuilder outBuilder;
    //                // nav mesh remove tile
    //                NavMesh.RemoveTile(NavMesh.GetTileRefAt(x, y, 0), out outBuilder);
    //                // nav mesh add tile
    //                long result = 0;
    //                NavMesh.AddTile(builder, NavMesh.TileFreeData, 0, ref result);
    //            }
    //        }
    //        Progress++;

    //}

    //public void BuildTile(int x, int y)
    //{
    //    RecastVertex bmin = Geometry.MinBounds;
    //    RecastVertex bmax = Geometry.MaxBounds;
    //    float tcs = Config.TileSize * Config.CellSize;
    //    RecastVertex tileBMin = new RecastVertex();
    //    RecastVertex tileBMax = new RecastVertex();
    //    tileBMin.X = bmin.X + x * tcs;
    //    tileBMin.Y = bmin.Y;
    //    tileBMin.Z = bmin.Z + y * tcs;

    //    tileBMax.X = bmin.X + (x + 1) * tcs;
    //    tileBMax.Y = bmax.Y;
    //    tileBMax.Z = bmin.Z + (y + 1) * tcs;

    //    var builder = BuildTileMesh(x, y, tileBMin, tileBMax);

    //    // remove/add new tile?
    //    if (builder != null)
    //    {
    //        LunaNav.NavMeshBuilder outBuilder;
    //        // nav mesh remove tile
    //        NavMesh.RemoveTile(NavMesh.GetTileRefAt(x, y, 0), out outBuilder);
    //        // nav mesh add tile
    //        long result = 0;
    //        NavMesh.AddTile(builder, NavMesh.TileFreeData, 0, ref result);
    //    }
    //}

    public void RemoveTile(int x, int y)
    {
        LunaNav.NavMeshBuilder outBuilder;
        // nav mesh remove tile
        NavMesh.RemoveTile(NavMesh.GetTileRefAt(x, y, 0), out outBuilder);
    }

    //private LunaNav.NavMeshBuilder BuildTileMesh(int tx, int ty, RecastVertex min, RecastVertex max)
    //{
    //    Config.Width = Config.TileSize + Config.BorderSize * 2;
    //    Config.Height = Config.TileSize + Config.BorderSize * 2;
    //    Config.MinBounds = min;
    //    Config.MaxBounds = max;
    //    Config.MinBounds.X -= Config.BorderSize * Config.CellSize;
    //    Config.MinBounds.Z -= Config.BorderSize * Config.CellSize;

    //    Config.MaxBounds.X += Config.BorderSize * Config.CellSize;
    //    Config.MaxBounds.Z += Config.BorderSize * Config.CellSize;

    //    HeightField heightfield = new HeightField(Config.Width, Config.Height, Config.MinBounds.ToArray(), Config.MaxBounds.ToArray(), Config.CellSize, Config.CellHeight);


    //    short[] triAreas = new short[Geometry.ChunkyTriMesh.MaxTrisPerChunk];

    //    float[] tbmin = new float[2], tbmax = new float[2];
    //    tbmin[0] = Config.MinBounds.X;
    //    tbmin[1] = Config.MinBounds.Z;

    //    tbmax[0] = Config.MaxBounds.X;
    //    tbmax[1] = Config.MaxBounds.Z;

    //    int[] cid = new int[512];

    //    int ncid = Geometry.ChunkyTriMesh.GetChunksOverlappingRect(tbmin, tbmax, ref cid, 512);

    //    if (ncid == 0)
    //        return null;

    //    for (int i = 0; i < ncid; i++)
    //    {
    //        ChunkyTriMeshNode node = Geometry.ChunkyTriMesh.Nodes[cid[i]];
    //        int[] tris = new int[node.n * 3];
    //        Array.Copy(Geometry.ChunkyTriMesh.Tris, node.i * 3, tris, 0, node.n * 3);
    //        List<int> ctris = new List<int>(tris);
    //        int nctris = node.n;

    //        Array.Clear(triAreas, 0, triAreas.Length);
    //        Geometry.MarkWalkableTriangles(Config.WalkableSlopeAngle, ctris, nctris, ref triAreas);

    //        heightfield.RasterizeTriangles(Geometry, ctris, nctris, triAreas, Config.WalkableClimb);
    //    }

    //    heightfield.FilterLowHangingWalkableObstacles(Config.WalkableClimb);
    //    heightfield.FilterLedgeSpans(Config.WalkableHeight, Config.WalkableClimb);
    //    heightfield.FilterWalkableLowHeightSpans(Config.WalkableHeight);


    //    CompactHeightfield compactHeightfield = new CompactHeightfield(Config.WalkableHeight, Config.WalkableClimb, heightfield);
    //    compactHeightfield.ErodeWalkableArea(Config.WalkableRadius);

    //    // optional convex volumes

    //    compactHeightfield.BuildDistanceField();
    //    compactHeightfield.BuildRegions(Config.BorderSize, Config.MinRegionArea, Config.MergeRegionArea);


    //    ContourSet contourSet = new ContourSet(compactHeightfield, Config.MaxSimplificationError, Config.MaxEdgeLength);

    //    if (contourSet.NConts == 0)
    //        return null;

    //    PolyMesh polyMesh = new PolyMesh(contourSet, Config.MaxVertexesPerPoly);

    //    DetailPolyMesh detailPolyMesh = new DetailPolyMesh(polyMesh, compactHeightfield, Config.DetailSampleDistance,
    //                                                        Config.DetailSampleMaxError);

    //    // Convert the Areas and Flags for path weighting
    //    for (int i = 0; i < polyMesh.NPolys; i++)
    //    {

    //        if (polyMesh.Areas[i] == Geometry.WalkableArea)
    //        {
    //            polyMesh.Areas[i] = 0; // Sample_polyarea_ground
    //            polyMesh.Flags[i] = 1; // Samply_polyflags_walk
    //        }
    //    }
    //    NavMeshCreateParams param = new NavMeshCreateParams
    //    {
    //        Verts = polyMesh.Verts,
    //        VertCount = polyMesh.NVerts,
    //        Polys = polyMesh.Polys,
    //        PolyAreas = polyMesh.Areas,
    //        PolyFlags = polyMesh.Flags,
    //        PolyCount = polyMesh.NPolys,
    //        Nvp = polyMesh.Nvp,
    //        DetailMeshes = detailPolyMesh.Meshes,
    //        DetailVerts = detailPolyMesh.Verts,
    //        DetailVertsCount = detailPolyMesh.NVerts,
    //        DetailTris = detailPolyMesh.Tris,
    //        DetailTriCount = detailPolyMesh.NTris,

    //        // Off Mesh data
    //        OffMeshConVerts = Geometry.OffMeshConnectionVerts.ToArray(),
    //        OffMeshConRad = Geometry.OffMeshConnectionRadii.ToArray(),
    //        OffMeshConDir = Geometry.OffMeshConnectionDirections.ToArray(),
    //        OffMeshConAreas = Geometry.OffMeshConnectionAreas.ToArray(),
    //        OffMeshConFlags = Geometry.OffMeshConnectionFlags.ToArray(),
    //        OffMeshConUserId = Geometry.OffMeshConnectionIds.ToArray(),
    //        OffMeshConCount = (int)Geometry.OffMeshConnectionCount,
    //        // end off mesh data

    //        WalkableHeight = Config.WalkableHeight,
    //        WalkableRadius = Config.WalkableRadius,
    //        WalkableClimb = Config.WalkableClimb,
    //        BMin = new float[] { polyMesh.BMin[0], polyMesh.BMin[1], polyMesh.BMin[2] },
    //        BMax = new float[] { polyMesh.BMax[0], polyMesh.BMax[1], polyMesh.BMax[2] },
    //        Cs = polyMesh.Cs,
    //        Ch = polyMesh.Ch,
    //        BuildBvTree = true,
    //        TileX = tx,
    //        TileY = ty,
    //        TileLayer = 0
    //    };
    //    return new LunaNav.NavMeshBuilder(param);
    //}

    #region NavMesh Geometry Functions

    /// <summary>
    /// Builds the NavMesh geometry into a Unity3d Mesh to display for debug
    /// </summary>
    //public void BuildNavMeshGeometry()
    //{
    //    if (NavMesh != null)
    //    {
    //        NavMeshTriangles = new List<int>();
    //        NavMeshVerts = new List<Vector3>();
    //        NavMeshUVs = new List<Vector2>();
    //        NavMeshColors = new List<Color>();
    //        for (int i = 0; i < NavMesh.GetMaxTiles(); i++)
    //        {
    //            MeshTile tile = NavMesh.GetTile(i);
    //            if (tile.Header == null) continue;
    //            long baseId = NavMesh.GetPolyRefBase(tile);
    //            for (int j = 0; j < tile.Header.PolyCount; j++)
    //            {
    //                BuildNavMeshPoly(baseId | (uint)j, duRGBA(0, 0, 64, 128), NavMeshVerts, NavMeshColors, NavMeshUVs, NavMeshTriangles);
    //            }
    //        }
    //    }
    //}

    /// <summary>
    /// Builds a Single polygon out of the NavMesh, called by BuildNavMeshGeometry
    /// </summary>
    /// <param name="refId"></param>
    /// <param name="color"></param>
    /// <param name="verts"></param>
    /// <param name="colors"></param>
    /// <param name="uvs"></param>
    /// <param name="tris"></param>
    //private void BuildNavMeshPoly(long refId, Color color, List<Vector3> verts, List<Color> colors, List<Vector2> uvs, List<int> tris)
    //{

    //    MeshTile tile = null;
    //    Poly poly = null;
    //    if ((NavMesh.GetTileAndPolyByRef(refId, ref tile, ref poly) & Status.Failure) != 0)
    //        return;

    //    long ip = 0;
    //    for (int i = 0; i < tile.Polys.Length; i++)
    //    {
    //        if (poly == tile.Polys[i])
    //            ip = i;
    //    }

    //    if (poly.Type == LunaNav.NavMeshBuilder.PolyTypeOffMeshConnection)
    //    {
    //    }
    //    else
    //    {
    //        PolyDetail pd = tile.DetailMeshes[ip];
    //        for (int i = 0; i < pd.TriCount; i++)
    //        {
    //            int t = ((int)pd.TriBase + i) * 4;
    //            for (int j = 0; j < 3; j++)
    //            {
    //                if (tile.DetailTris[t + j] < poly.VertCount)
    //                {
    //                    verts.Add(new Vector3(tile.Verts[poly.Verts[tile.DetailTris[t + j]] * 3 + 0], tile.Verts[poly.Verts[tile.DetailTris[t + j]] * 3 + 1], tile.Verts[poly.Verts[tile.DetailTris[t + j]] * 3 + 2]));
    //                }
    //                else
    //                {
    //                    verts.Add(
    //                        new Vector3(tile.DetailVerts[(pd.VertBase + tile.DetailTris[t + j] - poly.VertCount) * 3 + 0],
    //                                    tile.DetailVerts[(pd.VertBase + tile.DetailTris[t + j] - poly.VertCount) * 3 + 1],
    //                                    tile.DetailVerts[(pd.VertBase + tile.DetailTris[t + j] - poly.VertCount) * 3 + 2]));
    //                }
    //                uvs.Add(new Vector2());
    //                colors.Add(color);//duIntToCol((int)ip, 192));
    //                tris.Add(tris.Count);
    //            }
    //        }
    //    }

    //}

    /// <summary>
    /// Converts an RGBA of intgers into a Unity3d Color
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="a"></param>
    /// <returns></returns>
    //Color duRGBA(int r, int g, int b, int a)
    //{
    //    return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    //}

    /// <summary>
    /// Creates a Mesh Object as a child of this object and builds the mesh from the data built by BuildNavMeshGeometry
    /// </summary>
//    public void DrawNavMeshGeometry()
//    {
//        DestroyImmediate(RecastMesh);
//        if (MeshObject != null && MeshObject.GetComponent<MeshRenderer>() != null)
//        {
//            MeshObject.GetComponent<MeshRenderer>().materials = new Material[0];
//        }
//        if (MeshObject == null)
//        {
//            MeshObject = new GameObject("Mesh");
//            MeshObject.transform.parent = this.transform;
//            MeshObject.AddComponent<MeshFilter>();
//            MeshObject.AddComponent<MeshRenderer>();
//        }

//        MeshFilter = MeshObject.GetComponent<MeshFilter>();
//        RecastMesh = new Mesh();
//        RecastMesh.vertices = NavMeshVerts.ToArray();
//        RecastMesh.triangles = NavMeshTriangles.ToArray();
//        RecastMesh.colors = NavMeshColors.ToArray();
//        RecastMesh.uv = NavMeshUVs.ToArray();
//        RecastMesh.RecalculateNormals();
//        MeshFilter.sharedMesh = RecastMesh;
//        MeshObject.GetComponent<MeshRenderer>().sharedMaterial = Mat;
//        MeshActive = true;
//    }

//    public void Toggle()
//    {
//        if (MeshObject != null)
//        {
//            MeshActive = !MeshActive;
//#if UNITY_3_5
//            MeshObject.active = MeshActive;
//#else
//            MeshObject.SetActive(MeshActive);
//#endif
//        }
//    }

    #endregion

    #region Visual Debug Functions
    //private Mesh mesh;

    #region voxel geometry

    //protected List<List<Vector3>> VoxelVerticies { get; set; }
    //protected List<List<int>> VoxelTriangles { get; set; }

    //public void BuildVoxelGeometry()
    //{
    //    VoxelVerticies = new List<List<Vector3>>();
    //    VoxelVerticies.Add(new List<Vector3>());
    //    VoxelTriangles = new List<List<int>>();
    //    VoxelTriangles.Add(new List<int>());
    //    for (int y = 0; y < Heightfield.Height; y++)
    //    {
    //        for (int x = 0; x < Heightfield.Width; x++)
    //        {
    //            float fx = Heightfield.Bmin[0] + x * Heightfield.Cs;
    //            float fz = Heightfield.Bmin[2] + y * Heightfield.Cs;
    //            Span s = Heightfield.Spans[x + y * Heightfield.Width];
    //            while (s != null)
    //            {
    //                AddBox(fx, Heightfield.Bmin[1] + s.SMin * Heightfield.Ch, fz, fx + Heightfield.Cs,
    //                       Heightfield.Bmin[1] + s.SMax * Heightfield.Ch, fz + Heightfield.Cs,
    //                       VoxelVerticies[VoxelVerticies.Count - 1], VoxelTriangles[VoxelTriangles.Count - 1]);
    //                if (VoxelVerticies[VoxelVerticies.Count - 1].Count + 8 > 64992)
    //                {
    //                    VoxelVerticies.Add(new List<Vector3>());
    //                    VoxelTriangles.Add(new List<int>());
    //                }
    //                s = s.Next;
    //            }
    //        }
    //    }
    //}

    //public void DrawVoxelGeometry()
    //{
    //    for (int i = 0; i < VoxelVerticies.Count; i++)
    //    {
    //        meshObject = new GameObject("Mesh" + i);
    //        meshObject.AddComponent<MeshFilter>();
    //        meshObject.AddComponent<MeshRenderer>();
    //        mesh = new Mesh();
    //        mesh.vertices = VoxelVerticies[i].ToArray();
    //        mesh.triangles = VoxelTriangles[i].ToArray();
    //        //mesh.colors = CompactRegionColors[i].ToArray();
    //        //mesh.uv = CompactRegionUVs[i].ToArray();
    //        mesh.RecalculateNormals();
    //        meshFilter = meshObject.GetComponent<MeshFilter>();
    //        meshFilter.mesh = mesh;
    //        meshObject.GetComponent<MeshRenderer>().material = mat;
    //    }
    //}

    #endregion

    #region walkable voxels

    //protected List<List<Vector3>> WalkableVoxelVerticies { get; set; }
    //protected List<List<int>> WalkableVoxelTriangles { get; set; }

    //public void BuildWalkableVoxelGeometry()
    //{
    //    WalkableVoxelVerticies = new List<List<Vector3>>();
    //    WalkableVoxelVerticies.Add(new List<Vector3>());
    //    WalkableVoxelTriangles = new List<List<int>>();
    //    WalkableVoxelTriangles.Add(new List<int>());
    //    for (int y = 0; y < Heightfield.Height; y++)
    //    {
    //        for (int x = 0; x < Heightfield.Width; x++)
    //        {
    //            float fx = Heightfield.Bmin[0] + x * Heightfield.Cs;
    //            float fz = Heightfield.Bmin[2] + y * Heightfield.Cs;
    //            Span s = Heightfield.Spans[x + y * Heightfield.Width];
    //            while (s != null)
    //            {
    //                if (s.Area == Geometry.WalkableArea) // || s.Area == HeightField.NullArea)
    //                {
    //                    AddBox(fx, Heightfield.Bmin[1] + s.SMin * Heightfield.Ch, fz, fx + Heightfield.Cs,
    //                           Heightfield.Bmin[1] + s.SMax * Heightfield.Ch, fz + Heightfield.Cs,
    //                           WalkableVoxelVerticies[WalkableVoxelVerticies.Count - 1],
    //                           WalkableVoxelTriangles[WalkableVoxelTriangles.Count - 1]);
    //                    if (WalkableVoxelVerticies[WalkableVoxelVerticies.Count - 1].Count + 8 > 64992)
    //                    {
    //                        WalkableVoxelVerticies.Add(new List<Vector3>());
    //                        WalkableVoxelTriangles.Add(new List<int>());
    //                    }
    //                }
    //                s = s.Next;
    //            }
    //        }
    //    }
    //}
    //public void DrawWalkableVoxelGeometry()
    //{
    //    for (int i = 0; i < WalkableVoxelVerticies.Count; i++)
    //    {
    //        meshObject = new GameObject("Mesh" + i);
    //        meshObject.AddComponent<MeshFilter>();
    //        meshObject.AddComponent<MeshRenderer>();
    //        mesh = new Mesh();
    //        mesh.vertices = WalkableVoxelVerticies[i].ToArray();
    //        mesh.triangles = WalkableVoxelTriangles[i].ToArray();
    //        //mesh.colors = CompactRegionColors[i].ToArray();
    //        //mesh.uv = CompactRegionUVs[i].ToArray();
    //        mesh.RecalculateNormals();
    //        meshFilter = meshObject.GetComponent<MeshFilter>();
    //        meshFilter.mesh = mesh;
    //        meshObject.GetComponent<MeshRenderer>().material = mat;
    //    }
    //}

    //private void AddBox(float minx, float miny, float minz, float maxx, float maxy, float maxz, List<Vector3> voxelVertices, List<int> voxelTriangles)
    //{
    //    int initialIndex = voxelVertices.Count;
    //    if (initialIndex + 8 < 65000)
    //    {
    //        voxelVertices.Add(new Vector3(minx, miny, minz));
    //        voxelVertices.Add(new Vector3(maxx, miny, minz));
    //        voxelVertices.Add(new Vector3(maxx, miny, maxz));
    //        voxelVertices.Add(new Vector3(minx, miny, maxz));
    //        voxelVertices.Add(new Vector3(minx, maxy, minz));
    //        voxelVertices.Add(new Vector3(maxx, maxy, minz));
    //        voxelVertices.Add(new Vector3(maxx, maxy, maxz));
    //        voxelVertices.Add(new Vector3(minx, maxy, maxz));

    //        // Add faces        7, 6, 5, 4,
    //        voxelTriangles.Add(initialIndex + 7);
    //        voxelTriangles.Add(initialIndex + 6);
    //        voxelTriangles.Add(initialIndex + 5);
    //        voxelTriangles.Add(initialIndex + 7);
    //        voxelTriangles.Add(initialIndex + 5);
    //        voxelTriangles.Add(initialIndex + 4);

    //        // Add faces		0, 1, 2, 3,
    //        voxelTriangles.Add(initialIndex + 0);
    //        voxelTriangles.Add(initialIndex + 1);
    //        voxelTriangles.Add(initialIndex + 2);
    //        voxelTriangles.Add(initialIndex + 0);
    //        voxelTriangles.Add(initialIndex + 2);
    //        voxelTriangles.Add(initialIndex + 3);

    //        // Add faces		1, 5, 6, 2,
    //        voxelTriangles.Add(initialIndex + 1);
    //        voxelTriangles.Add(initialIndex + 5);
    //        voxelTriangles.Add(initialIndex + 6);
    //        voxelTriangles.Add(initialIndex + 1);
    //        voxelTriangles.Add(initialIndex + 6);
    //        voxelTriangles.Add(initialIndex + 2);

    //        // Add faces		3, 7, 4, 0,
    //        voxelTriangles.Add(initialIndex + 3);
    //        voxelTriangles.Add(initialIndex + 7);
    //        voxelTriangles.Add(initialIndex + 4);
    //        voxelTriangles.Add(initialIndex + 3);
    //        voxelTriangles.Add(initialIndex + 4);
    //        voxelTriangles.Add(initialIndex + 0);

    //        // Add faces		2, 6, 7, 3,
    //        voxelTriangles.Add(initialIndex + 2);
    //        voxelTriangles.Add(initialIndex + 6);
    //        voxelTriangles.Add(initialIndex + 7);
    //        voxelTriangles.Add(initialIndex + 2);
    //        voxelTriangles.Add(initialIndex + 7);
    //        voxelTriangles.Add(initialIndex + 3);

    //        // Add faces		0, 4, 5, 1,
    //        voxelTriangles.Add(initialIndex + 0);
    //        voxelTriangles.Add(initialIndex + 4);
    //        voxelTriangles.Add(initialIndex + 5);
    //        voxelTriangles.Add(initialIndex + 0);
    //        voxelTriangles.Add(initialIndex + 5);
    //        voxelTriangles.Add(initialIndex + 1);
    //    }
    //}
#endregion

    #region build polymesh geometry

    //protected List<Vector3> PolyVerticies { get; set; }
    //protected List<int> PolyTriangles { get; set; }
    //protected List<Color> PolyColors { get; set; }
    //protected List<Vector2> PolyUVs { get; set; }

    //public void BuildPolyMeshGeometry()
    //{
    //    PolyVerticies = new List<Vector3>();
    //    PolyTriangles = new List<int>();
    //    PolyColors = new List<Color>();
    //    PolyUVs = new List<Vector2>();
    //    int tricount = 0;
    //    for (int i = 0; i < PolyMesh.NPolys; ++i)
    //    {
    //        int p = i * PolyMesh.Nvp * 2;

    //        int[] vi = new int[3];
    //        for (int j = 2; j < PolyMesh.Nvp; ++j)
    //        {
    //            if (PolyMesh.Polys[p + j] == PolyMesh.MeshNullIdx) break;
    //            vi[0] = PolyMesh.Polys[p + 0];
    //            vi[1] = PolyMesh.Polys[p + j - 1];
    //            vi[2] = PolyMesh.Polys[p + j];
    //            for (int k = 0; k < 3; ++k)
    //            {
    //                int v = vi[k] * 3;
    //                float x = PolyMesh.BMin[0] + PolyMesh.Verts[v + 0] * Config.CellSize;
    //                float y = PolyMesh.BMin[1] + (PolyMesh.Verts[v + 1] + 1) * Config.CellHeight;
    //                float z = PolyMesh.BMin[2] + PolyMesh.Verts[v + 2] * Config.CellSize;
    //                PolyVerticies.Add(new Vector3(x, y, z));
    //                PolyColors.Add(duRGBA(0, 192, 255, 64));
    //                PolyUVs.Add(new Vector2(0, 0));
    //                PolyTriangles.Add(tricount++);
    //            }
    //        }
    //    }
    //}

    //public void DrawPolyMeshGeometry()
    //{
    //    meshObject = new GameObject("Mesh");
    //    meshObject.AddComponent<MeshFilter>();
    //    meshObject.AddComponent<MeshRenderer>();
    //    mesh = new Mesh();
    //    mesh.vertices = PolyVerticies.ToArray();
    //    mesh.triangles = PolyTriangles.ToArray();
    //    mesh.colors = PolyColors.ToArray();
    //    mesh.uv = PolyUVs.ToArray();
    //    mesh.RecalculateNormals();
    //    meshFilter = meshObject.GetComponent<MeshFilter>();
    //    meshFilter.mesh = mesh;
    //    meshObject.GetComponent<MeshRenderer>().material = mat;
    //}

    #endregion

    #endregion
}
