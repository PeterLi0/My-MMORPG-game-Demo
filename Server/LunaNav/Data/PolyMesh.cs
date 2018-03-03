using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class PolyMesh
	{
	    public int[] Verts;
        public int[] Polys;
        public int[] Regs;
        public int[] Flags;
        public short[] Areas;
        public int NVerts;
        public int NPolys;
        public int MaxPolys;
        public int Nvp;
        public float[] BMin = new float[3];
        public float[] BMax = new float[3];
        public float Cs;
        public float Ch;
        public int BorderSize;

	    public static int VertexBucketCount = (1 << 12);
	    public static int MeshNullIdx = 0xffff;
	    public PolyMesh(ContourSet cset, int nvp)
	    {
	        Array.Copy(cset.BMin, BMin, 3);
            Array.Copy(cset.BMax, BMax, 3);
	        Cs = cset.Cs;
	        Ch = cset.Ch;
	        BorderSize = cset.BorderSize;

	        int maxVertices = 0;
	        int maxTris = 0;
	        int maxVertsPerCont = 0;
	        for (int i = 0; i < cset.NConts; i++)
	        {
	            if (cset.Conts[i].NVerts < 3) continue;
	            maxVertices += cset.Conts[i].NVerts;
	            maxTris += cset.Conts[i].NVerts - 2;
	            maxVertsPerCont = Math.Max(maxVertsPerCont, cset.Conts[i].NVerts);
	        }

            short[] vflags = new short[maxVertices];

            Verts = new int[maxVertices*3];
            Polys = new int[maxTris*nvp*2];
            Regs = new int[maxTris];
            Areas = new short[maxTris];

	        NVerts = 0;
	        NPolys = 0;
	        Nvp = nvp;
	        MaxPolys = maxTris;

	        for (int i = 0; i < maxTris*nvp*2; i++)
	        {
	            Polys[i] = 0xffff; // memset(mesh.polys, 0xff)
	        }

            int[] nextVert = new int[maxVertices];

            int[] firstVert = new int[VertexBucketCount];
            for (int i = 0; i < firstVert.Length; i++)
            {
                firstVert[i] = -1;
            }

            int[] indices = new int[maxVertsPerCont];
            int[] tris = new int[maxVertsPerCont*3];
            int[] polys = new int[(maxVertsPerCont+1)*nvp];
            int[] tmpPoly = new int[nvp];

	        for (int i = 0; i < cset.NConts; i++)
	        {
	            Contour cont = cset.Conts[i];

	            if (cont.NVerts < 3) continue;

	            for (int j = 0; j < cont.NVerts; j++)
	            {
	                indices[j] = j;
	            }

	            int ntris = Triangulate(cont.NVerts, cont.Verts, ref indices, ref tris);
                if (ntris <= 0)
                {
                    // error
                    ntris = -ntris;
                }

	            for (int j = 0; j < cont.NVerts; j++)
	            {
	                int v = j*4;
	                indices[j] = AddVertex(cont.Verts[v + 0], cont.Verts[v + 1], cont.Verts[v + 2], ref firstVert, ref nextVert);
                    if ((cont.Verts[v + 3] & ContourSet.BorderVertex) != 0)
                    {
                        vflags[indices[j]] = 1;
                    }
	            }

                //build initial polygons
	            int npolys = 0;
	            for (int j = 0; j < polys.Length; j++)
	            {
                    polys[j] = 0xffff;
	            }
	            for (int j = 0; j < ntris; j++)
	            {
	                int t = j*3;
                    if (tris[t + 0] != tris[t + 1] && tris[t + 0] != tris[t + 2] && tris[t + 1] != tris[t + 2])
                    {
                        polys[npolys * nvp + 0] = indices[tris[t + 0]];
                        polys[npolys * nvp + 1] = indices[tris[t + 1]];
                        polys[npolys * nvp + 2] = indices[tris[t + 2]];
                        npolys++;
                    }
	            }
	            if (npolys == 0)
	                continue;

                if (nvp > 3)
                {
                    for (;;)
                    {
                        int bestMergeVal = 0;
                        int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                        for (int j = 0; j < npolys-1; j++)
                        {
                            int pj = j*nvp; // polys[j*nvp]
                            for (int k = j+1; k < npolys; k++)
                            {
                                int pk = k*nvp;
                                int ea = 0, eb = 0;
                                int v = GetPolyMergeValue(polys, pj, pk, Verts, ref ea, ref eb, nvp);
                                if (v > bestMergeVal)
                                {
                                    bestMergeVal = v;
                                    bestPa = j;
                                    bestPb = k;
                                    bestEa = ea;
                                    bestEb = eb;
                                }
                            }
                        }

                        if (bestMergeVal > 0)
                        {
                            int pa = bestPa*nvp;
                            int pb = bestPb*nvp;
                            MergePolys(ref polys, pa, pb, bestEa, bestEb, tmpPoly, nvp);
                            // fill in the hole at pb with the last poly
                            Array.Copy(polys, (npolys-1)*nvp, polys, pb, nvp);
                            npolys--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

	            for (int j = 0; j < npolys; j++)
	            {
	                int p = NPolys*nvp*2;
	                int q = j*nvp;
	                for (int k = 0; k < nvp; k++)
	                {
	                    Polys[p + k] = polys[q + k];
	                }
	                Regs[NPolys] = cont.Reg;
	                Areas[NPolys] = cont.Area;
	                NPolys++;
	            }
	        }

	        for (int i = 0; i < NVerts; i++)
	        {
                //if (vflags[i] > 0)
                //{
                //    if (!CanRemoveVertex(i)) continue;
                //    if (!RemoveVertex(i, maxTris))
                //    {
                //        // error
                //    }

                //    for (int j = i; j < NVerts; j++)
                //    {
                //        vflags[j] = vflags[j + 1];
                //    }
                //    i--;
                //}
	        }

            if (!BuildMeshAdjacency(nvp))
            {
                // error
            }

            // find portal edges
            if (BorderSize > 0) // defaults to 0
            {
                int w = cset.Width;
                int h = cset.Height;
                for (int i = 0; i < NPolys; i++)
                {
                    int p = i * 2 * nvp;
                    for (int j = 0; j < nvp; j++)
                    {
                        if (Polys[p + j] == MeshNullIdx) break;
                        if (Polys[p + nvp + j] != MeshNullIdx) continue;
                        int nj = j + 1;
                        if (nj >= nvp || Polys[p + nj] == MeshNullIdx) nj = 0;
                        int va = Polys[p + j] * 3;
                        int vb = Polys[p + nj] * 3;

                        if (Verts[va + 0] == 0 && Verts[vb + 0] == 0)
                            Polys[p + nvp + j] = 0x8000 | 0;
                        else if (Verts[va + 2] == h && Verts[vb + 2] == h)
                            Polys[p + nvp + j] = 0x8000 | 1;
                        else if (Verts[va + 0] == w && Verts[vb + 0] == w)
                            Polys[p + nvp + j] = 0x8000 | 2;
                        else if (Verts[va + 2] == 0 && Verts[vb + 2] == 0)
                            Polys[p + nvp + j] = 0x8000 | 3;
                    }
                }
            }

            // user fills in this data
            Flags = new int[NPolys];
	    }

	    private int Triangulate(int n, int[] verts, ref int[] indices, ref int[] tris)
	    {
	        int ntris = 0;
	        int trisIndex = 0;

	        for (int i = 0; i < n; i++)
	        {
	            int i1 = Next(i, n);
	            int i2 = Next(i1, n);
	            if (Diagonal(i, i2, n, verts, indices))
	                indices[i1] = (int)(indices[i1] | 0x80000000);
	        }

            while (n > 3)
            {
                int minLen = -1;
                int mini = -1;
                for (int i = 0; i < n; i++)
                {
                    int i1 = Next(i, n);
                    if ((int) (indices[i1] & 0x80000000) != 0)
                    {
                        int p0 = (indices[i] & 0x0fffffff)*4;
                        int p2 = (indices[Next(i1, n) & 0x0fffffff])*4;

                        int dx = verts[p2 + 0] - verts[p0 + 0];
                        int dy = verts[p2 + 2] - verts[p0 + 2];
                        int len = dx*dx + dy*dy;

                        if (minLen < 0 || len < minLen)
                        {
                            minLen = len;
                            mini = i;
                        }
                    }
                }

                if (mini == -1)
                    return -ntris;

                int j = mini;
                int j1 = Next(j, n);
                int j2 = Next(j1, n);

                tris[trisIndex++] = indices[j] & 0x0fffffff;
                tris[trisIndex++] = indices[j1] & 0x0fffffff;
                tris[trisIndex++] = indices[j2] & 0x0fffffff;
                ntris++;

                n--;
                for (int k = j1; k < n; k++)
                {
                    indices[k] = indices[k + 1];
                }

                if (j1 >= n) j1 = 0;
                j = Prev(j1, n);

                if (Diagonal(Prev(j, n), j1, n, verts, indices))
                    indices[j] = (int) (indices[j] | 0x80000000);
                else
                {
                    indices[j] &= 0x0fffffff;
                }

                if (Diagonal(j, Next(j1, n), n, verts, indices))
                    indices[j1] = (int) (indices[j1] | 0x80000000);
                else
                {
                    indices[j1] &= 0x0fffffff;
                }
            }

            tris[trisIndex++] = indices[0] & 0x0fffffff;
            tris[trisIndex++] = indices[1] & 0x0fffffff;
            tris[trisIndex++] = indices[2] & 0x0fffffff;
	        ntris++;

	        return ntris;
	    }

	    private bool Diagonal(int i, int j, int n, int[] verts, int[] indices)
	    {
	        return InCone(i, j, n, verts, indices) && Diagonalie(i, j, n, verts, indices);
	    }

	    private bool Diagonalie(int i, int j, int n, int[] verts, int[] indices)
	    {
	        int d0 = (indices[i] & 0x0fffffff)*4;
	        int d1 = (indices[j] & 0x0fffffff)*4;

	        for (int k = 0; k < n; k++)
	        {
	            int k1 = Next(k, n);

                if (!((k == i) || (k1 == i) || (k == j) || (k1 == j)))
                {
                    int p0 = (indices[k] & 0x0fffffff)*4;
                    int p1 = (indices[k1] & 0x0fffffff)*4;

                    if(VEqual(verts[d0+0], verts[d0+2], verts[p0+0], verts[p0+2]) || VEqual(verts[d1+0], verts[d1+2], verts[p0+0], verts[p0+2]) || VEqual(verts[d0+0], verts[d0+2], verts[p1+0], verts[p1+2]) || VEqual(verts[d1+0], verts[d1+2], verts[p1+0], verts[p1+2]))
                        continue;

                    if (Intersect(verts[d0 + 0], verts[d0 + 2], verts[d1 + 0], verts[d1 + 2], verts[p0 + 0],
                                  verts[p0 + 2], verts[p1 + 0], verts[p1 + 2]))
                        return false;
                }
	        }
	        return true;
	    }

	    private bool Intersect(int ax, int az, int bx, int bz, int cx, int cz, int dx, int dz)
	    {
            if (IntersectProp(ax, az, bx, bz, cx, cz, dx, dz))
                return true;
            else if (Between(ax, az, bx, bz, cx, cz) || Between(ax, az, bx, bz, dx, dz) ||
                     Between(cx, cz, dx, dz, ax, az) || Between(cx, cz, dx, dz, bx, bz))
                return true;
            else
                return false;
	    }

	    private bool IntersectProp(int ax, int az, int bx, int bz, int cx, int cz, int dx, int dz)
	    {
	        if (Collinear(ax, az, bx, bz, cx, cz) || Collinear(ax, az, bx, bz, dx, dz) ||
	            Collinear(cx, cz, dx, dz, ax, az) || Collinear(cx, cz, dx, dz, bx, bz))
	            return false;

	        return Xorb(Left(ax, az, bx, bz, cx, cz), Left(ax, az, bx, bz, dx, dz)) &&
	               Xorb(Left(cx, cz, dx, dz, ax, az), Left(cx, cz, dx, dz, bx, bz));
	    }

	    private bool Xorb(bool x, bool y)
	    {
	        return !x ^ !y;
	    }

	    private bool Between(int ax, int az, int bx, int bz, int cx, int cz)
	    {
	        if (!Collinear(ax, az, bx, bz, cx, cz))
	            return false;
	        if (ax != bx)
	            return ((ax <= cx) && (cx <= bx)) || ((ax >= cx) && (cx >= bx));
	        else
	            return ((az <= cz) && (cz <= bz)) || ((az >= cz) && (cz >= bz));
	    }

	    private bool VEqual(int ax, int az, int bx, int bz)
	    {
	        return ax == bx && az == bz;
	    }

	    private bool InCone(int i, int j, int n, int[] verts, int[] indices)
	    {
	        int pi = (indices[i] & 0x0fffffff)*4;
	        int pj = (indices[j] & 0x0fffffff)*4;
	        int pi1 = (indices[Next(i, n)] & 0x0fffffff)*4;
	        int pin1 = (indices[Prev(i, n)] & 0x0fffffff)*4;

	        if (LeftOn(verts[pin1+0], verts[pin1+2], verts[pi+0], verts[pi+2], verts[pi1+0], verts[pi1+2]))
                return Left(verts[pi + 0], verts[pi + 2], verts[pj + 0], verts[pj + 2], verts[pin1 + 0], verts[pin1 + 2]) && Left(verts[pj + 0], verts[pj + 2], verts[pi + 0], verts[pi + 2], verts[pi1 + 0], verts[pi1 + 2]);
            return !(LeftOn(verts[pi + 0], verts[pi + 2], verts[pj + 0], verts[pj + 2], verts[pi1 + 0], verts[pi1 + 2]) && LeftOn(verts[pj + 0], verts[pj + 2], verts[pi + 0], verts[pi + 2], verts[pin1 + 0], verts[pin1 + 2]));
	    }

	    private bool LeftOn(int ax, int az, int bx, int bz, int cx, int cz)
	    {
	        return Area2(ax, az, bx, bz, cx, cz) <= 0;
	    }

	    private bool Left(int ax, int az, int bx, int bz, int cx, int cz)
        {
            return Area2(ax, az, bx, bz, cx, cz) < 0;
        }

        private bool Collinear(int ax, int az, int bx, int bz, int cx, int cz)
        {
            return Area2(ax, az, bx, bz, cx, cz) == 0;
        }

        private int Area2(int ax, int az, int bx, int bz, int cx, int cz)
        {
            return (bx - ax)*(cz - az) - (cx - ax)*(bz - az);
        }

        private int Next(int i, int n)
	    {
	        return i + 1 < n ? i + 1 : 0;
	    }

        private int Prev(int i, int n)
        {
            return i - 1 >= 0 ? i - 1 : n - 1;
        }

	    private int AddVertex(int x, int y, int z, ref int[] firstVert, ref int[] nextVert)
	    {
	        int bucket = ComputeVertexHash(x, 0, z);
	        int i = firstVert[bucket];

            while (i != -1)
            {
                int v = i*3;
                if (Verts[v + 0] == x && Math.Abs(Verts[v + 1] - y) <= 2 && Verts[v + 2] == z)
                    return i;
                i = nextVert[i];
            }

            i = NVerts;
	        NVerts++;
	        int v1 = i*3;
            Verts[v1 + 0] = x;
            Verts[v1 + 1] = y;
            Verts[v1 + 2] = z;
	        nextVert[i] = firstVert[bucket];
	        firstVert[bucket] = i;

	        return i;
	    }

	    private int ComputeVertexHash(int x, int y, int z)
	    {
	        uint h1 = 0x8da6b343; // Large multiplicative constants;
	        uint h2 = 0xd8163841; // here arbitrarily chosen primes
	        uint h3 = 0xcb1ab31f;
	        uint n = (uint)(h1 * x + h2 * y + h3 * z);
	        return (int)(n & (VertexBucketCount-1));	        
	    }

	    private int GetPolyMergeValue(int[] polys, int pa, int pb, int[] verts, ref int ea, ref int eb, int nvp)
	    {
            int na = CountPolyVerts(polys, pa, nvp);
            int nb = CountPolyVerts(polys, pb, nvp);

	        if (na + nb - 2 > nvp)
	            return -1;

	        ea = -1;
	        eb = -1;

	        for (int i = 0; i < na; i++)
	        {
                int va0 = polys[pa + i];
                int va1 = polys[pa + ((i + 1) % na)];
                if (va0 > va1)
                {
                    int temp = va0;
                    va0 = va1;
                    va1 = temp;
                }
	            for (int j = 0; j < nb; j++)
	            {
                    int vb0 = polys[pb + j];
                    int vb1 = polys[pb + ((j + 1) % nb)];
                    if (vb0 > vb1)
                    {
                        int temp = vb0;
                        vb0 = vb1;
                        vb1 = temp;
                    }
                    if (va0 == vb0 && va1 == vb1)
                    {
                        ea = i;
                        eb = j;
                        break;
                    }
	            }
	        }

	        if (ea == -1 || eb == -1)
	            return -1;

	        int va, vb, vc;
            va = polys[pa + ((ea + na - 1) % na)];
            vb = polys[pa + ea];
            vc = polys[pb + ((eb + 2) % nb)];
            if (!ULeft(verts[va * 3 + 0], verts[va * 3 + 2], verts[vb * 3 + 0], verts[vb * 3 + 2], verts[vc * 3 + 0], verts[vc * 3 + 2]))
	            return -1;

            va = polys[pb + ((eb + nb - 1) % nb)];
            vb = polys[pb + eb];
            vc = polys[pa + ((ea + 2) % na)];
            if (!ULeft(verts[va * 3 + 0], verts[va * 3 + 2], verts[vb * 3 + 0], verts[vb * 3 + 2], Verts[vc * 3 + 0], verts[vc * 3 + 2]))
	            return -1;

            va = polys[pa + ea];
            vb = polys[pa + ((ea + 1) % na)];

            int dx = verts[va * 3 + 0] - verts[vb * 3 + 0];
            int dz = verts[va * 3 + 2] - verts[vb * 3 + 2];

	        return dx*dx + dz*dz;
	    }

	    private bool ULeft(int ax, int az, int bx, int bz, int cx, int cz)
	    {
	        return (bx - ax)*(cz - az) - (cx - ax)*(bz - az) < 0;
	    }

	    private void MergePolys(ref int[] polys, int pa, int pb, int ea, int eb, int[] tmpPoly, int nvp)
	    {
            int na = CountPolyVerts(polys, pa, nvp);
            int nb = CountPolyVerts(polys, pb, nvp);

	        for (int i = 0; i < nvp; i++)
	        {
                tmpPoly[i] = 0xffff;
	        }
	        int n = 0;
	        for (int i = 0; i < na-1; i++)
	        {
                tmpPoly[n++] = polys[pa + ((ea + 1 + i) % na)];
	        }
	        for (int i = 0; i < nb-1; i++)
	        {
                tmpPoly[n++] = polys[pb + ((eb + 1 + i) % nb)];
	        }

            Array.Copy(tmpPoly, 0, polys, pa, nvp);
	    }

	    private bool CanRemoveVertex(int rem)
	    {
	        int nvp = Nvp;

	        int numRemovedVerts = 0;
	        int numTouchedVerts = 0;
	        int numRemainingEdges = 0;

	        for (int i = 0; i < NPolys; i++)
	        {
	            int p = i*nvp*2;
	            int nv = CountPolyVerts(Polys, p, nvp);
	            int numRemoved = 0;
	            int numVerts = 0;
	            for (int j = 0; j < nv; j++)
	            {
	                if (Polys[p + j] == rem)
	                {
	                    numTouchedVerts++;
	                    numRemoved++;
	                }
	                numVerts++;
	            }
                if (numRemoved > 0)
                {
                    numRemovedVerts += numRemoved;
                    numRemainingEdges += numVerts - (numRemoved + 1);
                }
	        }

	        if (numRemainingEdges <= 2)
	            return false;

	        int maxEdges = numTouchedVerts*2;
	        int nedges = 0;
            int[] edges = new int[maxEdges*3];

	        for (int i = 0; i < NPolys; i++)
	        {
	            int p = i*nvp*2;
	            int nv = CountPolyVerts(Polys, p, nvp);

	            for (int j = 0, k = nv-1; j < nv; k=j++)
	            {
	                if (Polys[p + j] == rem || Polys[p + k] == rem)
	                {
	                    int a = Polys[p + j], b = Polys[p + k];
                        if (b == rem)
                        {
                            int temp = a;
                            a = b;
                            b = temp;
                        }

	                    bool exists = false;
	                    for (int m = 0; m < nedges; m++)
	                    {
	                        int e = m*3;
                            if (edges[e + 1] == b)
                            {
                                edges[e + 2]++;
                                exists = true;
                            }
	                    }
                        if (!exists)
                        {
                            int e = nedges*3;
                            edges[e + 0] = a;
                            edges[e + 1] = b;
                            edges[e + 2] = 1;
                            nedges++;
                        }
	                }
	            }
	        }

	        int numOpenEdges = 0;
	        for (int i = 0; i < nedges; i++)
	        {
	            if (edges[i*3 + 2] < 2)
	                numOpenEdges++;
	        }
	        if (numOpenEdges > 2)
	            return false;

	        return true;
	    }

	    private bool RemoveVertex(int rem, int maxTris)
	    {
	        int nvp = Nvp;

	        int numRemovedVerts = 0;
	        for (int i = 0; i < NPolys; i++)
	        {
	            int p = i*nvp*2;
	            int nv = CountPolyVerts(Polys, p, nvp);
	            for (int j = 0; j < nv; j++)
	            {
	                if (Polys[p + j] == rem)
	                {
	                    numRemovedVerts++;
	                }
	            }
	        }
	        int nedges = 0;
            int[] edges = new int[numRemovedVerts*nvp*4];

	        int nhole = 0;
            int[] hole = new int[numRemovedVerts*nvp];

	        int nhreg = 0;
            int[] hreg = new int[numRemovedVerts*nvp];

	        int nharea = 0;
            int[] harea = new int[numRemovedVerts*nvp];

	        for (int i = 0; i < NPolys; i++)
	        {
	            int p = i*nvp*2;
	            int nv = CountPolyVerts(Polys, p, nvp);
	            bool hasRem = false;
	            for (int j = 0; j < nv; j++)
	            {
	                if (Polys[p + j] == rem) hasRem = true;
	            }
                if (hasRem)
                {
                    for (int j = 0, k = nv-1; j < nv; k=j++)
                    {
                        if (Polys[p + j] != rem && Polys[p + k] != rem)
                        {
                            int e = nedges*4;
                            edges[e + 0] = Polys[p + k];
                            edges[e + 1] = Polys[p + j];
                            edges[e + 2] = Regs[i];
                            edges[e + 3] = Areas[i];
                            nedges++;
                        }
                    }

                    int p2 = (NPolys - 1)*nvp*2;
                    Array.Copy(Polys, p2, Polys, p, nvp);
                    for (int j = p+nvp; j < p+nvp+nvp; j++)
                    {
                        Polys[j] = 0xffff;
                    }
                    Regs[i] = Regs[NPolys - 1];
                    Areas[i] = Areas[NPolys - 1];
                    NPolys--;
                    --i;
                }
	        }

	        for (int i = rem; i < NVerts-1; i++)
	        {
                Verts[i * 3 + 0] = Verts[(i + 1) * 3 + 0];
                Verts[i * 3 + 1] = Verts[(i + 1) * 3 + 1];
                Verts[i * 3 + 2] = Verts[(i + 1) * 3 + 2];
	        }
	        NVerts--;

	        for (int i = 0; i < NPolys; i++)
	        {
	            int p = i*nvp*2;
	            int nv = CountPolyVerts(Polys, p, nvp);
	            for (int j = 0; j < nv; j++)
	            {
	                if (Polys[p + j] > rem) Polys[p+j]--;
	            }
	        }
	        for (int i = 0; i < nedges; i++)
	        {
	            if (edges[i*4 + 0] > rem) edges[i*4 + 0]--;
	            if (edges[i*4 + 1] > rem) edges[i*4 + 1]--;
	        }

	        PushBack(edges[0], ref hole, ref nhole);
	        PushBack(edges[2], ref hreg, ref nhreg);
	        PushBack(edges[3], ref harea, ref nharea);

            while (nedges > 0)
            {
                bool match = false;
                for (int i = 0; i < nedges; i++)
                {
                    int ea = edges[i*4 + 0];
                    int eb = edges[i*4 + 1];
                    int r = edges[i*4 + 2];
                    int a = edges[i*4 + 3];
                    bool add = false;
                    if (hole[0] == eb)
                    {
                        PushFront(ea, ref hole, ref nhole);
                        PushFront(r, ref hreg, ref nhreg);
                        PushFront(a, ref harea, ref nharea);
                        add = true;
                    }
                    else if (hole[nhole - 1] == ea)
                    {
                        PushBack(eb, ref hole, ref nhole);
                        PushBack(r, ref hreg, ref nhreg);
                        PushBack(a, ref harea, ref nharea);
                        add = true;
                    }
                    if (add)
                    {
                        edges[i * 4 + 0] = edges[(nedges - 1) * 4 + 0];
                        edges[i * 4 + 1] = edges[(nedges - 1) * 4 + 1];
                        edges[i * 4 + 2] = edges[(nedges - 1) * 4 + 2];
                        edges[i * 4 + 3] = edges[(nedges - 1) * 4 + 3];
                        --nedges;
                        match = true;
                        --i;
                    }
                }
                if (!match)
                    break;
            }

            int[] tris = new int[nhole*3];
            int[] tverts = new int[nhole*4];
            int[] thole = new int[nhole];

	        for (int i = 0; i < nhole; i++)
	        {
	            int pi = hole[i];
                tverts[i * 4 + 0] = Verts[pi * 3 + 0];
                tverts[i * 4 + 1] = Verts[pi * 3 + 1];
                tverts[i * 4 + 2] = Verts[pi * 3 + 2];
	            tverts[i*4 + 3] = 0;
	            thole[i] = i;
	        }

	        int ntris = Triangulate(nhole, tverts, ref thole, ref tris);
            if (ntris < 0)
            {
                ntris = -ntris;
            }

            int[] polys = new int[ntris*nvp];
            int[] pregs = new int[ntris];
            int[] pareas = new int[ntris];

	        int[] tmpPoly = new int[nvp];

	        int npolys = 0;
	        for (int i = 0; i < ntris*nvp; i++)
	        {
                polys[i] = 0xffff;
	        }
	        for (int j = 0; j < ntris; j++)
	        {
	            int t = j*3;
                if (tris[t + 0] != tris[t + 1] && tris[t + 0] != tris[t + 2] && tris[t + 1] != tris[t + 2])
                {
                    polys[npolys * nvp + 0] = hole[tris[t + 0]];
                    polys[npolys * nvp + 1] = hole[tris[t + 1]];
                    polys[npolys * nvp + 2] = hole[tris[t + 2]];
                    pregs[npolys] = hreg[tris[t + 0]];
                    pareas[npolys] = harea[tris[t + 0]];
                    npolys++;
                }
	        }
	        if (npolys == 0)
	            return true;

            if (nvp > 3)
            {
                for (;;)
                {
                    int bestMergeVal = 0;
                    int bestPa = 0, bestPb = 0, bestEa = 0, bestEb = 0;

                    for (int j = 0; j < npolys-1; j++)
                    {
                        int pj = j*nvp;
                        for (int k = j+1; k < npolys; k++)
                        {
                            int pk = k*nvp;
                            int ea = 0, eb = 0;
                            int v = GetPolyMergeValue(polys, pj, pk, Verts, ref ea, ref eb, nvp);
                            if (v > bestMergeVal)
                            {
                                bestMergeVal = v;
                                bestPa = j;
                                bestPb = k;
                                bestEa = ea;
                                bestEb = eb;
                            }
                        }
                    }

                    if (bestMergeVal > 0)
                    {
                        int pa = bestPa*nvp;
                        int pb = bestPb*nvp;
                        MergePolys(ref polys, pa, pb, bestEa, bestEb, tmpPoly, nvp);
                        Array.Copy(polys, pb, polys, (npolys-1)*nvp, nvp);
                        pregs[bestPb] = pregs[npolys - 1];
                        pareas[bestPb] = pareas[npolys - 1];
                        npolys--;
                    }
                    else
                    {
                        break;
                    }
                }
            }

	        for (int i = 0; i < npolys; i++)
	        {
	            if (NPolys >= maxTris) break;
	            int p = NPolys*nvp*2;
	            for (int j = p; j < p+nvp*2; j++)
	            {
                    Polys[j] = 0xffff;
	            }
	            for (int j = 0; j < nvp; j++)
	            {
	                Polys[p + j] = polys[i*nvp + j];
	            }
	            Regs[NPolys] = pregs[i];
	            Areas[NPolys] = (short)pareas[i];
	            NPolys++;
	        }
	        return true;
	    }

	    private void PushFront(int v, ref int[] arr, ref int an)
	    {
	        an++;
	        for (int i = an-1; i > 0; i--)
	        {
	            arr[i] = arr[i - 1];
	        }
	        arr[0] = v;
	    }

	    private void PushBack(int v, ref int[] arr, ref int an)
	    {
	        arr[an] = v;
	        an++;
	    }

	    private int CountPolyVerts(int[] polys, int p, int nvp)
	    {
	        for (int i = 0; i < nvp; i++)
	        {
	            if (polys[p + i] == MeshNullIdx)
	                return i;
	        }
	        return nvp;
	    }

	    private bool BuildMeshAdjacency(int vertsPerPoly)
	    {
	        int maxEdgeCount = NPolys*vertsPerPoly;
            int[] firstEdge = new int[NVerts];
	        int[] nextEdge = new int[maxEdgeCount];

	        int edgeCount = 0;

            Edge[] edges = new Edge[maxEdgeCount];
	        for (int i = 0; i < maxEdgeCount; i++)
	        {
	            edges[i] = new Edge();
	        }

	        for (int i = 0; i < NVerts; i++)
	        {
	            firstEdge[i] = MeshNullIdx;
	        }

	        for (int i = 0; i < NPolys; i++)
	        {
	            int t = i*vertsPerPoly*2;
	            for (int j = 0; j < vertsPerPoly; j++)
	            {
	                if (Polys[t + j] == MeshNullIdx) break;
	                int v0 = Polys[t + j];
	                int v1 = (j + 1 >= vertsPerPoly || Polys[t + j + 1] == MeshNullIdx) ? Polys[t + 0] : Polys[t + j + 1];
                    if (v0 < v1)
                    {
                        Edge edge = edges[edgeCount];
                        edge.Vert[0] = v0;
                        edge.Vert[1] = v1;
                        edge.Poly[0] = i;
                        edge.PolyEdge[0] = j;
                        edge.Poly[1] = i;
                        edge.PolyEdge[1] = 0;

                        nextEdge[edgeCount] = firstEdge[v0];
                        firstEdge[v0] = edgeCount;
                        edgeCount++;
                    }
	            }
	        }

	        for (int i = 0; i < NPolys; i++)
	        {
	            int t = i*vertsPerPoly*2;
	            for (int j = 0; j < vertsPerPoly; j++)
	            {
	                if (Polys[t + j] == MeshNullIdx) break;
	                int v0 = Polys[t + j];
	                int v1 = (j + 1 >= vertsPerPoly || Polys[t + j + 1] == MeshNullIdx) ? Polys[t + 0] : Polys[t + j + 1];
                    if (v0 > v1)
                    {
                        for (int e = firstEdge[v1]; e < MeshNullIdx; e = nextEdge[e])
                        {
                            Edge edge = edges[e];
                            if (edge.Vert[1] == v0 && edge.Poly[0] == edge.Poly[1])
                            {
                                edge.Poly[1] = i;
                                edge.PolyEdge[1] = j;
                                break;
                            }
                        }
                    }
	            }
	        }

	        for (int i = 0; i < edgeCount; i++)
	        {
	            Edge e = edges[i];
                if (e.Poly[0] != e.Poly[1])
                {
                    int p0 = e.Poly[0]*vertsPerPoly*2;
                    int p1 = e.Poly[1]*vertsPerPoly*2;

                    Polys[p0 + vertsPerPoly + e.PolyEdge[0]] = e.Poly[1];
                    Polys[p1 + vertsPerPoly + e.PolyEdge[1]] = e.Poly[0];
                }
	        }

	        return true;
	    }
	}
}
