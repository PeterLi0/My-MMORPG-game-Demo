using System;



namespace LunaNav
{
    public class PathCorridor
    {
        private float[] _pos = new float[3];
        private float[] _target = new float[3];

        private long[] _path;
        private int _npath;
        private int _maxPath;

        public PathCorridor()
        {
            _path = null;
            _npath = 0;
            _maxPath = 0;
        }

        public bool Init(int maxPath)
        {
            if(_path != null)
                throw new Exception("Path already exists, reset before initializing");

            _path = new long[maxPath];
            _npath = 0;
            _maxPath = maxPath;
            return true;
        }

        public void Reset(long refId, float[] pos)
        {
            Helper.VCopy(ref _pos, pos);
            Helper.VCopy(ref _target, pos);
            _path[0] = refId;
            _npath = 1;
        }

        public int FindCorners(ref float[] cornerVerts, ref short[] cornerFlags, ref long[] cornerPolys, int maxCorners,
                               NavMeshQuery navQuery, QueryFilter filter)
        {
            if(_path == null || _npath == 0)
                throw new Exception("Corridor must be initialised first");

            float MinTargetDist = 0.01f;

            int ncorners = 0;

            navQuery.FindStraightPath(_pos, _target, _path, _npath, ref cornerVerts, ref cornerFlags, ref cornerPolys,
                                      ref ncorners, maxCorners);

            while (ncorners > 0)
            {
                if ((cornerFlags[0] & NavMeshQuery.StraightPathOffMeshConnection) != 0 ||
                    Helper.VDist2DSqr(cornerVerts[0], cornerVerts[1], cornerVerts[2], _pos[0], _pos[1], _pos[2]) > MinTargetDist*MinTargetDist)
                    break;
                ncorners--;
                if (ncorners > 0)
                {
                    Array.Copy(cornerFlags, 1, cornerFlags, 0, ncorners);
                    Array.Copy(cornerPolys, 1, cornerPolys, 0, ncorners);
                    Array.Copy(cornerVerts, 3, cornerVerts, 0, ncorners*3);
                }
            }

            for (int i = 0; i < ncorners; i++)
            {
                if ((cornerFlags[i] & NavMeshQuery.StraightPathOffMeshConnection) != 0)
                {
                    ncorners = i + 1;
                    break;
                }
            }
            return ncorners;
        }

        public void OptimizePathVisibility(float[] next, float pathOptimizationRange, NavMeshQuery navQuery,
                                           QueryFilter filter)
        {
            if(_path == null)
                throw new Exception("Corridor must be initialised first");

            float[] goal = new float[3];
            Helper.VCopy(ref goal, next);
            float dist = Helper.VDist2D(_pos, goal);

            if (dist < 0.01f)
                return;

            dist = Math.Min(dist + 0.01f, pathOptimizationRange);

            float[] delta = Helper.VSub(goal[0], goal[1], goal[2], _pos[0], _pos[1], _pos[2]);
            Helper.VMad(ref goal, _pos, delta, pathOptimizationRange/dist);

            int MaxRes = 32;
            long[] res = new long[MaxRes];
            float t = 0;
            float[] norm = new float[3];
            int nres = 0;
            navQuery.Raycast(_path[0], _pos, goal, filter, ref t, ref norm, ref res, ref nres, MaxRes);
            if (nres > 1 && t > 0.99f)
            {
                _npath = MergeCorridorStartShortcut(ref _path, _npath, _maxPath, res, nres);
            }
        }

        public bool OptimizePathTopology(NavMeshQuery navQuery, QueryFilter filter)
        {
            if (_npath < 3)
                return false;

            int MaxIters = 32;
            int MaxRes = 32;

            long[] res = new long[MaxRes];
            int nres = 0;

            navQuery.InitSlicedFindPath(_path[0], _path[_npath - 1], _pos, _target, filter);
            int doneIters = 0;
            navQuery.UpdateSlicedFindPath(MaxIters, ref doneIters);

            Status status = navQuery.FinalizeSlicedFindPathPartial(_path, _npath, ref res, ref nres, MaxRes);

            if ((status & Status.Success) != 0 && nres > 0)
            {
                _npath = MergeCorridorStartShortcut(ref _path, _npath, _maxPath, res, nres);
                return true;
            }

            return false;
        }

        public bool MoveOverOffmeshConnection(long offMeshConRef, ref long[] refs, ref float[] startPos, ref float[] endPos,
                                              NavMeshQuery navQuery)
        {
            long prefRef = 0, polyRef = _path[0];
            int npos = 0;
            while (npos < _npath && polyRef != offMeshConRef)
            {
                prefRef = polyRef;
                polyRef = _path[npos];
                npos++;
            }
            if (npos == _npath)
                return false;

            for (int i = npos; i < _npath; i++)
            {
                _path[i - npos] = _path[i];
            }

            _npath -= npos;

            refs[0] = prefRef;
            refs[1] = polyRef;

            NavMesh nav = navQuery.NavMesh;

            Status status = nav.GetOffMeshConnectionPolyEndPoints(refs[0], refs[1], ref startPos, ref endPos);
            if ((status & Status.Success) != 0)
            {
                Helper.VCopy(ref _pos, endPos);
                return true;
            }

            return false;
        }

        public bool FixPathStart(long safeRef, float[] safePos)
        {
            Helper.VCopy(ref _pos, safePos);
            if (_npath < 3 && _npath > 0)
            {
                _path[2] = _path[_npath - 1];
                _path[0] = safeRef;
                _path[1] = 0;
                _npath = 3;
            }
            else
            {
                _path[0] = safeRef;
                _path[1] = 0;
            }

            return true;
        }

        public bool TrimInvalidPath(long safeRef, float[] safePos, NavMeshQuery navQuery, QueryFilter filter)
        {
            int n = 0;
            while (n < _npath && navQuery.IsValidPolyRef(_path[n], filter))
            {
                n++;
            }

            if (n == _npath)
                return true;
            else if (n == 0)
            {
                Helper.VCopy(ref _pos, safePos);
                _path[0] = safeRef;
                _npath = 1;
            }
            else
            {
                _npath = n;
            }

            float[] tgt = new float[3];
            Helper.VCopy(ref tgt, _target);
            navQuery.ClosestPointOnPolyBoundary(_path[_npath - 1], tgt, ref _target);

            return true;
        }

        public bool IsValid(int maxLookAhead, NavMeshQuery navQuery, QueryFilter filter)
        {
            int n = Math.Min(_npath, maxLookAhead);
            for (int i = 0; i < n; i++)
            {
                if (!navQuery.IsValidPolyRef(_path[i], filter))
                    return false;
            }
            return true;
        }

        public void MovePosition(float[] npos, NavMeshQuery navQuery, QueryFilter filter)
        {
            float[] result = new float[3];
            int MaxVisited = 16;
            long[] visited = new long[MaxVisited];
            int nvisited = 0;

            navQuery.MoveAlongSurface(_path[0], _pos, npos, filter, ref result, ref visited, ref nvisited, MaxVisited);
            _npath = MergeCorridorStartMoved(ref _path, _npath, _maxPath, visited, nvisited);

            float h = _pos[1];
            navQuery.GetPolyHeight(_path[0], result, ref h);
            result[1] = h;
            Helper.VCopy(ref _pos, result);
        }

        public void MoveTargetPosition(float[] npos, NavMeshQuery navQuery, QueryFilter filter)
        {
            float[] result = new float[3];
            int MaxVisited = 16;
            long[] visited = new long[MaxVisited];
            int nvisited = 0;
            navQuery.MoveAlongSurface(_path[_npath - 1], _target, npos, filter, ref result, ref visited, ref nvisited,
                                      MaxVisited);
            _npath = MergeCorridorEndMoved(ref _path, _npath, _maxPath, visited, nvisited);

            Helper.VCopy(ref _target, result);
        }

        public void SetCorridor(float[] target, long[] path, int npath)
        {
            Helper.VCopy(ref _target, target);
            Array.Copy(path, _path, npath);
            _npath = npath;
        }

        public float[] Pos
        {
            get { return _pos; }
        }

        public float[] Target
        {
            get { return _target; }
        }

        public long FirstPoly
        {
            get { return _npath > 0 ? _path[0] : 0; }
        }

        public long LastPoly
        {
            get { return _npath > 0 ? _path[_npath - 1] : 0; }
        }

        public long[] GetPath
        {
            get { return _path; }
        }

        public int PathCount
        {
            get { return _npath; }
        }

        public static int MergeCorridorStartMoved(ref long[] path, int npath, int maxPath, long[] visited, int nvisited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            for (int i = npath-1; i >= 0; i--)
            {
                bool found = false;
                for (int j = nvisited-1; j >= 0; j--)
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
            {
                return npath;
            }

            int req = nvisited - furthestVisited;
            int orig = Math.Min(furthestPath + 1, npath);
            int size = Math.Max(0, npath - orig);
            if (req + size > maxPath)
                size = maxPath - req;
            if(size >= 0)
                Array.Copy(path, orig, path, req, size);

            for (int i = 0; i < req; i++)
            {
                path[i] = visited[(nvisited - 1) - i];
            }

            return req + size;
        }

        public static int MergeCorridorEndMoved(ref long[] path, int npath, int maxPath, long[] visited, int nvisited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            for (int i = 0; i < npath; i++)
            {
                bool found = false;
                for (int j = nvisited-1; j >= 0; j--)
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

            int ppos = furthestPath + 1;
            int vpos = furthestVisited + 1;
            int count = Math.Min(nvisited - vpos, maxPath - ppos);
            if(count >= 0)
                Array.Copy(visited, vpos, path, ppos, count);

            return ppos + count;
        }

        public static int MergeCorridorStartShortcut(ref long[] path, int npath, int maxPath, long[] visited,
                                                     int nvisited)
        {
            int furthestPath = -1;
            int furthestVisited = -1;

            for (int i = npath - 1; i >= 0; --i)
            {
                bool found = false;
                for (int j = nvisited - 1; j >= 0; --j)
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

            int req = furthestVisited;
            if (req <= 0)
                return npath;

            int orig = furthestPath;
            int size = Math.Max(0, npath - orig);
            if (req + size > maxPath)
                size = maxPath - req;
            if(size > 0)
                Array.Copy(path, orig, path, req, size);

            for (int i = 0; i < req; i++)
            {
                path[i] = visited[i];
            }

            return req + size;
        }
    }
}