using System;



namespace LunaNav
{
    public class LocalBoundary
    {
        private const int MaxLocalSegs = 8;
        private const int MaxLocalPolys = 18;

        class Segment
        {
            public float[] s = new float[6];
            public float d;
        }

        private float[] _center = new float[3];
        private Segment[] _segs = new Segment[MaxLocalSegs];
        private int _nsegs;

        private long[] _polys = new long[MaxLocalPolys];
        private int _npolys;

        public LocalBoundary()
        {
            for (int i = 0; i < MaxLocalSegs; i++)
            {
                _segs[i] = new Segment();
            }
            Reset();
        }

        private void AddSegment(float dist, float[] s)
        {
            Segment seg = null;
            if (_nsegs <= 0)
            {
                seg = _segs[0];
            }
            else if (dist >= _segs[_nsegs - 1].d)
            {
                if (_nsegs >= MaxLocalSegs)
                    return;
                seg = _segs[_nsegs];
            }
            else
            {
                // insert between
                int i;
                for (i = 0; i < _nsegs; i++)
                {
                    if (dist <= _segs[i].d)
                        break;
                }

                int tgt = i + 1;
                int n = Math.Min(_nsegs - i, MaxLocalSegs - tgt);
                if (n < 0)
                {
                    Array.Copy(_segs, i, _segs, tgt, n);
                }

                seg = _segs[i];
            }

            seg.d = dist;
            Array.Copy(s, seg.s, 6);

            if (_nsegs < MaxLocalSegs)
                _nsegs++;
        }

        public void Reset()
        {
            _nsegs = 0;
            _npolys = 0;

            Helper.VSet(ref _center, float.MaxValue, float.MaxValue, float.MaxValue);
        }

        public void Update(long refId, float[] pos, float collisionQueryRange, NavMeshQuery navQuery, QueryFilter filter)
        {
            int MaxSegsPerPoly = NavMeshBuilder.VertsPerPoly * 3;

            if (refId <= 0)
            {
                Reset();
                return;
            }

            Helper.VCopy(ref _center, pos);
            long[] parentPoly = null;
            navQuery.FindLocalNeighbourhood(refId, pos, collisionQueryRange, filter, ref _polys, ref parentPoly, ref _npolys,
                                            MaxLocalPolys);

            _nsegs = 0;
            float[] segs = new float[MaxSegsPerPoly*6];
            int nsegs = 0;
            long[] parentrefs = null;
            for (int j = 0; j < _npolys; j++)
            {
                navQuery.GetPolyWallSegments(_polys[j], filter, ref segs, ref parentrefs, ref nsegs, MaxSegsPerPoly);
                for (int k = 0; k < nsegs; k++)
                {
                    int s = k*6;

                    float tseg;
                    float distSqr = Helper.DistancePtSegSqr2D(pos[0], pos[1], pos[2], segs[s + 0], segs[s + 1],
                                                              segs[s + 2], segs[s + 3], segs[s + 4], segs[s + 5],
                                                              out tseg);
                    if (distSqr > collisionQueryRange*collisionQueryRange)
                        continue;
                    float[] tempS = new float[6];
                    Array.Copy(segs, s, tempS, 0, 6);
                    AddSegment(distSqr, tempS);
                }
            }
        }

        public bool IsValid(NavMeshQuery navQuery, QueryFilter filter)
        {
            if (_npolys <= 0)
                return false;

            for (int i = 0; i < _npolys; i++)
            {
                if (!navQuery.IsValidPolyRef(_polys[i], filter))
                    return false;
            }
            return true;
        }

        public float[] Center
        {
            get { return _center; }
        }

        public int SegmentCount
        {
            get { return _nsegs; }
        }

        public float[] GetSegment(int i)
        {
            return _segs[i].s;
        }
    }
}