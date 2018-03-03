//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
//using System.Xml.Serialization;
//using Recast.Json;
//using Recast.Json.Bson;
//using UnityEngine;
//using LunaNav;

//[AddComponentMenu("Recast/NavMesh Query")]
//[Serializable]
//public class RecastNavMeshQuery : MonoBehaviour
//{
//    public NavMeshQuery _navMeshQuery;
//    public QueryFilter filter;
//    public int SmoothPathNum = 0;
//    public float[] SmoothPath = new float[2048 * 3];
//    public int MaxSmooth = 2048;

//    public bool doneSmoothing;

//    public GameObject StartPosition;
//    public GameObject EndPosition;
//    public GameObject RecastNavMesh;

//    public long startRef = 0, endRef = 0;
//    public const int MaxPolys = 256;
//    public long[] polys = new long[MaxPolys];
//    public int polyCount = 0;
//    public float[] nearestPt = new float[3];
//    public float[] startPos;
//    public float[] endPos;

//    public string FilePath;

//    protected List<Vector3> PathVerts { get; set; }
//    protected List<int> PathTriangles { get; set; }
//    protected List<Color> PathColors { get; set; }
//    protected List<Vector2> PathUVs { get; set; }

//    private Mesh mesh;
//    private MeshFilter meshFilter;
//    private MeshRenderer meshRenderer;
//    public Material mat;
//    public Crowd Crowd { get; set; }
//    public List<DetourActor> Actors { get; set; }

//    private bool initializedCrowd = false;
    
//    public void Awake()
//    {
//        Initialize();
//    }

//    public void Initialize()
//    {
//#if UNITY_WEBPLAYER
//            var asset = Resources.Load(FilePath, typeof(TextAsset)) as TextAsset;
//            MemoryStream f = new MemoryStream(asset.bytes);
//            XmlSerializer xmlSerializer = new XmlSerializer(typeof(NavMeshSerializer));
//            InitializeQuery(((NavMeshSerializer)xmlSerializer.Deserialize(f)).Reconstitute());    
//#else
//        if (Path.GetExtension(FilePath).Equals(""))
//        {
//            var asset = Resources.Load(FilePath, typeof(TextAsset)) as TextAsset;
//            MemoryStream f = new MemoryStream(asset.bytes);
//            XmlSerializer xmlSerializer = new XmlSerializer(typeof(NavMeshSerializer));
//            InitializeQuery(((NavMeshSerializer)xmlSerializer.Deserialize(f)).Reconstitute());
//        }
//        else
//        {
//            FileStream f = null;
//            f = File.OpenRead(FilePath);
//            if (FilePath.LastIndexOf(".xml", StringComparison.OrdinalIgnoreCase) >= 0)
//            {
//                XmlSerializer xmlSerializer = new XmlSerializer(typeof(NavMeshSerializer));
//                InitializeQuery(((NavMeshSerializer)xmlSerializer.Deserialize(f)).Reconstitute());
//            }
//            else if (FilePath.LastIndexOf(".json", StringComparison.OrdinalIgnoreCase) >= 0)
//            {
//                StreamReader sr = new StreamReader(f);
//                JsonReader reader = new JsonTextReader(sr);
//                JsonSerializer serializer = new JsonSerializer();
//                InitializeQuery(serializer.Deserialize<NavMeshSerializer>(reader).Reconstitute());
//            }
//            else if (FilePath.LastIndexOf(".bytes", StringComparison.OrdinalIgnoreCase) >= 0)
//            {
//                XmlSerializer xmlSerializer = new XmlSerializer(typeof(NavMeshSerializer));
//                InitializeQuery(((NavMeshSerializer)xmlSerializer.Deserialize(f)).Reconstitute());
//            }
//            else
//            {
//                BsonReader reader = new BsonReader(f);
//                JsonSerializer serializer = new JsonSerializer();
//                InitializeQuery(serializer.Deserialize<NavMeshSerializer>(reader).Reconstitute());
//            }
//        }
//#endif

//        Actors = new List<DetourActor>();
//        foreach (DetourActor detourActor in GameObject.FindObjectsOfType(typeof(DetourActor)))
//        {
//            Actors.Add(detourActor);
//        }
//        Crowd = new Crowd();

//        Crowd.Init(Actors.Count, 0.6f, _navMeshQuery.NavMesh);
//        Crowd.Filter = filter;
//    }

//    public void OnDrawGizmosSelected()
//    {
//#if UNITY_EDITOR
//        if (doneSmoothing)
//        {
//            Vector3 prev = new Vector3(SmoothPath[0], SmoothPath[1], SmoothPath[2]);
//            for (int i = 1; i < SmoothPathNum; i++)
//            {
//                Gizmos.DrawLine(prev, new Vector3(SmoothPath[i * 3 + 0], SmoothPath[i * 3 + 1] + 0.1f, SmoothPath[i * 3 + 2]));
//                prev = new Vector3(SmoothPath[i * 3 + 0], SmoothPath[i * 3 + 1], SmoothPath[i * 3 + 2]);
//            }
//        }
//#endif

//    }

//    public void Start()
//    {
//        initializedCrowd = true;
//    }

//    /// <summary>
//    /// Called once per frame - If a straight path has been found, it draws a debug line detailing the path
//    /// </summary>
//    public void Update()
//    {
//        if (initializedCrowd)
//        {
//            CrowdAgentDebugInfo info = new CrowdAgentDebugInfo();
//            Crowd.Update(Time.deltaTime, ref info);
//        }
//    }

//    /// <summary>
//    /// Builds the initial query, normally called by awake
//    /// Can be callen manually with a different nav mesh.
//    /// </summary>
//    /// <param name="navMesh"></param>
//    public void InitializeQuery(LunaNav.NavMesh navMesh)
//    {
//        _navMeshQuery = new NavMeshQuery();
//        _navMeshQuery.Init(navMesh, 2048);
//        filter = new QueryFilter();

//        // These values need to me modifiable in the editor later using RecastArea
//        filter.IncludeFlags = 15;
//        filter.ExcludeFlags = 0;
//        filter.SetAreaCost(1, 1.0f);
//        filter.SetAreaCost(2, 10.0f);
//        filter.SetAreaCost(3, 1.0f);
//        filter.SetAreaCost(4, 1.0f);
//        filter.SetAreaCost(5, 2);
//        filter.SetAreaCost(6, 1.5f);
//    }

//    /// <summary>
//    /// Builds a blocky path following the middle of the connecting edges across the nav mesh.
//    /// </summary>
//    public void GeneratePath()
//    {
//        startPos = new float[] { StartPosition.transform.position.x, StartPosition.transform.position.y, StartPosition.transform.position.z };
//        endPos = new float[]{ EndPosition.transform.position.x, EndPosition.transform.position.y, EndPosition.transform.position.z };
//        Status status = _navMeshQuery.FindNearestPoly(startPos, new[] { 2f, 4f, 2f }, filter, ref startRef, ref nearestPt);
//        Debug.Log(string.Format("Found start position status: {0}, Ref {1}, pos {2}, {3}, {4} ", status, startRef, startPos[0], startPos[1], startPos[2]));
//        status = _navMeshQuery.FindNearestPoly(endPos, new[] { 2f, 4f, 2f }, filter, ref endRef, ref nearestPt);
//        Debug.Log(string.Format("Found end position status: {0}, Ref {1}, pos {2}, {3}, {4} ", status, endRef, endPos[0], endPos[1], endPos[2]));
//        status = _navMeshQuery.FindPath(startRef, endRef, startPos, endPos, filter, ref polys, ref polyCount, MaxPolys);
//    }

//    #region Smooth Path Generation
//    /// <summary>
//    /// Calls GeneratePath. Builds a smooth path by going to corners and trying to find the straightest paths possible across
//    /// the NavMesh from the StartPosition to the EndPosition
//    /// </summary>
//    public void SmoothGeneratedPath()
//    {
//        GeneratePath();
//        long[] smoothPolys = new long[MaxPolys];
//        Array.Copy(polys, smoothPolys, polyCount);
//        int smoothPolyCount = polyCount;

//        float[] iterPos = new float[3], targetPos = new float[3];

//        _navMeshQuery.ClosestPointOnPoly(startRef, startPos, ref iterPos);
//        _navMeshQuery.ClosestPointOnPoly(smoothPolys[smoothPolyCount - 1], endPos, ref targetPos);

//        float StepSize = 0.5f;
//        float Slop = 0.01f;

//        SmoothPathNum = 0;
//        Array.Copy(iterPos, 0, SmoothPath, SmoothPathNum * 3, 3);
//        SmoothPathNum++;

//        while (smoothPolyCount > 0 && SmoothPathNum < 2048)
//        {

//            float[] steerPos = new float[3];
//            short steerPosFlag = 0;
//            long steerPosRef = 0;

//            if (!GetSteerTarget(_navMeshQuery, iterPos, targetPos, Slop, smoothPolys, smoothPolyCount, ref steerPos, ref steerPosFlag, ref steerPosRef))
//                break;

//            bool endOfPath = (steerPosFlag & StraightPathEnd) != 0;
//            bool offMeshConnection = (steerPosFlag & StraightPathOffMeshConnection) != 0;

//            float[] delta = Helper.VSub(steerPos[0], steerPos[1], steerPos[2], iterPos[0], iterPos[1], iterPos[2]);
//            float len = (float)Math.Sqrt(Helper.VDot(delta, delta));

//            if ((endOfPath || offMeshConnection) && len < StepSize)
//                len = 1;
//            else
//            {
//                len = StepSize / len;
//            }

//            float[] moveTarget = new float[3];
//            Helper.VMad(ref moveTarget, iterPos, delta, len);

//            float[] result = new float[3];
//            long[] visited = new long[16];
//            int nVisited = 0;

//            _navMeshQuery.MoveAlongSurface(smoothPolys[0], iterPos, moveTarget, filter, ref result, ref visited, ref nVisited, 16);
//            smoothPolyCount = FixupCorridor(ref smoothPolys, smoothPolyCount, MaxPolys, visited, nVisited);
//            float h = 0;
//            _navMeshQuery.GetPolyHeight(smoothPolys[0], result, ref h);
//            result[1] = h;
//            Array.Copy(result, iterPos, 3);

//            if (endOfPath && InRange(iterPos, steerPos, Slop, 1.0f))
//            {
//                Array.Copy(targetPos, iterPos, 3);
//                if (SmoothPathNum < 2048)
//                {
//                    Array.Copy(iterPos, 0, SmoothPath, SmoothPathNum * 3, 3);
//                    SmoothPathNum++;
//                }
//                break;
//            }
//            else if (offMeshConnection && InRange(iterPos, steerPos, Slop, 1.0f))
//            {
//                float[] startPosOffMesh = new float[3], endPosOffMesh = new float[3];
//                long prevRef = 0, polyRef = smoothPolys[0];
//                int npos = 0;
//                while (npos < smoothPolyCount && polyRef != steerPosRef)
//                {
//                    prevRef = polyRef;
//                    polyRef = smoothPolys[npos];
//                    npos++;
//                }
//                for (int i = npos; i < smoothPolyCount; i++)
//                {
//                    smoothPolys[i - npos] = smoothPolys[i];
//                }
//                smoothPolyCount -= npos;

//                Status status = _navMeshQuery.NavMesh.GetOffMeshConnectionPolyEndPoints(prevRef, polyRef, ref startPosOffMesh, ref endPosOffMesh);
//                if ((status & Status.Success) != 0)
//                {
//                    if (SmoothPathNum < MaxSmooth)
//                    {
//                        Array.Copy(startPosOffMesh, 0, SmoothPath, SmoothPathNum*3, 3);
//                        SmoothPathNum++;
//                        if ((SmoothPathNum & 1) == 1)
//                        {
//                            Array.Copy(startPosOffMesh, 0, SmoothPath, SmoothPathNum*3, 3);
//                            SmoothPathNum++;
//                        }
//                    }
//                    Array.Copy(endPosOffMesh, iterPos, 3);
//                    float eh = 0.0f;
//                    _navMeshQuery.GetPolyHeight(smoothPolys[0], iterPos, ref eh);
//                    iterPos[1] = eh;
//                }
//            }

//            if (SmoothPathNum < 2048)
//            {
//                Array.Copy(iterPos, 0, SmoothPath, SmoothPathNum * 3, 3);
//                SmoothPathNum++;
//            }
//        }
//        doneSmoothing = true;
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="path"></param>
//    /// <param name="npath"></param>
//    /// <param name="maxPath"></param>
//    /// <param name="visited"></param>
//    /// <param name="nVisited"></param>
//    /// <returns></returns>
//    private int FixupCorridor(ref long[] path, int npath, int maxPath, long[] visited, int nVisited)
//    {
//        int furthestPath = -1;
//        int furthestVisited = -1;

//        for (int i = npath - 1; i >= 0; --i)
//        {
//            bool found = false;
//            for (int j = nVisited - 1; j >= 0; --j)
//            {
//                if (path[i] == visited[j])
//                {
//                    furthestPath = i;
//                    furthestVisited = j;
//                    found = true;
//                }
//            }
//            if (found)
//                break;
//        }

//        if (furthestPath == -1 || furthestVisited == -1)
//            return npath;

//        int req = nVisited - furthestVisited;
//        int orig = Math.Min(furthestPath + 1, npath);
//        int size = Math.Max(0, npath - orig);
//        if (req + size > maxPath)
//            size = maxPath - req;
//        if (size > 0)
//            Array.Copy(path, orig, path, req, size);

//        for (int i = 0; i < req; i++)
//        {
//            path[i] = visited[(nVisited - 1) - i];
//        }

//        return req + size;
//    }

//    /// <summary>
//    /// Helper class for using an array instead of splitting out v1
//    /// </summary>
//    /// <param name="v1">array of 3 points for vector 1</param>
//    /// <param name="v2">array of 3 points for vector 2</param>
//    /// <param name="r">radius around which to search</param>
//    /// <param name="h">height above or below to check for</param>
//    /// <returns></returns>
//    private bool InRange(float[] v1, float[] v2, float r, float h)
//    {
//        return InRange(v1[0], v1[1], v1[2], v2, r, h);
//    }

//    /// <summary>
//    /// Checking if V1 is in range of V2
//    /// </summary>
//    /// <param name="v1x">V1 x-component</param>
//    /// <param name="v1y">V1 y-component</param>
//    /// <param name="v1z">V1 z-component</param>
//    /// <param name="v2">Vector to check with</param>
//    /// <param name="r">radius around v1 to check</param>
//    /// <param name="h">height above and below v1 to check</param>
//    /// <returns></returns>
//    private bool InRange(float v1x, float v1y, float v1z, float[] v2, float r, float h)
//    {
//        float dx = v2[0] - v1x;
//        float dy = v2[1] - v1y;
//        float dz = v2[2] - v1z;
//        return (dx * dx + dz * dz) < r * r && Math.Abs(dy) < h;
//    }

//    /// <summary>
//    /// Static value to check if a connection is offmesh or not.
//    /// </summary>
//    protected int StraightPathOffMeshConnection
//    {
//        get { return 4; }
//    }

//    /// <summary>
//    /// Static to check if the straight path is at the end
//    /// </summary>
//    protected int StraightPathEnd
//    {
//        get { return 2; }
//    }

//    /// <summary>
//    /// Tries to find the straightest path between 2 polygons
//    /// </summary>
//    /// <param name="navMeshQuery"></param>
//    /// <param name="startPos"></param>
//    /// <param name="endPos"></param>
//    /// <param name="minTargetDistance"></param>
//    /// <param name="path"></param>
//    /// <param name="pathSize"></param>
//    /// <param name="steerPos"></param>
//    /// <param name="steerPosFlag"></param>
//    /// <param name="steerPosRef"></param>
//    /// <param name="outPoints"></param>
//    /// <param name="outPointCount"></param>
//    /// <returns></returns>
//    private bool GetSteerTarget(NavMeshQuery navMeshQuery, float[] startPos, float[] endPos, float minTargetDistance,
//                                long[] path, int pathSize, ref float[] steerPos, ref short steerPosFlag,
//                                ref long steerPosRef)
//    {
//        float[] outPoints = null;
//        int outPointsCount = 0;
//        return GetSteerTarget(navMeshQuery, startPos, endPos, minTargetDistance,
//                              path, pathSize, ref steerPos, ref steerPosFlag,
//                              ref steerPosRef, ref outPoints, ref outPointsCount);
//    }

//    private bool GetSteerTarget(NavMeshQuery navMeshQuery, float[] startPos, float[] endPos, float minTargetDistance, long[] path, int pathSize, ref float[] steerPos, ref short steerPosFlag, ref long steerPosRef, ref float[] outPoints, ref int outPointCount)
//    {
//        int MaxSteerPoints = 3;
//        float[] steerPath = new float[MaxSteerPoints * 3];
//        short[] steerPathFlags = new short[MaxSteerPoints];
//        long[] steerPathPolys = new long[MaxSteerPoints];

//        int nSteerPath = 0;

//        navMeshQuery.FindStraightPath(startPos, endPos, path, pathSize, ref steerPath, ref steerPathFlags,
//                                      ref steerPathPolys, ref nSteerPath, MaxSteerPoints);

//        if (nSteerPath == 0)
//            return false;

//        if (outPoints != null && outPointCount > 0)
//        {
//            outPointCount = nSteerPath;
//            for (int i = 0; i < nSteerPath; i++)
//            {
//                Array.Copy(steerPath, i * 3, outPoints, i * 3, 3);
//            }
//        }

//        int ns = 0;
//        while (ns < nSteerPath)
//        {
//            if ((steerPathFlags[ns] & StraightPathOffMeshConnection) != 0 ||
//                !InRange(steerPath[ns * 3 + 0], steerPath[ns * 3 + 1], steerPath[ns * 3 + 2], startPos, minTargetDistance, 1000.0f))
//                break;
//            ns++;
//        }

//        if (ns >= nSteerPath)
//            return false;

//        Array.Copy(steerPath, ns * 3, steerPos, 0, 3);
//        steerPos[1] = startPos[1];
//        steerPosFlag = steerPathFlags[ns];
//        steerPosRef = steerPathPolys[ns];

//        return true;
//    }

//    #endregion

//    #region Draw the Path of polygons

//    /// <summary>
//    /// Builds the Path Geometry from start to end as Unity MeshData
//    /// </summary>
//    /// <param name="startRef">polygon ID to color red</param>
//    /// <param name="endRef">polygon ID to color green</param>
//    /// <param name="polys">List of polygon IDs to follow across the path</param>
//    /// <param name="npolys">Number of polys actually used in the list</param>
//    private void BuildPathGeometry(long startRef, long endRef, long[] polys, int npolys)
//    {
//        if (_navMeshQuery.NavMesh != null)
//        {
//            PathVerts = new List<Vector3>();
//            PathColors = new List<Color>();
//            PathUVs = new List<Vector2>();
//            PathTriangles = new List<int>();

//            BuildNavMeshPoly(_navMeshQuery.NavMesh, startRef, duRGBA(128, 25, 0, 192), PathVerts, PathColors, PathUVs, PathTriangles);
//            BuildNavMeshPoly(_navMeshQuery.NavMesh, endRef, duRGBA(51, 102, 0, 192), PathVerts, PathColors, PathUVs, PathTriangles);

//            if (npolys > 0)
//            {
//                for (int i = 0; i < npolys; i++)
//                {
//                    if (polys[i] == startRef || polys[i] == endRef)
//                        continue;
//                    BuildNavMeshPoly(_navMeshQuery.NavMesh, polys[i], duRGBA(0, 0, 0, 64), PathVerts, PathColors, PathUVs, PathTriangles);
//                }
//            }
//        }
//    }

//    /// <summary>
//    /// Builds a single polygon from the nav mesh
//    /// </summary>
//    /// <param name="navmesh">the entire navmesh which contains all polygon data</param>
//    /// <param name="refId">id of the polygon that is to be drawn</param>
//    /// <param name="color">Color to make the polygon</param>
//    /// <param name="verts">List of Verts to add to</param>
//    /// <param name="colors">List of colors for specific polygons</param>
//    /// <param name="uvs">List of UVs for the polygons</param>
//    /// <param name="tris">List of Triangles for the polygons</param>
//    private void BuildNavMeshPoly(LunaNav.NavMesh navmesh, long refId, Color color, List<Vector3> verts, List<Color> colors, List<Vector2> uvs, List<int> tris)
//    {
//        MeshTile tile = null;
//        Poly poly = null;
//        if ((navmesh.GetTileAndPolyByRef(refId, ref tile, ref poly) & Status.Failure) != 0)
//            return;

//        long ip = 0;
//        for (int i = 0; i < tile.Polys.Length; i++)
//        {
//            if (poly == tile.Polys[i])
//                ip = i;
//        }
//        if (poly.Type == LunaNav.NavMeshBuilder.PolyTypeOffMeshConnection)
//        {
//            // do nothing for now
//        }
//        else
//        {
//            PolyDetail pd = tile.DetailMeshes[ip];
//            for (int i = 0; i < pd.TriCount; i++)
//            {
//                int t = ((int)pd.TriBase + i) * 4;
//                for (int j = 0; j < 3; j++)
//                {
//                    if (tile.DetailTris[t + j] < poly.VertCount)
//                    {
//                        verts.Add(new Vector3(tile.Verts[poly.Verts[tile.DetailTris[t + j]] * 3 + 0], tile.Verts[poly.Verts[tile.DetailTris[t + j]] * 3 + 1], tile.Verts[poly.Verts[tile.DetailTris[t + j]] * 3 + 2]));
//                    }
//                    else
//                    {
//                        verts.Add(
//                            new Vector3(tile.DetailVerts[(pd.VertBase + tile.DetailTris[t + j] - poly.VertCount) * 3 + 0],
//                                        tile.DetailVerts[(pd.VertBase + tile.DetailTris[t + j] - poly.VertCount) * 3 + 1],
//                                        tile.DetailVerts[(pd.VertBase + tile.DetailTris[t + j] - poly.VertCount) * 3 + 2]));
//                    }
//                    uvs.Add(new Vector2());
//                    colors.Add(color);
//                    tris.Add(tris.Count);
//                }
//            }
//        }

//    }

//    /// <summary>
//    /// Creates a Unity Color from 4 integers
//    /// </summary>
//    /// <param name="r">red component</param>
//    /// <param name="g">green component</param>
//    /// <param name="b">blue component</param>
//    /// <param name="a">alpha channel</param>
//    /// <returns></returns>
//    Color duRGBA(int r, int g, int b, int a)
//    {
//        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
//    }

//    /// <summary>
//    /// Draw the Polys for the path from startref to endref
//    /// </summary>
//    public void DrawPathGeometry()
//    {
//        if (mesh == null)
//        {
//            mesh = new Mesh();
//        }
//        mesh.Clear();
//        BuildPathGeometry(startRef, endRef, polys, polyCount);
//        if(GetComponent<MeshFilter>() == null)
//            gameObject.AddComponent<MeshFilter>();
//        if (GetComponent<MeshRenderer>() == null)
//            gameObject.AddComponent<MeshRenderer>();
//        mesh.vertices = PathVerts.ToArray();
//        mesh.triangles = PathTriangles.ToArray();
//        mesh.colors = PathColors.ToArray();
//        mesh.uv = PathUVs.ToArray();
//        mesh.RecalculateNormals();
//        meshFilter = gameObject.GetComponent<MeshFilter>();
//        meshFilter.mesh = mesh;
//        gameObject.GetComponent<MeshRenderer>().material = mat;
//    }

//    #endregion
//}
