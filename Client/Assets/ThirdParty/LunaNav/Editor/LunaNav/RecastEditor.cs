using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LunaNav;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class RecastEditor : EditorWindow
{
    private static bool _initialized;
    private Vector2 _scrollPosition;
    private Config _config;
    private Geometry _geom;

    public bool ShowTools { get; set; }
    public bool ShowLog { get; set; }
    public bool MonotonePartitioning { get; set; }
    public bool KeepItermediate { get; set; }
    public int Tags { get; set; }
    private int OldTags { get; set; }
    public GameObject InputMesh { get; set; }

    public float CellSize { get; set; }
    public float CellHeight { get; set; }
    public float AgentHeight { get; set; }
    public float AgentRadius { get; set; }
    public float AgentMaxClimb { get; set; }
    public float EdgeMaxError { get; set; }
    public float BuildTime { get; set; }

    public int AgentMaxSlope { get; set; }
    public int RegionMinSize { get; set; }
    public int RegionMergeSize { get; set; }
    public int EdgeMaxLen { get; set; }
    public int VertsPerPoly { get; set; }
    public int DetailSampleDist { get; set; }
    public int DetailSampleMaxError { get; set; }

    public int TileSize { get; set; }
    private int OldTileSize { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public int MaxTiles { get; set; }
    public int MaxPolysPerTile { get; set; }

    public int Verts { get; set; }
    public int Tris { get; set; }

    /// <summary>
    /// Builds the recast editor that is used to build the navmesh
    /// </summary>
    [MenuItem("Window/Recast/Generate a Nav Mesh")]
    static void Initialize()
    {
        //It has to be the typeof from the Class you creating. In this case... CreationPanel
        var window = GetWindow<RecastEditor>("Recast Generator");
        _initialized = true;
        window.ResetDefaults();
    }

    /// <summary>
    /// Creates a NavMeshQuery Object for Crowd and Actors
    /// </summary>
    [MenuItem("GameObject/Create Other/Recast/NavMesh Query")]
    static void CreateQuery()
    {
        if (FindObjectOfType(typeof (RecastNavMeshQuery)) == null)
        {
            GameObject go = new GameObject("RecastNavMeshQuery");
            go.AddComponent<RecastNavMeshQuery>();
        }
    }

    /// <summary>
    /// Creates a NavMeshQuery Object for Crowd and Actors
    /// </summary>
    [MenuItem("GameObject/Create Other/Recast/OffMesh Connection")]
    static void CreateOffMeshConnection()
    {
        GameObject go = new GameObject("OffMeshConnection");
        go.AddComponent<OffMeshConnector>();
    }


    /// <summary>
    /// Creates a DetourActor Object for Crowd
    /// </summary>
    [MenuItem("GameObject/Create Other/Recast/Detour Actor")]
    static void CreateActor()
    {
        GameObject go = new GameObject("DetourActor");
        go.AddComponent<DetourActor>();
    }

    [MenuItem("Window/Recast/Getting Started")]
    static void GettingStarted()
    {
        Application.OpenURL("http://www.cjrgaming.com/index.php/products/recast-menu");
    }

    void OnGUI()
    {
        if (!_initialized)
        {
            Initialize();
        }

        DrawPropertiesPanel();
    }

    /// <summary>
    /// Draws all the editor properties needed to generate a NavMesh with Recast
    /// </summary>
    private void DrawPropertiesPanel()
    {
        var catagoryStyle = new GUIStyle();
        catagoryStyle.fontStyle = FontStyle.Bold;

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false); //Last two bools are if we want to show the scroll bar always.
        EditorGUILayout.BeginVertical();
        EditorGUILayout.PrefixLabel("Properties", GUIStyle.none, catagoryStyle);

        Tags = EditorGUILayout.MaskField("Tags", Tags, UnityEditorInternal.InternalEditorUtility.tags);

        if (Tags != OldTags)
        {
            BuildConfig();
            OldTags = Tags;
        }

        // -1 = everything
        // 0 = nothing
        // >0 = tags up to "everything"
        if (Tags != 0)
        {
            EditorGUILayout.LabelField(
                string.Format("Verts: {0}", Verts),
                string.Format("Tris: {0}", Tris));
        }
        EditorGUILayout.Separator();

        EditorGUILayout.PrefixLabel("Rasterization", GUIStyle.none, catagoryStyle);
        CellSize = EditorGUILayout.Slider("Cell Size", CellSize, 0f, 1f);
        CellHeight = EditorGUILayout.Slider("Cell Height", CellHeight, 0f, 1f);

        EditorGUILayout.Separator();

        EditorGUILayout.PrefixLabel("Agent", GUIStyle.none, catagoryStyle);
        AgentHeight = EditorGUILayout.Slider("Height", AgentHeight, 0f, 5f);
        AgentRadius = EditorGUILayout.Slider("Radius", AgentRadius, 0f, 5f);
        AgentMaxClimb = EditorGUILayout.Slider("Max Climb", AgentMaxClimb, 0f, 5f);
        AgentMaxSlope = EditorGUILayout.IntSlider("Max Slope", AgentMaxSlope, 0, 90);

        EditorGUILayout.Separator();

        EditorGUILayout.PrefixLabel("Region", GUIStyle.none, catagoryStyle);
        RegionMinSize = EditorGUILayout.IntSlider("Min Region Size", RegionMinSize, 0, 150);
        RegionMergeSize = EditorGUILayout.IntSlider("Merged Region Size", RegionMergeSize, 0, 150);

        //EditorGUILayout.BeginHorizontal();
        //MonotonePartitioning = EditorGUILayout.Toggle(MonotonePartitioning, GUILayout.Width(20));
        //EditorGUILayout.LabelField(new GUIContent("Monotone Partitioning", "Not yet implemented"), catagoryStyle);
        //EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        EditorGUILayout.PrefixLabel("Polygonization", GUIStyle.none, catagoryStyle);
        EdgeMaxLen = EditorGUILayout.IntSlider("Max Edge Length", EdgeMaxLen, 0, 50);
        EdgeMaxError = EditorGUILayout.Slider("Max Edge Error", EdgeMaxError, 0.1f, 3f);
        VertsPerPoly = EditorGUILayout.IntSlider("Verts Per Poly", VertsPerPoly, 3, 12);

        EditorGUILayout.Separator();

        EditorGUILayout.PrefixLabel("Detail Mesh", GUIStyle.none, catagoryStyle);
        DetailSampleDist = EditorGUILayout.IntSlider("Sample Distance", DetailSampleDist, 0, 50);
        DetailSampleMaxError = EditorGUILayout.IntSlider("Max Sample Error", DetailSampleMaxError, 0, 16);

        //EditorGUILayout.Space();
        //EditorGUILayout.BeginHorizontal();
        //KeepItermediate = EditorGUILayout.Toggle(KeepItermediate, GUILayout.Width(20));
        //EditorGUILayout.LabelField("Keep Intermediate Results", catagoryStyle);
        //EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        EditorGUILayout.PrefixLabel("Tiling", GUIStyle.none, catagoryStyle);
        TileSize = EditorGUILayout.IntSlider("TileSize", TileSize, 16, 1024);
        if (TileSize != OldTileSize && _geom != null)
        {
            BuildConfig();
            OldTileSize = TileSize;
        }
        EditorGUILayout.LabelField(
            string.Format("Tiles: {0} x {1}", TileWidth, TileHeight),
            string.Format("Max Tiles: {0} Max Polys: {1}", MaxTiles, MaxPolysPerTile));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(string.Format("Build Time: {0}ms", BuildTime));
        if (GUILayout.Button("Build"))
        {
            // create nav mesh object and build nav data (build all tiles)
            GameObject go = new GameObject("RecastNavMesh");
            go.AddComponent<RecastNavMesh>();
            RecastNavMesh navMesh = go.GetComponent<RecastNavMesh>();
            BuildTime = navMesh.BuildAllTiles(_config, _geom, TileWidth, TileHeight, MaxPolysPerTile, MaxTiles);
        }
        if (GUILayout.Button("Reset"))
        {
            ResetDefaults();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void BuildConfig()
    {
        _config = new Config()
        {
            CellSize = CellSize,
            CellHeight = CellHeight,
            WalkableSlopeAngle = AgentMaxSlope,
            WalkableHeight = (int) Math.Ceiling(AgentHeight/CellHeight),
            WalkableClimb = (int) Math.Floor(AgentMaxClimb/CellHeight),
            WalkableRadius = (int) Math.Ceiling(AgentRadius/CellSize),
            MaxEdgeLength = (int) (EdgeMaxLen/CellSize),
            MaxSimplificationError = EdgeMaxError,
            MinRegionArea = (int) (RegionMinSize*RegionMinSize),
            MergeRegionArea = (int) (RegionMergeSize*RegionMergeSize),
            MaxVertexesPerPoly = (int) VertsPerPoly,
            DetailSampleDistance = DetailSampleDist < 0.9 ? 0 : CellSize*DetailSampleDist,
            DetailSampleMaxError = CellHeight*DetailSampleMaxError,
            BorderSize = (int) Math.Ceiling(AgentRadius/CellSize) + 3,
            TileSize = TileSize,
        };

        _config.Width = _config.TileSize + _config.BorderSize*2;
        _config.Height = _config.TileSize + _config.BorderSize*2;

        _geom = new Geometry();
        BuildGeometry(_config, _geom);
        BuildTileSizeData();
    }

    public void BuildTileSizeData()
    {
        RecastVertex bmin = _geom.MinBounds;
        RecastVertex bmax = _geom.MaxBounds;

        int gw = 0, gh = 0;

        CalcGridSize(bmin, bmax, _config.CellSize, out gw, out gh);

        int ts = TileSize;
        int tw = (gw + ts - 1)/ts;
        int th = (gh + ts - 1)/ts;

        TileWidth = tw;
        TileHeight = th;

        int tileBits = Math.Min(ilog2(nextPow2(th*tw)), 14);
        if (tileBits > 14)
            tileBits = 14;

        int polyBits = 22 - tileBits;
        MaxTiles = 1 << tileBits;
        MaxPolysPerTile = 1 << polyBits;
    }

    private void CalcGridSize(RecastVertex bmin, RecastVertex bmax, float cellSize, out int w, out int h)
    {
        if (bmin != null && bmax != null)
        {
            w = (int) ((bmax.X - bmin.X)/cellSize + 0.5f);
            h = (int) ((bmax.Z - bmin.Z)/cellSize + 0.5f);
        }
        else
        {
            w = 0;
            h = 0;
        }
    }

    private int nextPow2(int v)
    {
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        v++;
        return v;
    }

    private int ilog2(int v)
    {
        int r;
        int shift;

        r = ((v > 0xffff) ? 1 : 0) << 4;
        v >>= r;
        shift = ((v > 0xff) ? 1 : 0) << 3;
        v >>= shift;
        r |= shift;
        shift = ((v > 0xf) ? 1 : 0) << 2;
        v >>= shift;
        r |= shift;
        shift = ((v > 0x3) ? 1 : 0) << 1;
        v >>= shift;
        r |= shift;
        r |= (v >> 1);
        return r;
    }

    /// <summary>
    /// This takes the current geometry and builds the data to go into Recast
    /// It needs to be updated to take into account scale, position, and rotation
    /// It needs to be updated to look for specific tags
    /// </summary>
    /// <param name="geom"></param>
    private void BuildGeometry(Config config, Geometry geom)
    {
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if ((Tags & (1 << i)) != 0)
            {
                foreach (var gameObject in GameObject.FindGameObjectsWithTag(UnityEditorInternal.InternalEditorUtility.tags[i]))
                {
                    foreach (var terrainObj in gameObject.GetComponentsInChildren<Terrain>())
                    {
                        var terrain = terrainObj.terrainData;
                        var w = terrain.heightmapWidth;
                        var h = terrain.heightmapHeight;
                        var meshScale = terrain.size;
                        var tRes = 1;
                        meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
                        var tData = terrain.GetHeights(0, 0, w, h);

                        w = (w - 1) / tRes + 1;
                        h = (h - 1) / tRes + 1;
                        var tVertices = new Vector3[w * h];
                        var tPolys = new int[(w - 1) * (h - 1) * 6];

                        // Build vertices and UVs
                        for (int y = 0; y < h; y++)
                        {
                            for (int x = 0; x < w; x++)
                            {
                                tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(x, tData[y * tRes, x * tRes], y)) + terrainObj.transform.position;
                            }
                        }

                        var index = 0;
                        // Build triangle indices: 3 indices into vertex array for each triangle
                        for (int y = 0; y < h - 1; y++)
                        {
                            for (int x = 0; x < w - 1; x++)
                            {
                                // For each grid cell output two triangles
                                tPolys[index++] = (y * w) + x;
                                tPolys[index++] = ((y + 1) * w) + x;
                                tPolys[index++] = (y * w) + x + 1;

                                tPolys[index++] = ((y + 1) * w) + x;
                                tPolys[index++] = ((y + 1) * w) + x + 1;
                                tPolys[index++] = (y * w) + x + 1;
                            }
                        }
                        int subTotalVerts = geom.NumVertexes;
                        foreach (var tVertex in tVertices)
                        {
                            geom.Vertexes.Add(new RecastVertex(tVertex.x, tVertex.y, tVertex.z));
                            geom.NumVertexes++;
                        }
                        for (int j = 0; j < tPolys.Length; j += 3)
                        {
                            geom.Triangles.Add(tPolys[j] + subTotalVerts);
                            geom.Triangles.Add(tPolys[j + 1] + subTotalVerts);
                            geom.Triangles.Add(tPolys[j + 2] + subTotalVerts);
                            geom.NumTriangles++;
                        }
                    }
                    foreach (var componentsInChild in gameObject.GetComponentsInChildren<MeshFilter>())
                    {
                        int subTotalVerts = geom.NumVertexes;
                        foreach (Vector3 vector3 in componentsInChild.sharedMesh.vertices)
                        {
                            Vector3 vec = gameObject.transform.TransformPoint(vector3);
                            geom.Vertexes.Add(new RecastVertex(vec.x, vec.y, vec.z));
                            geom.NumVertexes++;
                        }
                        for (int j = 0; j < componentsInChild.sharedMesh.triangles.Length; j += 3)
                        {
                            geom.Triangles.Add(componentsInChild.sharedMesh.triangles[j] + subTotalVerts);
                            geom.Triangles.Add(componentsInChild.sharedMesh.triangles[j + 1] + subTotalVerts);
                            geom.Triangles.Add(componentsInChild.sharedMesh.triangles[j + 2] + subTotalVerts);
                            geom.NumTriangles++;
                        }
                    }
                    foreach (var offMeshConnector in gameObject.GetComponentsInChildren<OffMeshConnector>())
                    {
                        RecastVertex start = new RecastVertex(offMeshConnector.StartPosition.x, offMeshConnector.StartPosition.y, offMeshConnector.StartPosition.z);
                        RecastVertex end = new RecastVertex(offMeshConnector.EndPosition.x, offMeshConnector.EndPosition.y, offMeshConnector.EndPosition.z);
                        geom.AddOffMeshConnection(start, end, offMeshConnector.Radius, offMeshConnector.Bidirectional, 5, 8);
                    }
                }
            }
        }
        Verts = geom.NumVertexes;
        Tris = geom.NumTriangles;

        if (Verts != 0)
        {
            geom.CalculateBounds();
            config.CalculateGridSize(geom);
            geom.CreateChunkyTriMesh();
        }
    }

    /// <summary>
    /// Resets the Editor values to the starting defaults
    /// </summary>
    public void ResetDefaults()
    {
        CellSize = 0.300000f;
        CellHeight = 0.2f;
        AgentHeight = 2.0f;
        AgentRadius = .6f;
        AgentMaxClimb = 0.9f;
        AgentMaxSlope = 45;
        RegionMinSize = 8;
        RegionMergeSize = 20;
        MonotonePartitioning = false;
        EdgeMaxLen = 12;
        EdgeMaxError = 1.3f;
        VertsPerPoly = 6;
        DetailSampleDist = 6;
        DetailSampleMaxError = 1;
    }

}