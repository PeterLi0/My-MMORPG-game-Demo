using System;


namespace LunaNav
{
    public class ObstacleAvoidanceQuery
    {
        private ObstacleAvoidanceParams _params;
        private float _invHorizTime;
        private float _vmax;
        private float _invVmax;

        private int _maxCircles;
        private ObstacleCircle[] _circles;
        private int _nCircles;

        private int _maxSegments;
        private ObstacleSegment[] _segments;
        private int _nSegments;

        private const int MaxPatternDivs = 32;
        private const int MaxPatternRings = 4;

        public ObstacleAvoidanceQuery()
        {
            _maxCircles = 0;
            _circles = null;
            _nCircles = 0;

            _maxSegments = 0;
            _segments = null;
            _nSegments = 0;
        }

        public bool Init(int maxCircles, int maxSegments)
        {
            _maxCircles = maxCircles;
            _nCircles = 0;
            _circles = new ObstacleCircle[_maxCircles];
            for (int i = 0; i < _maxCircles; i++)
            {
                _circles[i] = new ObstacleCircle();
            }
            _maxSegments = maxSegments;
            _nSegments = 0;
            _segments = new ObstacleSegment[_maxSegments];
            for (int i = 0; i < _maxSegments; i++)
            {
                _segments[i] = new ObstacleSegment();
            }
            return true;
        }

        public void Reset()
        {
            _nCircles = 0;
            _nSegments = 0;
        }

        private void Prepare(float[] pos, float[] dvel)
        {
            for (int i = 0; i < _nCircles; i++)
            {
                ObstacleCircle cir = _circles[i];

                float[] pa = pos;
                float[] pb = cir.p;

                float[] orig = new float[3];
                orig[0] = 0;
                orig[1] = 0;

                float[] dv = new float[3];
                cir.dp = Helper.VSub(pb[0], pb[1], pb[2], pa[0], pa[1], pa[2]);
                Helper.VNormalize(ref cir.dp);
                dv = Helper.VSub(cir.dvel[0], cir.dvel[1], cir.dvel[2], dvel[0], dvel[1], dvel[2]);

                float a = Helper.TriArea2D(orig, cir.dp, dv);
                if (a < 0.01f)
                {
                    cir.np[0] = -cir.dp[2];
                    cir.np[2] = cir.dp[0];
                }
                else
                {
                    cir.np[0] = cir.dp[2];
                    cir.np[2] = -cir.dp[0];
                    
                }
            }

            for (int i = 0; i < _nSegments; i++)
            {
                ObstacleSegment seg = _segments[i];

                float r = 0.01f;
                float t;
                seg.touch = Helper.DistancePtSegSqr2D(pos[0], pos[1], pos[2], seg.p[0], seg.p[1], seg.p[2], seg.q[0], seg.q[1], seg.q[2], out t) < r*r;
            }
        }

        public int SweetCircleCircle(float[] c0, float r0, float[] v, float[] c1, float r1, ref float tmin,
                                     ref float tmax)
        {
            float EPS = 0.001f;
            float[] s = Helper.VSub(c1[0], c1[1], c1[2], c0[0], c0[1], c0[2]);
            float r = r0 + r1;
            float c = Helper.VDot2D(s, s) - r*r;
            float a = Helper.VDot2D(v, v);
            if (a < EPS) return 0;

            float b = Helper.VDot2D(v, s);
            float d = b*b - a*c;
            if (d < 0.0f) return 0;
            a = 1.0f/a;
            float rd = (float)Math.Sqrt(d);
            tmin = (b - rd)*a;
            tmax = (b + rd)*a;
            return 1;
        }

        public int isectRaySeg(float[] ap, float[] u, float[] bp, float[] bq, ref float t)
        {
            float[] v = Helper.VSub(bq[0], bq[1], bq[2], bp[0], bp[1], bp[2]);
            float[] w = Helper.VSub(ap[0], ap[1], ap[2], bp[0], bp[1], bp[2]);
            float d = Helper.VPerp2D(u, v);
            if (Math.Abs(d) < 1e-6f) return 0;
            d = 1.0f/d;
            t = Helper.VPerp2D(v, w)*d;
            if (t < 0 || t > 1) return 0;
            float s = Helper.VPerp2D(u, w)*d;
            if (s < 0 || s > 1) return 0;
            return 1;
        }

        private float ProcessSample(float[] vcand, float cs, float[] pos, float rad, float[] vel, float[] dvel, ObstacleAvoidanceDebugData debug = null)
        {
            float tmin = _params.horizTime;
            float side = 0;
            int nside = 0;

            for (int i = 0; i < _nCircles; i++)
            {
                ObstacleCircle cir = _circles[i];

                float[] vab = Helper.VScale(vcand[0], vcand[1], vcand[2], 2);
                vab = Helper.VSub(vab[0], vab[1], vab[2], vel[0], vel[1], vel[2]);

                side += (float)Math.Max(0.0, Math.Min(1.0, Math.Min(Helper.VDot2D(cir.dp, vab)*0.5f + 0.5f, Helper.VDot2D(cir.np, vab)*2f)));
                nside++;

                float htmin = 0, htmax = 0;
                if (SweetCircleCircle(pos, rad, vab, cir.p, cir.rad, ref htmin, ref htmax) == 0)
                {
                    continue;
                }

                if (htmin < 0.0f && htmax > 0.0f)
                {
                    htmin = -htmin*0.5f;
                }

                if (htmin >= 0.0f)
                {
                    if (htmin < tmin)
                        tmin = htmin;
                }
            }

            for (int i = 0; i < _nSegments; i++)
            {
                ObstacleSegment seg = _segments[i];
                float htmin = 0;

                if (seg.touch)
                {
                    float[] sdir = Helper.VSub(seg.q[0], seg.q[1], seg.q[2], seg.p[0], seg.p[1], seg.p[2]);
                    float[] snorm = new float[3];
                    snorm[0] = -sdir[2];
                    snorm[2] = sdir[0];

                    if (Helper.VDot2D(snorm, vcand) < 0.0f)
                        continue;

                    htmin = 0.0f;
                }
                else
                {
                    if (isectRaySeg(pos, vcand, seg.p, seg.q, ref htmin) == 0)
                        continue;
                }

                htmin *= 2.0f;

                if (htmin < tmin)
                    tmin = htmin;
            }

            if (nside > 0)
                side /= nside;

            float vpen = _params.weightDesVel*(Helper.VDist2D(vcand, dvel)*_invVmax);
            float vcpen = _params.weightCurVel*(Helper.VDist2D(vcand, vel)*_invVmax);
            float spen = _params.weightSide*side;
            float tpen = _params.weightToi*(1.0f/(0.1f + tmin*_invHorizTime));

            float penalty = vpen + vcpen + spen + tpen;

            if(debug != null)
                debug.AddSample(vcand, cs, penalty, vpen, vcpen, spen, tpen);

            return penalty;
        }

        public void AddCircle(float[] pos, float rad, float[] vel, float[] dvel)
        {
            if (_nCircles >= _maxCircles)
                return;

            ObstacleCircle cir = _circles[_nCircles++];
            Helper.VCopy(ref cir.p, pos);
            cir.rad = rad;
            Helper.VCopy(ref cir.vel, vel);
            Helper.VCopy(ref cir.dvel, dvel);
        }

        public void AddSegment(float[] p, float[] q)
        {
            if (_nSegments >= _maxSegments)
                return;

            ObstacleSegment seg = _segments[_nSegments++];
            Helper.VCopy(ref seg.p, p);
            Helper.VCopy(ref seg.q, q);
        }

        public int SampleVelocityGrid(float[] pos, float rad, float vmax, float[] vel, float[] dvel, ref float[] nvel,
                                      ObstacleAvoidanceParams param, ObstacleAvoidanceDebugData debug = null)
        {
            Prepare(pos, dvel);

            _params = new ObstacleAvoidanceParams(param);
            _invHorizTime = 1.0f/_params.horizTime;
            _vmax = vmax;
            _invVmax = 1.0f/vmax;

            Helper.VSet(ref nvel, 0, 0, 0);

            if(debug != null)
                debug.Reset();

            float cvx = dvel[0]*_params.velBias;
            float cvz = dvel[2]*_params.velBias;
            float cs = vmax*2*(1 - _params.velBias)/(float) (_params.gridSize - 1);
            float half = (_params.gridSize - 1)*cs*0.5f;

            float minPenalty = float.MaxValue;
            int ns = 0;

            for (int y = 0; y < _params.gridSize; y++)
            {
                for (int x = 0; x < _params.gridSize; x++)
                {
                    float[] vcand = new float[3];
                    vcand[0] = cvx + x*cs - half;
                    vcand[1] = 0;
                    vcand[2] = cvz + y*cs - half;

                    if (vcand[0]*vcand[0] + vcand[2]*vcand[2] > (vmax + cs/2)*(vmax + cs/2)) continue;

                    float penalty = ProcessSample(vcand, cs, pos, rad, vel, dvel, debug);
                    ns++;
                    if (penalty < minPenalty)
                    {
                        minPenalty = penalty;
                        Helper.VCopy(ref nvel, vcand);
                    }
                }
            }
            return ns;
        }

        public int SampleVelocityAdaptive(float[] pos, float rad, float vmax, float[] vel, float[] dvel,
                                          ref float[] nvel, ObstacleAvoidanceParams param,
                                          ObstacleAvoidanceDebugData debug = null)
        {
            Prepare(pos, dvel);

            _params = new ObstacleAvoidanceParams(param);
            _invHorizTime = 1.0f/_params.horizTime;
            _vmax = vmax;
            _invVmax = 1.0f/vmax;

            Helper.VSet(ref nvel, 0, 0, 0);

            if(debug != null)
                debug.Reset();

            float[] pat = new float[(MaxPatternDivs*MaxPatternRings+1)*2];
            int npat = 0;

            int ndivs = _params.adaptiveDivs;
            int nrings = _params.adaptiveRings;
            int depth = _params.adaptiveDepth;

            int nd = Math.Max(1, Math.Min(MaxPatternDivs, ndivs));
            int nr = Math.Max(1, Math.Min(MaxPatternRings, nrings));
            float da = (1.0f/nd)*(float)Math.PI*2f;
            float dang = (float)Math.Atan2(dvel[2], dvel[0]);

            pat[npat*2 + 0] = 0;
            pat[npat*2 + 1] = 0;
            npat++;

            for (int j = 0; j < nr; j++)
            {
                float r = (nr - j)/(float)nr;
                float a = dang + (j & 1)*0.5f*da;
                for (int i = 0; i < nd; i++)
                {
                    pat[npat*2 + 0] = (float)Math.Cos(a)*r;
                    pat[npat*2 + 1] = (float) Math.Sin(a)*r;
                    npat++;
                    a += da;
                }
            }

            float cr = vmax*(1.0f - _params.velBias);
            float[] res = new float[3];
            Helper.VSet(ref res, dvel[0]*_params.velBias, 0, dvel[2]*_params.velBias);
            int ns = 0;

            for (int k = 0; k < depth; k++)
            {
                float minPenalty = float.MaxValue;
                float[] bvel = new float[3];
                Helper.VSet(ref bvel, 0, 0, 0);

                for (int i = 0; i < npat; i++)
                {
                    float[] vcand = {res[0] + pat[i*2 + 0]*cr, 0, res[2] + pat[i*2 + 1]*cr};

                    if (vcand[0]*vcand[0] + vcand[2]*vcand[2] > (vmax + 0.001f)*(vmax + 0.001f)) continue;

                    float penalty = ProcessSample(vcand, cr/10f, pos, rad, vel, dvel, debug);
                    ns++;
                    if (penalty < minPenalty)
                    {
                        minPenalty = penalty;
                        Helper.VCopy(ref bvel, vcand);
                    }
                }
                Helper.VCopy(ref res, bvel);

                cr *= 0.5f;
            }

            Helper.VCopy(ref nvel, res);

            return ns;
        }

        public int ObstacleCircleCount
        {
            get { return _nCircles; }
        }

        public ObstacleCircle GetObstacleCircle(int i)
        {
            return _circles[i];
        }

        public int ObstacleSegmentCount
        {
            get { return _nSegments; }
        }

        public ObstacleSegment GetObstacleSegment(int i)
        {
            return _segments[i];
        }
    }
}