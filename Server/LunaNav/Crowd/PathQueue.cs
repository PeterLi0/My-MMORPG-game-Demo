using System;



namespace LunaNav
{
    public class PathQueue
    {
        public const long PathQInvalid = 0;

        private class PathQuery
        {
            public long refId;
            public float[] startPos = new float[3], endPos = new float[3];
            public long startRef, endRef;
            public long[] path;
            public int npath;
            public Status status;
            public int keepAlive;
            public QueryFilter filter;
        }

        private const int MaxQueue = 8;
        private PathQuery[] _queue;
        private long _nextHandle;
        private int _maxPathSize;
        private int _queueHead;
        private NavMeshQuery _navQuery;

        public PathQueue()
        {
            _nextHandle = 1;
            _maxPathSize = 0;
            _queueHead = 0;
            _navQuery = null;
            _queue = new PathQuery[MaxQueue];
            for (int i = 0; i < MaxQueue; i++)
            {
                _queue[i] = new PathQuery();
                _queue[i].path = null;
            }
        }

        private void Purge()
        {
            _navQuery = null;
            for (int i = 0; i < MaxQueue; i++)
            {
                _queue[i].path = null;
            }
        }

        public bool Init(int maxPathSize, int maxSearchNodeCount, NavMesh nav)
        {
            Purge();
            _navQuery = new NavMeshQuery();
            if ((_navQuery.Init(nav, maxSearchNodeCount) & Status.Failure) != 0)
                return false;
            _maxPathSize = maxPathSize;
            for (int i = 0; i < MaxQueue; i++)
            {
                _queue[i].refId = PathQInvalid;
                _queue[i].path = new long[_maxPathSize];
            }

            _queueHead = 0;
            return true;
        }

        public void Update(int maxIters)
        {
            int MaxKeepAlive = 2;
            int iterCount = maxIters;
            for (int i = 0; i < MaxQueue; i++)
            {
                PathQuery q = _queue[_queueHead%MaxQueue];
                if (q.refId == PathQInvalid)
                {
                    _queueHead++;
                    continue;
                }

                if ((q.status & Status.Success) != 0 || (q.status & Status.Failure) != 0)
                {
                    q.keepAlive++;
                    if (q.keepAlive > MaxKeepAlive)
                    {
                        q.refId = PathQInvalid;
                        q.status = 0;
                    }

                    _queueHead++;
                    continue;
                }

                if (q.status == 0)
                {
                    q.status = _navQuery.InitSlicedFindPath(q.startRef, q.endRef, q.startPos, q.endPos, q.filter);
                }

                if ((q.status & Status.InProgress) != 0)
                {
                    int iters = 0;
                    q.status = _navQuery.UpdateSlicedFindPath(iterCount, ref iters);
                    iterCount -= iters;
                }
                if ((q.status & Status.Success) != 0)
                {
                    q.status = _navQuery.FinalizeSlicedFindPath(ref q.path, ref q.npath, _maxPathSize);
                }

                if (iterCount <= 0)
                    break;

                _queueHead++;
            }
        }

        public long Request(long startRef, long endRef, float[] startPos, float[] endPos, QueryFilter filter)
        {
            int slot = -1;
            for (int i = 0; i < MaxQueue; i++)
            {
                if (_queue[i].refId == PathQInvalid)
                {
                    slot = i;
                    break;
                }
            }

            if (slot == -1)
            {
                return PathQInvalid;
            }

            long refId = _nextHandle++;
            if (_nextHandle == PathQInvalid) _nextHandle++;

            PathQuery q = _queue[slot];
            q.refId = refId;
            Helper.VCopy(ref q.startPos, startPos);
            q.startRef = startRef;
            Helper.VCopy(ref q.endPos, endPos);
            q.endRef = endRef;

            q.status = 0;
            q.npath = 0;
            q.filter = filter;
            q.keepAlive = 0;

            return refId;
        }

        public Status GetRequestStatus(long refId)
        {
            for (int i = 0; i < MaxQueue; i++)
            {
                if (_queue[i].refId == refId)
                    return _queue[i].status;
            }
            return Status.Failure;
        }

        public Status GetPathResult(long refId, ref long[] path, ref int pathSize, int maxPath)
        {
            for (int i = 0; i < MaxQueue; i++)
            {
                if (_queue[i].refId == refId)
                {
                    PathQuery q = _queue[i];
                    q.refId = PathQInvalid;
                    q.status = 0;
                    int n = Math.Min(q.npath, maxPath);
                    Array.Copy(q.path, path, n);
                    pathSize = n;
                    return Status.Success;
                }
            }
            return Status.Failure;
        }

        public NavMeshQuery NavQuery
        {
            get { return _navQuery; }
        }
    }
}