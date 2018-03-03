using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using LunaNav;
using Luna3D;


[Serializable]
public class LunaNavmeshQuery 
{
    public NavMeshQuery _navMeshQuery;
    public QueryFilter filter;
    public int SmoothPathNum = 0;
    public float[] SmoothPath = new float[2048 * 3];
    public int MaxSmooth = 2048;

    public bool doneSmoothing;

    public long startRef = 0, endRef = 0;
    public const int MaxPolys = 256;
    public long[] polys = new long[MaxPolys];
    public int polyCount = 0;
    public float[] nearestPt = new float[3];
    public float[] startPos;
    public float[] endPos;

    protected List<Vector3> PathVerts { get; set; }
    protected List<int> PathTriangles { get; set; }
    protected List<Vector2> PathUVs { get; set; }

    public Crowd Crowd { get; set; }

    private bool initializedCrowd = false;

    public void Initialize(string filePath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        MemoryStream ms = new MemoryStream();
        doc.Save(ms);

        byte[] data = ms.ToArray();
        MemoryStream navmeshMs = new MemoryStream(data);

        XmlSerializer xmlSerializer = new XmlSerializer(typeof(NavMeshSerializer));
        NavMeshSerializer navmeshSerializer = ((NavMeshSerializer)xmlSerializer.Deserialize(navmeshMs));
        LunaNav.NavMesh navmesh = navmeshSerializer.Reconstitute();
        InitializeQuery(navmesh);

        Crowd = new Crowd();

        Crowd.Init(50, 0.6f, _navMeshQuery.NavMesh);
        Crowd.Filter = filter;

        initializedCrowd = true;
    }


    public void Start()
    {
        initializedCrowd = true;
    }

    public void Update(float dt)
    {
        if (initializedCrowd)
        {
            CrowdAgentDebugInfo info = new CrowdAgentDebugInfo();
            Crowd.Update(dt, ref info);
        }
    }

    public void InitializeQuery(LunaNav.NavMesh navMesh)
    {
        _navMeshQuery = new NavMeshQuery();
        _navMeshQuery.Init(navMesh, 2048);
        filter = new QueryFilter();

        // These values need to me modifiable in the editor later using RecastArea
        filter.IncludeFlags = 15;
        filter.ExcludeFlags = 0;
        filter.SetAreaCost(1, 1.0f);
        filter.SetAreaCost(2, 10.0f);
        filter.SetAreaCost(3, 1.0f);
        filter.SetAreaCost(4, 1.0f);
        filter.SetAreaCost(5, 2);
        filter.SetAreaCost(6, 1.5f);
    }

    public void GeneratePath(Luna3D.Vector3 start, Luna3D.Vector3 end)
    {
        startPos = new float[] { start.x, start.y, start.z };
        endPos = new float[] { end.x, end.y, end.z };

        Status status = _navMeshQuery.FindNearestPoly(startPos, new[] { 2f, 4f, 2f }, filter, ref startRef, ref nearestPt);
        Console.WriteLine(string.Format("Found start position status: {0}, Ref {1}, pos {2}, {3}, {4} ", status, startRef, startPos[0], startPos[1], startPos[2]));


        status = _navMeshQuery.FindNearestPoly(endPos, new[] { 2f, 4f, 2f }, filter, ref endRef, ref nearestPt);
        Console.WriteLine(string.Format("Found end position status: {0}, Ref {1}, pos {2}, {3}, {4} ", status, endRef, endPos[0], endPos[1], endPos[2]));


        status = _navMeshQuery.FindPath(startRef, endRef, startPos, endPos, filter, ref polys, ref polyCount, MaxPolys);
    }

    public void SmoothGeneratedPath(Luna3D.Vector3 start, Luna3D.Vector3 end)
    {
        GeneratePath(start, end);

        long[] smoothPolys = new long[MaxPolys];
        Array.Copy(polys, smoothPolys, polyCount);
        int smoothPolyCount = polyCount;

        float[] iterPos = new float[3], targetPos = new float[3];

        _navMeshQuery.ClosestPointOnPoly(startRef, startPos, ref iterPos);
        _navMeshQuery.ClosestPointOnPoly(smoothPolys[smoothPolyCount - 1], endPos, ref targetPos);

        float StepSize = 0.5f;
        float Slop = 0.01f;

        SmoothPathNum = 0;
        Array.Copy(iterPos, 0, SmoothPath, SmoothPathNum * 3, 3);
        SmoothPathNum++;

        while (smoothPolyCount > 0 && SmoothPathNum < 2048)
        {

            float[] steerPos = new float[3];
            short steerPosFlag = 0;
            long steerPosRef = 0;

            if (!GetSteerTarget(_navMeshQuery, iterPos, targetPos, Slop, smoothPolys, smoothPolyCount, ref steerPos, ref steerPosFlag, ref steerPosRef))
                break;

            bool endOfPath = (steerPosFlag & StraightPathEnd) != 0;
            bool offMeshConnection = (steerPosFlag & StraightPathOffMeshConnection) != 0;

            float[] delta = Helper.VSub(steerPos[0], steerPos[1], steerPos[2], iterPos[0], iterPos[1], iterPos[2]);
            float len = (float)Math.Sqrt(Helper.VDot(delta, delta));

            if ((endOfPath || offMeshConnection) && len < StepSize)
                len = 1;
            else
            {
                len = StepSize / len;
            }

            float[] moveTarget = new float[3];
            Helper.VMad(ref moveTarget, iterPos, delta, len);

            float[] result = new float[3];
            long[] visited = new long[16];
            int nVisited = 0;

            _navMeshQuery.MoveAlongSurface(smoothPolys[0], iterPos, moveTarget, filter, ref result, ref visited, ref nVisited, 16);
            smoothPolyCount = FixupCorridor(ref smoothPolys, smoothPolyCount, MaxPolys, visited, nVisited);
            float h = 0;
            _navMeshQuery.GetPolyHeight(smoothPolys[0], result, ref h);
            result[1] = h;
            Array.Copy(result, iterPos, 3);

            if (endOfPath && InRange(iterPos, steerPos, Slop, 1.0f))
            {
                Array.Copy(targetPos, iterPos, 3);
                if (SmoothPathNum < 2048)
                {
                    Array.Copy(iterPos, 0, SmoothPath, SmoothPathNum * 3, 3);
                    SmoothPathNum++;
                }
                break;
            }
            else if (offMeshConnection && InRange(iterPos, steerPos, Slop, 1.0f))
            {
                float[] startPosOffMesh = new float[3], endPosOffMesh = new float[3];
                long prevRef = 0, polyRef = smoothPolys[0];
                int npos = 0;
                while (npos < smoothPolyCount && polyRef != steerPosRef)
                {
                    prevRef = polyRef;
                    polyRef = smoothPolys[npos];
                    npos++;
                }
                for (int i = npos; i < smoothPolyCount; i++)
                {
                    smoothPolys[i - npos] = smoothPolys[i];
                }
                smoothPolyCount -= npos;

                Status status = _navMeshQuery.NavMesh.GetOffMeshConnectionPolyEndPoints(prevRef, polyRef, ref startPosOffMesh, ref endPosOffMesh);
                if ((status & Status.Success) != 0)
                {
                    if (SmoothPathNum < MaxSmooth)
                    {
                        Array.Copy(startPosOffMesh, 0, SmoothPath, SmoothPathNum * 3, 3);
                        SmoothPathNum++;
                        if ((SmoothPathNum & 1) == 1)
                        {
                            Array.Copy(startPosOffMesh, 0, SmoothPath, SmoothPathNum * 3, 3);
                            SmoothPathNum++;
                        }
                    }
                    Array.Copy(endPosOffMesh, iterPos, 3);
                    float eh = 0.0f;
                    _navMeshQuery.GetPolyHeight(smoothPolys[0], iterPos, ref eh);
                    iterPos[1] = eh;
                }
            }

            if (SmoothPathNum < 2048)
            {
                Array.Copy(iterPos, 0, SmoothPath, SmoothPathNum * 3, 3);
                SmoothPathNum++;
            }
        }
        doneSmoothing = true;
    }

    private int FixupCorridor(ref long[] path, int npath, int maxPath, long[] visited, int nVisited)
    {
        int furthestPath = -1;
        int furthestVisited = -1;

        for (int i = npath - 1; i >= 0; --i)
        {
            bool found = false;
            for (int j = nVisited - 1; j >= 0; --j)
            {
                if (path[i] == visited[j])
                {
                    furthestPath = i;
                    furthestVisited = j;
                    found = true;
                }
            }
            if (found)
                break;
        }

        if (furthestPath == -1 || furthestVisited == -1)
            return npath;

        int req = nVisited - furthestVisited;
        int orig = Math.Min(furthestPath + 1, npath);
        int size = Math.Max(0, npath - orig);
        if (req + size > maxPath)
            size = maxPath - req;
        if (size > 0)
            Array.Copy(path, orig, path, req, size);

        for (int i = 0; i < req; i++)
        {
            path[i] = visited[(nVisited - 1) - i];
        }

        return req + size;
    }

    private bool InRange(float[] v1, float[] v2, float r, float h)
    {
        return InRange(v1[0], v1[1], v1[2], v2, r, h);
    }

    private bool InRange(float v1x, float v1y, float v1z, float[] v2, float r, float h)
    {
        float dx = v2[0] - v1x;
        float dy = v2[1] - v1y;
        float dz = v2[2] - v1z;
        return (dx * dx + dz * dz) < r * r && Math.Abs(dy) < h;
    }

    protected int StraightPathOffMeshConnection
    {
        get { return 4; }
    }

    protected int StraightPathEnd
    {
        get { return 2; }
    }

    private bool GetSteerTarget(NavMeshQuery navMeshQuery, float[] startPos, float[] endPos, float minTargetDistance,
        long[] path, int pathSize, ref float[] steerPos, ref short steerPosFlag, ref long steerPosRef)
    {
        float[] outPoints = null;
        int outPointsCount = 0;
        return GetSteerTarget(navMeshQuery, startPos, endPos, minTargetDistance, path, pathSize, ref steerPos, ref steerPosFlag, ref steerPosRef, ref outPoints, ref outPointsCount);
    }

    private bool GetSteerTarget(NavMeshQuery navMeshQuery, float[] startPos, float[] endPos, float minTargetDistance, 
        long[] path, int pathSize, ref float[] steerPos, ref short steerPosFlag, ref long steerPosRef, ref float[] outPoints, ref int outPointCount)
    {
        int MaxSteerPoints = 3;
        float[] steerPath = new float[MaxSteerPoints * 3];
        short[] steerPathFlags = new short[MaxSteerPoints];
        long[] steerPathPolys = new long[MaxSteerPoints];

        int nSteerPath = 0;

        navMeshQuery.FindStraightPath(startPos, endPos, path, pathSize, ref steerPath, ref steerPathFlags, ref steerPathPolys, ref nSteerPath, MaxSteerPoints);

        if (nSteerPath == 0)
            return false;

        if (outPoints != null && outPointCount > 0)
        {
            outPointCount = nSteerPath;
            for (int i = 0; i < nSteerPath; i++)
            {
                Array.Copy(steerPath, i * 3, outPoints, i * 3, 3);
            }
        }

        int ns = 0;
        while (ns < nSteerPath)
        {
            if ((steerPathFlags[ns] & StraightPathOffMeshConnection) != 0 ||
                !InRange(steerPath[ns * 3 + 0], steerPath[ns * 3 + 1], steerPath[ns * 3 + 2], startPos, minTargetDistance, 1000.0f))
                break;
            ns++;
        }

        if (ns >= nSteerPath)
            return false;

        Array.Copy(steerPath, ns * 3, steerPos, 0, 3);
        steerPos[1] = startPos[1];
        steerPosFlag = steerPathFlags[ns];
        steerPosRef = steerPathPolys[ns];

        return true;
    }   
}
