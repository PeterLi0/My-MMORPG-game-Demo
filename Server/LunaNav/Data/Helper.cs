using System;


namespace LunaNav
{
    public class Helper
    {
        public static int NavMeshMagic = 1;
        public static int NavMeshVersion = 1;
        public static int StatusDetailMast = 0x0fffffff;
        protected static int[] offsetX = { -1, 0, 1, 0 };
        protected static int[] offsetY = { 0, 1, 0, -1 };

        public Helper()
        {
        }
        
        public static int GetDirOffsetX(int dir)
        {
            return offsetX[dir & 0x03];
        }

        public static int GetDirOffsetY(int dir)
        {
            return offsetY[dir & 0x03];
        }

        public static long NextPow2(long v)
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

        public static long Ilog2(long v)
        {
            long r;
            long shift;
            r = (v > 0xffff) ? 1 << 4 : 0 << 4;
            v >>= (int)r;
            shift = (v > 0xff) ? 1 << 3 : 0 << 3; v >>= (int) shift; r |= shift;
            shift = (v > 0xf) ? 1 << 2 : 0 << 2; v >>= (int) shift; r |= shift;
            shift = (v > 0x3) ? 1 << 1 : 0 << 1; v >>= (int)shift; r |= shift;
            r |= (v >> 1);
            return r;
        }

        public static float GetSlabCoord(float vax, float vay, float vaz, int side)
        {
            if (side == 0 || side == 4)
                return vax;
            if (side == 2 || side == 6)
                return vaz;
            return 0;
        }

        public static void CalcSlabEndPoints(float vax, float vay, float vaz, float vbx, float vby, float vbz, ref float[] bmin, ref float[] bmax, int side)
        {
            if (side == 0 || side == 4)
            {
                if (vaz < vbz)
                {
                    bmin[0] = vaz;
                    bmin[1] = vay;
                    bmax[0] = vbz;
                    bmax[1] = vby;
                }
                else
                {
                    bmin[0] = vbz;
                    bmin[1] = vby;
                    bmax[0] = vaz;
                    bmax[1] = vay;
                }
            }
            else if (side == 2 || side == 6)
            {
                if (vax < vbx)
                {
                    bmin[0] = vax;
                    bmin[1] = vay;
                    bmax[0] = vbx;
                    bmax[1] = vby;
                }
                else
                {
                    bmin[0] = vbx;
                    bmin[1] = vby;
                    bmax[0] = vax;
                    bmax[1] = vay;
                }
            }
        }

        public static bool OverlapSlabs(float[] amin, float[] amax, float[] bmin, float[] bmax, float px, float py)
        {
            float minx = Math.Max(amin[0] + px, bmin[0] + px);
            float maxx = Math.Min(amax[0] - px, bmax[0] - px);
            if (minx > maxx)
                return false;

            float ad = (amax[1] - amin[1])/(amax[0] - amin[0]);
            float ak = amin[1] - ad*amin[0];
            float bd = (bmax[1] - bmin[1])/(bmax[0] - bmin[0]);
            float bk = bmin[1] - bd*bmin[0];
            float aminy = ad*minx + ak;
            float amaxy = ad*maxx + ak;
            float bminy = bd*minx + bk;
            float bmaxy = bd*maxx + bk;
            float dmin = bminy - aminy;
            float dmax = bmaxy - amaxy;

            if (dmin*dmax < 0)
                return true;

            float thr = (py*2)*(py*2);
            if (dmin*dmin <= thr || dmax*dmax <= thr)
                return true;

            return false;
        }

        public static int OppositeTile(int side)
        {
            return (side + 4) & 0x7;
        }

        public static float VDist(float v1x, float v1y, float v1z, float v2x, float v2y, float v2z)
        {
            float dx = v2x - v1x;
            float dy = v2y - v1y;
            float dz = v2z - v1z;
            return (float)Math.Sqrt(dx*dx + dy*dy + dz*dz);
        }

        public static void VLerp(ref float[] dest, float v1x, float v1y, float v1z, float v2x, float v2y, float v2z, float t)
        {
            dest[0] = v1x + (v2x - v1x)*t;
            dest[1] = v1y + (v2y - v1y)*t;
            dest[2] = v1z + (v2z - v1z)*t;
        }

        public static bool DistancePtPolyEdgesSqr(float ptx, float pty, float ptz, float[] verts, int nverts, ref float[] ed, ref float[] et)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = nverts-1; i < nverts; j=i++)
            {
                int vi = i*3;
                int vj = j*3;
                if (((verts[vi + 2] > ptz) != (verts[vj + 2] > ptz)) &&
                    (ptx < (verts[vj + 0] - verts[vi + 0])*(ptz - verts[vi + 2])/(verts[vj + 2] - verts[vi + 2]) + verts[vi + 0]))
                    c = !c;
                ed[j] = DistancePtSegSqr2D(ptx, pty, ptz, verts[vj + 0], verts[vj + 1], verts[vj + 2], verts[vi + 0],
                                              verts[vi + 1], verts[vi + 2], ref et, j);
            }

            return c;
        }

        public static float DistancePtSegSqr2D(float ptx, float pty, float ptz, float px, float py, float pz, float qx, float qy, float qz, ref float[] et, int t)
        {
            float pqx = qx - px;
            float pqz = qz - pz;
            float dx = ptx - px;
            float dz = ptz - pz;
            float d = pqx*pqx + pqz*pqz;
            et[t] = pqx*dx + pqz*dz;
            if (d > 0) et[t] /= d;
            if (et[t] < 0) et[t] = 0;
            else if(et[t] > 1) et[t] = 1;
            dx = px + et[t]*pqx - ptx;
            dz = pz + et[t]*pqz - ptz;
            return dx*dx + dz*dz;
        }

        public static float DistancePtSegSqr2D(float ptx, float pty, float ptz, float px, float py, float pz, float qx, float qy, float qz, out float t)
        {
            float pqx = qx - px;
            float pqz = qz - pz;
            float dx = ptx - px;
            float dz = ptz - pz;
            float d = pqx * pqx + pqz * pqz;
            t = pqx * dx + pqz * dz;
            if (d > 0) t /= d;
            if (t < 0) t = 0;
            else if (t > 1) t = 1;
            dx = px + t * pqx - ptx;
            dz = pz + t * pqz - ptz;
            return dx * dx + dz * dz;
        }

        public static bool ClosestHeightPointTriangle(float px, float py, float pz, float ax, float ay, float az, float bx, float by, float bz, float cx, float cy, float cz, ref float h)
        {
            float[] v0 = VSub(cx, cy, cz, ax, ay, az);
            float[] v1 = VSub(bx, by, bz, ax, ay, az);
            float[] v2 = VSub(px, py, pz, ax, ay, az);

            float dot00 = VDot2D(v0, v0);
            float dot01 = VDot2D(v0, v1);
            float dot02 = VDot2D(v0, v2);
            float dot11 = VDot2D(v1, v1);
            float dot12 = VDot2D(v1, v2);

            float invDenom = 1.0f/(dot00*dot11 - dot01*dot01);
            float u = (dot11*dot02 - dot01*dot12)*invDenom;
            float v = (dot00*dot12 - dot01*dot02)*invDenom;

            float EPS = 1e-4f;
            if (u >= -EPS && v >= -EPS && (u + v) <= 1 + EPS)
            {
                h = ay + v0[1]*u + v1[1]*v;
                return true;
            }
            return false;
        }

        public static float VDot2D(float[] u, float[] v)
        {
            return u[0]*v[0] + u[2]*v[2];
        }

        public static float VDot(float[] u, float[] v)
        {
            return u[0]*v[0] + u[1]*v[1] + u[2]*v[2];
        }

        public static void VMad(ref float[] dest, float[] v1, float[] v2, float s)
        {
            dest[0] = v1[0] + v2[0] * s;
            dest[1] = v1[1] + v2[1] * s;
            dest[2] = v1[2] + v2[2] * s;
        }

        public static float[] VSub(float v1x, float v1y, float v1z, float v2x, float v2y, float v2z)
        {
            float[] dest = new float[3];
            dest[0] = v1x - v2x;
            dest[1] = v1y - v2y;
            dest[2] = v1z - v2z;

            return dest;
        }

        public static float[] VAdd(float v1x, float v1y, float v1z, float v2x, float v2y, float v2z)
        {
            float[] dest = new float[3];
            dest[0] = v1x + v2x;
            dest[1] = v1y + v2y;
            dest[2] = v1z + v2z;
            return dest;
        }

        public static float VDistSqr(float v1x, float v1y, float v1z, float v2x, float v2y, float v2z)
        {
            float dx = v2x - v1x;
            float dy = v2y - v1y;
            float dz = v2z - v1z;
            return dx*dx + dy*dy + dz*dz;
        }

        public static bool OverlapQuantBounds(int[] amin, int[] amax, int[] bmin, int[] bmax)
        {
            bool overlap = true;
            overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
            overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
            overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
            return overlap;
        }

        public static void VMin(ref float[] mn, float vx, float vy, float vz)
        {
            mn[0] = Math.Min(mn[0], vx);
            mn[1] = Math.Min(mn[1], vx);
            mn[2] = Math.Min(mn[2], vx);
        }

        public static void VMax(ref float[] mn, float vx, float vy, float vz)
        {
            mn[0] = Math.Max(mn[0], vx);
            mn[1] = Math.Max(mn[1], vx);
            mn[2] = Math.Max(mn[2], vx);
        }

        public static bool OverlapBounds(float aminx, float aminy, float aminz, float amaxx, float amaxy, float amaxz, float bminx, float bminy, float bminz, float bmaxx, float bmaxy, float bmaxz)
        {
            bool overlap = true;
            overlap = (aminx > bmaxx || amaxx < bminx) ? false : overlap;
            overlap = (aminy > bmaxy || amaxy < bminy) ? false : overlap;
            overlap = (aminz > bmaxz || amaxz < bminz) ? false : overlap;
            return overlap;

        }

        public static long HashRef(long a)
        {
            a += ~(a << 15);
            a ^= (a >> 10);
            a += (a << 3);
            a ^= (a >> 6);
            a += ~(a << 11);
            a ^= (a >> 16);
            return a;
        }

        public static float TriArea2D(float[] a, float[] b, float[] c)
        {
            float abx = b[0] - a[0];
            float abz = b[2] - a[2];
            float acx = c[0] - a[0];
            float acz = c[2] - a[2];
            return acx*abz - abx*acz;
        }

        public static void RandomPointInConvexPoly(float[] pts, int npts, ref float[] areas, float s, float t, ref float[] outPt)
        {
            float areasum = 0.0f;
            float[] va = new float[3], vb = new float[3], vc = new float[3];
            for (int i = 2; i < npts; i++)
            {
                Array.Copy(pts, 0, va, 0, 3);
                Array.Copy(pts, (i-1)*3, vb, 0, 3);
                Array.Copy(pts, i*3, vc, 0, 3);
                areas[i] = TriArea2D(va, vb, vc);
                areasum += Math.Max(0.001f, areas[i]);
            }

            float thr = s*areasum;
            float acc = 0.0f;
            float u = 0.0f;
            int tri = 0;
            for (int i = 2; i < npts; i++)
            {
                float dacc = areas[i];
                if (thr >= acc && thr < (acc + dacc))
                {
                    u = (thr - acc)/dacc;
                    tri = i;
                    break;
                }
                acc += dacc;
            }

            float v = (float)Math.Sqrt(t);

            float a = 1 - v;
            float b = (1 - u)*v;
            float c = u*v;
            int pa = 0;
            int pb = (tri - 1)*3;
            int pc = tri*3;

            outPt[0] = a * pts[pa + 0] + b * pts[pb + 0] + c * pts[pc + 0];
            outPt[1] = a * pts[pa + 1] + b * pts[pb + 1] + c * pts[pc + 1];
            outPt[2] = a * pts[pa + 2] + b * pts[pb + 2] + c * pts[pc + 2];
        }

        public static bool VEqual(float p0x, float p0y, float p0z, float p1x, float p1y, float p1z)
        {
            float thr = ((1.0f/16384.0f)*(1.0f/16384.0f));
            float d = VDistSqr(p0x, p0y, p0z, p1x, p1y, p1z);
            return d < thr;
        }

        public static bool IntersectSegSeg2D(float apx, float apy, float apz, float aqx, float aqy, float aqz, float[] bp, float[] bq, ref float s, ref float t)
        {
            float[] u = VSub(aqx, aqy, aqz, apx, apy, apz);
            float[] v = VSub(bq[0], bq[1], bq[2], bp[0], bp[1], bp[2]);
            float[] w = VSub(apx, apy, apz, bp[0], bp[1], bp[2]);
            float d = VPerpXZ(u, v);
            if (Math.Abs(d) < 1e-6f) return false;
            s = VPerpXZ(v, w)/d;
            t = VPerpXZ(u, w)/d;
            return true;
        }

        private static float VPerpXZ(float[] a, float[] b)
        {
            return a[0]*b[2] - a[2]*b[0];
        }

        public static bool PointInPolygon(float ptx, float pty, float ptz, float[] verts, int nverts)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = nverts-1; i < nverts; j=i++)
            {
                int vi = i*3;
                int vj = j*3;
                if (((verts[vi + 2] > ptz) != (verts[vj + 2] > ptz)) &&
                    (ptx < (verts[vj + 0] - verts[vi + 0])*(ptz - verts[vi + 2])/(verts[vj + 2] - verts[vi + 2]) + verts[vi + 0]))
                    c = !c;
            }
            return c;
        }

        public static bool IntersectSegmentPoly2D(float[] p0, float[] p1, float[] verts, int nverts, out float tmin, out float tmax, out int segMin, out int segMax)
        {
            float EPS = 0.00000001f;

            tmin = 0;
            tmax = 1;
            segMin = -1;
            segMax = -1;

            float[] dir = VSub(p1[0], p1[1], p1[2], p0[0], p0[1], p0[2]);

            for (int i = 0, j = nverts-1; i < nverts; j=i++)
            {
                float[] edge = VSub(verts[i*3 + 0], verts[i*3 + 1], verts[i*3 + 2], verts[j*3 + 0], verts[j*3 + 1], verts[j*3 + 2]);
                float[] diff = VSub(p0[0], p0[1], p0[2], verts[j*3 + 0], verts[j*3 + 1], verts[j*3 + 2]);
                float n = VPerp2D(edge, diff);
                float d = VPerp2D(dir, edge);
                if (Math.Abs(d) < EPS)
                {
                    if (n < 0)
                        return false;
                    else
                    {
                        continue;
                    }
                }
                float t = n/d;
                if (d < 0)
                {
                    if (t > tmin)
                    {
                        tmin = t;
                        segMin = j;
                        if (tmin > tmax)
                            return false;
                    }
                }
                else
                {
                    if (t < tmax)
                    {
                        tmax = t;
                        segMax = j;
                        if (tmax < tmin)
                            return false;
                    }
                }
            }
            return true;
        }

        public static float VPerp2D(float[] u, float[] v)
        {
            return u[2]*v[0] - u[0]*v[2];
        }

        public static void VNormalize(ref float[] v)
        {
            float d = 1.0f/(float)Math.Sqrt(v[0]*v[0] + v[1]*v[1] + v[2]*v[2]);
            v[0] *= d;
            v[1] *= d;
            v[2] *= d;
        }

        public static float[] VScale(float vx, float vy, float vz, float t)
        {
            float[] dest = {vx*t, vy*t, vz*t};
            return dest;
        }

        public static bool OverlapPolyPoly2D(float[] polya, int npolya, float[] polyb, int npolyb)
        {
            float eps = 1e-4f;

            for (int i = 0, j = npolya-1; i < npolya; j=i++)
            {
                int va = j*3;
                int vb = i*3;
                float[] n = {polya[vb + 2] - polya[va + 2], 0, -(polya[vb + 0] - polya[va + 0])};
                float amin, amax, bmin, bmax;
                ProjectPoly(n, polya, npolya, out amin, out amax);
                ProjectPoly(n, polyb, npolyb, out bmin, out bmax);
                if (!OverlapRange(amin, amax, bmin, bmax, eps))
                    return false;
            }
            for (int i = 0, j = npolya - 1; i < npolyb; j = i++)
            {
                int va = j * 3;
                int vb = i * 3;
                float[] n = { polyb[vb + 2] - polyb[va + 2], 0, -(polyb[vb + 0] - polyb[va + 0]) };
                float amin, amax, bmin, bmax;
                ProjectPoly(n, polya, npolya, out amin, out amax);
                ProjectPoly(n, polyb, npolyb, out bmin, out bmax);
                if (!OverlapRange(amin, amax, bmin, bmax, eps))
                    return false;
            }
            return true;
        }

        private static bool OverlapRange(float amin, float amax, float bmin, float bmax, float eps)
        {
            return ((amin + eps) > bmax || (amax - eps) < bmin) ? false : true;
        }

        private static void ProjectPoly(float[] axis, float[] poly, int npoly, out float rmin, out float rmax)
        {
            float[] temp = new float[3];
            Array.Copy(poly, 0, temp, 0, 3);
            rmin = rmax = VDot2D(axis, temp);
            for (int i = 1; i < npoly; i++)
            {
                Array.Copy(poly, i*3, temp, 0, 3);
                float d = VDot2D(axis, temp);
                rmin = Math.Min(rmin, d);
                rmax = Math.Max(rmax, d);
            }
        }

        public static void VSet(ref float[] dest, float x, float y, float z)
        {
            dest[0] = x;
            dest[1] = y;
            dest[2] = z;
        }

        public static void VCopy(ref float[] dest, float[] a)
        {
            dest[0] = a[0];
            dest[1] = a[1];
            dest[2] = a[2];
        }

        public static float VDist2D(float[] v1, float[] v2)
        {
            float dx = v2[0] - v1[0];
            float dz = v2[2] - v1[2];
            return (float)Math.Sqrt(dx*dx + dz*dz);
        }

        public static float VDist2DSqr(float v1x, float v1y, float v1z, float v2x, float v2y, float v2z)
        {
            float dx = v2x - v1x;
            float dz = v2z - v1z;
            return dx*dx + dz*dz;
        }

        public static float VLen(float[] v)
        {
            return (float) Math.Sqrt(v[0]*v[0] + v[1]*v[1] + v[2]*v[2]);
        }

        public static float VLenSqr(float[] v)
        {
            return v[0]*v[0] + v[1]*v[1] + v[2]*v[2];
        }

        internal static float Clamp(float v, float min, float max)
        {
            return v < min ? min : (v > max ? max : v);
        }
    }
}