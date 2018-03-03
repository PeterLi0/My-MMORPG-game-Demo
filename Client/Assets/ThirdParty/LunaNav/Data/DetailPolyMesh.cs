using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class DetailPolyMesh
	{
        public long[] Meshes { get; set; }
        public float[] Verts { get; set; }
        public short[] Tris { get; set; }
        public int NMeshes { get; set; }
        public int NVerts { get; set; }
        public int NTris { get; set; }

	    public static int UnsetHeight = 0xffff;

	    public static int Undef = -1;
	    public static int Hull = -2;

        protected float[] DistPtTriV0 = new float[3], DistPtTriV1 = new float[3], DistPtTriV2 = new float[3];

        public DetailPolyMesh(PolyMesh mesh, CompactHeightfield chf, float sampleDist, float sampleMaxError)
        {
            if (mesh.NVerts == 0 || mesh.NPolys == 0)
                return;

            int nvp = mesh.Nvp;
            float cs = mesh.Cs;
            float ch = mesh.Ch;
            float[] orig = mesh.BMin;
            int borderSize = mesh.BorderSize;

            IntArray edges = new IntArray(64);
            IntArray tris = new IntArray(512);
            IntArray stack = new IntArray(512);
            IntArray samples = new IntArray(512);
            float[] verts = new float[256*3];
            HeightPatch hp = new HeightPatch();
            int nPolyVerts = 0;
            int maxhw = 0, maxhh = 0;

            int[] bounds = new int[mesh.NPolys*4];
            float[] poly = new float[nvp*3];

            for (int i = 0; i < mesh.NPolys; i++)
            {
                int p = i*nvp*2;
                int xmin = i*4 + 0;
                int xmax = i*4 + 1;
                int ymin = i*4 + 2;
                int ymax = i*4 + 3;

                bounds[xmin] = chf.Width;
                bounds[xmax] = 0;
                bounds[ymin] = chf.Height;
                bounds[ymax] = 0;

                for (int j = 0; j < nvp; j++)
                {
                    if (mesh.Polys[p + j] == PolyMesh.MeshNullIdx) break;
                    int v = mesh.Polys[p + j]*3;
                    bounds[xmin] = Math.Min(bounds[xmin], mesh.Verts[v + 0]);
                    bounds[xmax] = Math.Max(bounds[xmax], mesh.Verts[v + 0]);
                    bounds[ymin] = Math.Min(bounds[ymin], mesh.Verts[v + 2]);
                    bounds[ymax] = Math.Max(bounds[ymax], mesh.Verts[v + 2]);
                    nPolyVerts++;
                }
                bounds[xmin] = Math.Max(0, bounds[xmin] - 1);
                bounds[xmax] = Math.Min(chf.Width, bounds[xmax] + 1);
                bounds[ymin] = Math.Max(0, bounds[ymin] - 1);
                bounds[ymax] = Math.Min(chf.Height, bounds[ymax] + 1);
                if (bounds[xmin] >= bounds[xmax] || bounds[ymin] >= bounds[ymax]) continue;
                maxhw = Math.Max(maxhw, bounds[xmax] - bounds[xmin]);
                maxhh = Math.Max(maxhh, bounds[ymax] - bounds[ymin]);
            }

            hp.Data = new int[maxhw*maxhh];

            NMeshes = mesh.NPolys;
            //NVerts = 0;
            //NTris = 0;
            Meshes = new long[NMeshes*4];

            int vcap = nPolyVerts + nPolyVerts/2;
            int tcap = vcap*2;

            NVerts = 0;
            Verts = new float[vcap*3];
            NTris = 0;
            Tris = new short[tcap*4];
            int nverts;
            for (int i = 0; i < mesh.NPolys; i++)
            {
                int p = i*nvp*2;
                int npoly = 0;
                for (int j = 0; j < nvp; j++)
                {
                    if (mesh.Polys[p + j] == PolyMesh.MeshNullIdx) break;
                    int v = mesh.Polys[p + j]*3;
                    poly[j * 3 + 0] = mesh.Verts[v + 0] * cs;
                    poly[j * 3 + 1] = mesh.Verts[v + 1] * ch;
                    poly[j * 3 + 2] = mesh.Verts[v + 2] * cs;
                    npoly++;
                }

                hp.XMin = bounds[i*4 + 0];
                hp.YMin = bounds[i*4 + 2];
                hp.Width = bounds[i*4 + 1] - bounds[i*4 + 0];
                hp.Height = bounds[i*4 + 3] - bounds[i*4 + 2];

				int[] tempPoly = new int[nvp];
                Array.Copy(mesh.Polys, p, tempPoly, 0, nvp);
				
                GetHeightData(chf, tempPoly, npoly, mesh.Verts, borderSize, ref hp, ref stack);

                if (!BuildPolyDetail(poly, npoly, sampleDist, sampleMaxError, chf, hp, ref verts, out nverts, ref tris, ref edges, ref samples))
                {
                    return;
                }

                for (int j = 0; j < nverts; j++)
                {
                    verts[j * 3 + 0] += orig[0];
                    verts[j * 3 + 1] += orig[1] + chf.Ch;
                    verts[j * 3 + 2] += orig[2];
                }
                for (int j = 0; j < npoly; j++)
                {
                    poly[j * 3 + 0] += orig[0];
                    poly[j * 3 + 1] += orig[1];
                    poly[j * 3 + 2] += orig[2];
                }

                int ntris = tris.Size/4;

                Meshes[i*4 + 0] = NVerts;
                Meshes[i*4 + 1] = nverts;
                Meshes[i*4 + 2] = NTris;
                Meshes[i*4 + 3] = ntris;

                if (NVerts + nverts > vcap)
                {
                    while (NVerts + nverts > vcap)
                        vcap += 256;

                    float[] newv = new float[vcap*3];
                    if(NVerts > 0)
                        Array.Copy(Verts, newv, 3*NVerts);

                    Verts = newv;
                }

                for (int j = 0; j < nverts; j++)
                {
                    Verts[NVerts * 3 + 0] = verts[j * 3 + 0];
                    Verts[NVerts * 3 + 1] = verts[j * 3 + 1];
                    Verts[NVerts * 3 + 2] = verts[j * 3 + 2];
                    NVerts++;
                }

                if (NTris + ntris > tcap)
                {
                    while (NTris + ntris > tcap)
                        tcap += 256;
                    short[] newt = new short[tcap*4];
                    if(NTris > 0)
                        Array.Copy(Tris, newt, 4*NTris);

                    Tris = newt;
                }
                for (int j = 0; j < ntris; j++)
                {
                    int t = j*4;
                    Tris[NTris * 4 + 0] = (short)tris[t + 0];
                    Tris[NTris * 4 + 1] = (short)tris[t + 1];
                    Tris[NTris * 4 + 2] = (short)tris[t + 2];
                    Tris[NTris * 4 + 3] = GetTriFlags(verts, tris[t + 0]*3, verts, tris[t+1]*3, verts, tris[t+2]*3, poly, npoly);
                    NTris++;
                }
            }
        }

	    private void GetHeightData(CompactHeightfield chf, int[] p, int npoly, int[] verts, int bs, ref HeightPatch hp, ref IntArray stack)
	    {
	        for (int i = 0; i < hp.Width*hp.Height; i++)
	        {
	            hp.Data[i] = 0;
	        }

            stack.Resize(0);

	        int[] offset = {0, 0, -1, -1, 0, -1, 1, -1, 1, 0, 1, 1, 0, 1, -1, 1, -1, 0};

	        for (int j = 0; j < npoly; j++)
	        {
	            int cx = 0, cz = 0, ci = -1;

	            int dmin = UnsetHeight;
	            for (int k = 0; k < 9; k++)
	            {
	                int ax = verts[p[j]*3 + 0] + offset[k*2 + 0];
	                int ay = verts[p[j]*3 + 1];
	                int az = verts[p[j]*3 + 2] + offset[k*2 + 1];
	                if (ax < hp.XMin || ax >= hp.XMin + hp.Width || az < hp.YMin || az >= hp.YMin + hp.Height)
	                    continue;

	                CompactCell c = chf.Cells[(ax + bs) + (az + bs)*chf.Width];
	                for (int i = (int)c.Index, ni = (int)(c.Index+c.Count); i < ni; i++)
	                {
	                    CompactSpan s = chf.Spans[i];
	                    int d = Math.Abs(ay - s.Y);
                        if (d < dmin)
                        {
                            cx = ax;
                            cz = az;
                            ci = i;
                            dmin = d;
                        }
	                }
	            }
                if (ci != -1)
                {
                    stack.Push(cx);
                    stack.Push(cz);
                    stack.Push(ci);
                }
	        }

	        int pcx = 0, pcz = 0;
	        for (int j = 0; j < npoly; j++)
	        {
	            pcx += verts[p[j]*3 + 0];
	            pcz += verts[p[j]*3 + 2];
	        }
	        pcx /= npoly;
	        pcz /= npoly;

	        for (int i = 0; i < stack.Size; i+=3)
	        {
	            int cx = stack[i + 0];
	            int cy = stack[i + 1];
	            int idx = cx - hp.XMin + (cy - hp.YMin)*hp.Width;
	            hp.Data[idx] = 1;
	        }

	        while (stack.Size > 0)
	        {
	            int ci = stack.Pop();
	            int cy = stack.Pop();
	            int cx = stack.Pop();

                if (Math.Abs(cx - pcx) <= 1 && Math.Abs(cy - pcz) <= 1)
                {
                    stack.Resize(0);
                    stack.Push(cx);
                    stack.Push(cy);
                    stack.Push(ci);
                    break;
                }

	            CompactSpan cs = chf.Spans[ci];

	            for (int dir = 0; dir < 4; dir++)
	            {
	                if (cs.GetCon(dir) == CompactHeightfield.NotConnected) continue;

	                int ax = cx + Helper.GetDirOffsetX(dir);
	                int ay = cy + Helper.GetDirOffsetY(dir);

	                if (ax < hp.XMin || ax >= (hp.XMin + hp.Width) ||
	                    ay < hp.YMin || ay >= (hp.YMin + hp.Height))
	                    continue;

	                if (hp.Data[ax - hp.XMin + (ay - hp.YMin)*hp.Width] != 0)
	                    continue;

	                int ai = (int)chf.Cells[(ax + bs) + (ay + bs)*chf.Width].Index + cs.GetCon(dir);

	                int idx = ax - hp.XMin + (ay - hp.YMin)*hp.Width;
	                hp.Data[idx] = 1;

                    stack.Push(ax);
                    stack.Push(ay);
                    stack.Push(ai);
	            }
	        }

	        for (int i = 0; i < hp.Data.Length; i++)
	        {
	            hp.Data[i] = UnsetHeight;
	        }

	        for (int i = 0; i < stack.Size; i+=3)
	        {
	            int cx = stack[i + 0];
	            int cy = stack[i + 1];
	            int ci = stack[i + 2];
	            int idx = cx - hp.XMin + (cy - hp.YMin)*hp.Width;
	            CompactSpan cs = chf.Spans[ci];
	            hp.Data[idx] = cs.Y;
	        }

	        int RetractSize = 256;
	        int head = 0;

            while (head*3 < stack.Size)
            {
                int cx = stack[head*3 + 0];
                int cy = stack[head*3 + 1];
                int ci = stack[head*3 + 2];
                head++;
                if (head >= RetractSize)
                {
                    head = 0;
                    if (stack.Size > RetractSize*3)
                    {
                        Array.Copy(stack.Data, RetractSize*3, stack.Data, 0, stack.Size-RetractSize*3);
                    }
                    stack.Resize(stack.Size-RetractSize*3);
                }

                CompactSpan cs = chf.Spans[ci];
                for (int dir = 0; dir < 4; dir++)
                {
                    if (cs.GetCon(dir) == CompactHeightfield.NotConnected) continue;

                    int ax = cx + Helper.GetDirOffsetX(dir);
                    int ay = cy + Helper.GetDirOffsetY(dir);

                    if (ax < hp.XMin || ax >= (hp.XMin + hp.Width) ||
                        ay < hp.YMin || ay >= (hp.YMin + hp.Height))
                        continue;

                    if (hp.Data[ax - hp.XMin + (ay - hp.YMin)*hp.Width] != UnsetHeight)
                        continue;

                    int ai = (int) chf.Cells[(ax + bs) + (ay + bs)*chf.Width].Index + cs.GetCon(dir);

                    CompactSpan aspan = chf.Spans[ai];
                    int idx = ax - hp.XMin + (ay - hp.YMin)*hp.Width;
                    hp.Data[idx] = aspan.Y;

                    stack.Push(ax);
                    stack.Push(ay);
                    stack.Push(ai);
                }
            }
	    }

	    private bool BuildPolyDetail(float[] inArray, int nin, float sampleDist, float sampleMaxError, CompactHeightfield chf, HeightPatch hp, ref float[] verts, out int nverts, ref IntArray tris, ref IntArray edges, ref IntArray samples)
	    {
	        int MaxVerts = 127;
	        int MaxTris = 255;
	        int MaxVertsPerEdge = 32;
            float[] edge = new float[(MaxVertsPerEdge+1)*3];
            int[] hull = new int[MaxVerts];
	        int nhull = 0;

	        nverts = 0;

	        for (int i = 0; i < nin; i++)
	        {
	            Array.Copy(inArray, i*3, verts, i*3, 3);
	        }

	        nverts = nin;

	        float cs = chf.Cs;
	        float ics = 1.0f/cs;

            if (sampleDist > 0)
            {
                for (int i = 0, j = nin-1; i < nin; j=i++)
                {
                    int vj = j*3;
                    int vi = i*3;

                    bool swapped = false;
                    if (Math.Abs(inArray[vj + 0] - inArray[vi + 0]) < 1e-6f)
                    {
                        if (inArray[vj + 2] > inArray[vi + 2])
                        {
                            int temp = vj;
                            vj = vi;
                            vi = temp;
                            swapped = true;
                        }
                    }
                    else
                    {
                        if (inArray[vj + 0] > inArray[vi + 0])
                        {
                            int temp = vj;
                            vj = vi;
                            vi = temp;
                            swapped = true;                            
                        }
                    }

                    float dx = inArray[vi + 0] - inArray[vj + 0];
                    float dy = inArray[vi + 1] - inArray[vj + 1];
                    float dz = inArray[vi + 2] - inArray[vj + 2];
                    float d = (float)Math.Sqrt(dx*dx + dz*dz);
                    int nn = 1 + (int) Math.Floor(d/sampleDist);
                    if (nn >= MaxVertsPerEdge) nn = MaxVertsPerEdge - 1;
                    if (nverts + nn >= MaxVerts)
                        nn = MaxVerts - 1 - nverts;

                    for (int k = 0; k <= nn; k++)
                    {
                        float u = (float) k/(float) nn;
                        int pos = k*3;
                        edge[pos + 0] = inArray[vj + 0] + dx * u;
                        edge[pos + 1] = inArray[vj + 1] + dy * u;
                        edge[pos + 2] = inArray[vj + 2] + dz * u;
                        edge[pos + 1] = GetHeight(edge[pos + 0], edge[pos + 1], edge[pos + 2], cs, ics, chf.Ch, hp)*chf.Ch;
                    }

                    int[] idx = new int[MaxVertsPerEdge];
                    idx[0] = 0; idx[1] = nn;
                    int nidx = 2;
                    for (int k = 0; k < nidx-1;)
                    {
                        int a = idx[k];
                        int b = idx[k + 1];
                        int va = a*3;
                        int vb = b*3;

                        float maxd = 0;
                        int maxi = -1;
                        for (int m = a+1; m < b; m++)
                        {
                            float dev = DistancePtSeg(edge[m * 3 + 0], edge[m * 3 + 1], edge[m * 3 + 2], edge[va+0], edge[va+1], edge[va+2], edge[vb+0], edge[vb+1], edge[vb+2]);
                            if (dev > maxd)
                            {
                                maxd = dev;
                                maxi = m;
                            }
                        }

                        if (maxi != -1 && maxd > sampleMaxError*sampleMaxError)
                        {
                            for (int m = nidx; m > k; m--)
                            {
                                idx[m] = idx[m - 1];
                            }
                            idx[k + 1] = maxi;
                            nidx++;
                        }
                        else
                        {
                            k++;
                        }
                    }

                    hull[nhull++] = j;
                    if (swapped)
                    {
                        for (int k = nidx-2; k > 0; k--)
                        {
                            Array.Copy(edge, idx[k]*3, verts, nverts*3, 3);
                            hull[nhull++] = nverts;
                            nverts++;
                        }
                    }
                    else
                    {
                        for (int k = 1; k < nidx-1; k++)
                        {
                            Array.Copy(edge, idx[k]*3, verts, nverts*3, 3);
                            hull[nhull++] = nverts;
                            nverts++;
                        }
                    }
                }
            }

            edges.Resize(0);
            tris.Resize(0);

	        DelaunayHull(nverts, verts, nhull, hull, ref tris, ref edges);

            if (tris.Size == 0)
            {
                // error add default data
                for (int i = 2; i < nverts; i++)
                {
                    tris.Push(0);
                    tris.Push(i-1);
                    tris.Push(i);
                    tris.Push(0);
                }
                return true;
            }

            if (sampleDist > 0)
            {
                float[] bmin = new float[3], bmax = new float[3];
                Array.Copy(inArray, 0, bmin, 0, 3);
                Array.Copy(inArray, 0, bmax, 0, 3);

                for (int i = 1; i < nin; i++)
                {
                    bmin[0] = Math.Min(bmin[0], inArray[i*3 + 0]);
                    bmin[1] = Math.Min(bmin[1], inArray[i*3 + 1]);
                    bmin[2] = Math.Min(bmin[2], inArray[i*3 + 2]);
                    bmax[0] = Math.Max(bmax[0], inArray[i*3 + 0]);
                    bmax[1] = Math.Max(bmax[1], inArray[i*3 + 1]);
                    bmax[2] = Math.Max(bmax[2], inArray[i*3 + 2]);
                }

                int x0 = (int) Math.Floor(bmin[0]/sampleDist);
                int x1 = (int) Math.Ceiling(bmax[0]/sampleDist);
                int z0 = (int) Math.Floor(bmin[2]/sampleDist);
                int z1 = (int) Math.Ceiling(bmax[2]/sampleDist);
                samples.Resize(0);
                for (int z = z0; z < z1; z++)
                {
                    for (int x = x0; x < x1; x++)
                    {
                        float[] pt = new float[3];
                        pt[0] = x*sampleDist;
                        pt[1] = (bmax[1] + bmin[1])*0.5f;
                        pt[2] = z*sampleDist;

                        if (DistToPoly(nin, inArray, pt[0], pt[1], pt[2]) > -sampleDist/2) continue;
                        samples.Push(x);
                        samples.Push(GetHeight(pt[0], pt[1], pt[2], cs, ics, chf.Ch, hp));
                        samples.Push(z);
                        samples.Push(0);
                    }
                }

                int nsamples = samples.Size/4;
                for (int iter = 0; iter < nsamples; iter++)
                {
                    if (nverts >= MaxVerts)
                        break;

                    float[] bestpt = {0, 0, 0};
                    float bestd = 0;
                    int besti = -1;
                    for (int i = 0; i < nsamples; i++)
                    {
                        int s = i*4;
                        if (samples[s + 3] != 0) continue;
                        float[] pt = new float[3];

                        pt[0] = samples[s + 0]*sampleDist + GetJitterX(i)*cs*0.1f;
                        pt[1] = samples[s + 1]*chf.Ch;
                        pt[2] = samples[s + 2]*sampleDist + GetJitterY(i)*cs*0.1f;
                        float d = DistToTriMesh(pt[0], pt[1], pt[2], verts, nverts, tris, tris.Size/4);
                        if (d < 0) continue; // didn't hit the mesh
                        if (d > bestd)
                        {
                            bestd = d;
                            besti = i;
                            Array.Copy(pt, 0, bestpt, 0, 3);
                        }
                    }
                    if (bestd <= sampleMaxError || besti == -1)
                        break;
                    samples[besti*4 + 3] = 1;
                    Array.Copy(bestpt, 0, verts, nverts*3, 3);
                    nverts++;

                    edges.Resize(0);
                    tris.Resize(0);
                    DelaunayHull(nverts, verts, nhull, hull, ref tris, ref edges);
                }
            }

	        int ntris = tris.Size/4;
            if (ntris > MaxTris)
            {
                // error, shrink
                tris.Resize(MaxTris*4);
            }
	        return true;
	    }

	    private void DelaunayHull(int npts, float[] pts, int nhull, int[] hull, ref IntArray tris, ref IntArray edges)
	    {
	        int nfaces = 0;
	        int nedges = 0;
	        int maxEdges = npts*10;
            edges.Resize(maxEdges*4);

	        for (int i = 0, j = nhull-1; i < nhull; j=i++)
	        {
	            AddEdge(ref edges, ref nedges, maxEdges, hull[j], hull[i], Hull, Undef);
	        }

	        int currentEdge = 0;
            while (currentEdge < nedges)
            {
                if (edges[currentEdge*4+2] == Undef)
                    CompleteFacet(pts, npts, ref edges, ref nedges, maxEdges, ref nfaces, currentEdge);
                if (edges[currentEdge*4 + 3] == Undef)
                    CompleteFacet(pts, npts, ref edges, ref nedges, maxEdges, ref nfaces, currentEdge);
                currentEdge++;
            }

            tris.Resize(nfaces*4);
	        for (int i = 0; i < nfaces*4; i++)
	        {
	            tris[i] = -1;
	        }

	        for (int i = 0; i < nedges; i++)
	        {
	            int e = i*4;
                if (edges[e + 3] >= 0)
                {
                    int t = edges[e + 3]*4;
                    if (tris[t + 0] == -1)
                    {
                        tris[t + 0] = edges[e + 0];
                        tris[t + 1] = edges[e + 1];
                    }
                    else if (tris[t + 0] == edges[e + 1])
                    {
                        tris[t + 2] = edges[e + 0];
                    }
                    else if (tris[t + 1] == edges[e + 0])
                    {
                        tris[t + 2] = edges[e + 1];
                    }
                }
                if (edges[e + 2] >= 0)
                {
                    int t = edges[e + 2]*4;
                    if (tris[t + 0] == -1)
                    {
                        tris[t + 0] = edges[e + 1];
                        tris[t + 1] = edges[e + 0];
                    }
                    else if (tris[t + 0] == edges[e + 0])
                    {
                        tris[t + 2] = edges[e + 1];
                    }
                    else if (tris[t + 1] == edges[e + 1])
                    {
                        tris[t + 2] = edges[e + 0];
                    }
                }
	        }

	        for (int i = 0; i < tris.Size/4; i++)
	        {
	            int t = i*4;
                if (tris[t + 0] == -1 || tris[t + 1] == -1 || tris[t + 2] == -1)
                {
                    // error
                    tris[t + 0] = tris[tris.Size - 4];
                    tris[t + 1] = tris[tris.Size - 3];
                    tris[t + 2] = tris[tris.Size - 2];
                    tris[t + 3] = tris[tris.Size - 1];
                    tris.Resize(tris.Size-4);
                    --i;
                }
	        }
	    }

	    private void CompleteFacet(float[] pts, int npts, ref IntArray edges, ref int nedges, int maxEdges, ref int nfaces, int e)
	    {
	        float EPS = 1e-5f;

	        int edge = e*4;

	        int s = 0, t = 0;
            if (edges[edge + 2] == Undef)
            {
                s = edges[edge + 0];
                t = edges[edge + 1];
            }
            else if (edges[edge + 3] == Undef)
            {
                s = edges[edge + 1];
                t = edges[edge + 0];
            }
            else
            {
                return;
            }

	        int pt = npts;
	        float[] c = {0, 0, 0};
	        float r = -1;
	        for (int u = 0; u < npts; u++)
	        {
	            if (u == s || u == t) continue;

                if (VCross2(pts[s*3 + 0], pts[s*3 + 1], pts[s*3 + 2], pts[t*3 + 0], pts[t*3 + 1], pts[t*3 + 2], pts[u*3 + 0], pts[u*3 + 1], pts[u*3 + 2]) > EPS)
                {
                    if (r < 0)
                    {
                        pt = u;
                        CircumCircle(pts[s*3 + 0], pts[s*3 + 1], pts[s*3 + 2], pts[t*3 + 0], pts[t*3 + 1], pts[t*3 + 2],
                                     pts[u*3 + 0], pts[u*3 + 1], pts[u*3 + 2], ref c, ref r);
                        continue;
                    }
                    float d = VDist2(c[0], c[1], c[2], pts[u*3 + 0], pts[u*3 + 1], pts[u*3 + 2]);
                    float tol = 0.001f;
                    if (d > r*(1 + tol))
                    {
                        continue;
                    }
                    else if (d < r*(1 - tol))
                    {
                        pt = u;
                        CircumCircle(pts[s*3 + 0], pts[s*3 + 1], pts[s*3 + 2], pts[t*3 + 0], pts[t*3 + 1], pts[t*3 + 2],
                                     pts[u*3 + 0], pts[u*3 + 1], pts[u*3 + 2], ref c, ref r);
                    }
                    else
                    {
                        if (OverlapEdges(pts, edges, nedges, s, u))
                            continue;
                        if (OverlapEdges(pts, edges, nedges, t, u))
                            continue;

                        pt = u;
                        CircumCircle(pts[s * 3 + 0], pts[s * 3 + 1], pts[s * 3 + 2], pts[t * 3 + 0], pts[t * 3 + 1], pts[t * 3 + 2],
                                     pts[u * 3 + 0], pts[u * 3 + 1], pts[u * 3 + 2], ref c, ref r);
                    }
                }
	        }

            if (pt < npts)
            {
                UpdateLeftFace(ref edges, e*4, s, t, nfaces);

                e = FindEdge(edges, nedges, pt, s);
                if (e == Undef)
                {
                    AddEdge(ref edges, ref nedges, maxEdges, pt, s, nfaces, Undef);
                }
                else
                {
                    UpdateLeftFace(ref edges, e*4, pt, s, nfaces);
                }

                e = FindEdge(edges, nedges, t, pt);
                if (e == Undef)
                {
                    AddEdge(ref edges, ref nedges, maxEdges, t, pt, nfaces, Undef);
                }
                else
                {
                    UpdateLeftFace(ref edges, e*4, t, pt, nfaces);
                }

                nfaces++;
            }
            else
            {
                UpdateLeftFace(ref edges, e*4, s, t, Hull);
            }
	    }

	    private bool OverlapEdges(float[] pts, IntArray edges, int nedges, int s1, int t1)
	    {
	        for (int i = 0; i < nedges; i++)
	        {
	            int s0 = i*4 + 0;
	            int t0 = i*4 + 1;

	            if (edges[s0] == s1 || edges[s0] == t1 || edges[t0] == s1 || edges[t0] == t1)
	                continue;
	            if (OverlapSegSeg2d(pts[edges[s0]*3 + 0],   pts[edges[s0]*3 + 1],   pts[edges[s0]*3 + 2],
                                    pts[edges[t0]*3 + 0],   pts[edges[t0]*3 + 1],   pts[edges[t0]*3 + 2],
	                                pts[s1*3 + 0],          pts[s1*3 + 1],          pts[s1*3 + 1],
                                    pts[t1*3 + 0],          pts[t1*3 + 1],          pts[t1*3 + 1]))
	                return true;
	        }
	        return false;
	    }

	    private bool OverlapSegSeg2d(float ax, float ay, float az, float bx, float by, float bz, float cx, float cy, float cz, float dx, float dy, float dz)
	    {
	        float a1 = VCross2(ax, ay, az, bx, by, bz, dx, dy, dz);
	        float a2 = VCross2(ax, ay, az, bx, by, bz, cx, cy, cz);
            if (a1*a2 < 0.0f)
            {
                float a3 = VCross2(cx, cy, cz, dx, dy, dz, ax, ay, az);
                float a4 = a3 + a2 - a1;
                if (a3*a4 < 0.0f)
                    return true;
            }
	        return false;
	    }

	    private bool CircumCircle(float p1x, float p1y, float p1z, float p2x, float p2y, float p2z, float p3x, float p3y, float p3z, ref float[] c, ref float r)
	    {
	        float EPS = 1e-6f;

	        float cp = VCross2(p1x, p1y, p1z, p2x, p2y, p2z, p3x, p3y, p3z);
            if (Math.Abs(cp) > EPS)
            {
                float p1Sq = Dot(p1x, p1y, p1z, p1x, p1y, p1z);
                float p2Sq = Dot(p2x, p2y, p2z, p2x, p2y, p2z);
                float p3Sq = Dot(p3x, p3y, p3z, p3x, p3y, p3z);
                c[0] = (p1Sq*(p2z - p3z) + p2Sq*(p3z - p1z) + p3Sq*(p1z - p2z))/(2*cp);
                c[2] = (p1Sq*(p3x - p2x) + p2Sq*(p1x - p3x) + p3Sq*(p2x - p1x))/(2*cp);
                r = VDist2(c[0], c[1], c[2], p1x, p1y, p1z);
				return true;
            }

	        c[0] = p1x;
	        c[2] = p1z;
	        r = 0;
	        return false;
	    }

	    private float VDist2(float px, float py, float pz, float qx, float qy, float qz)
	    {
	        return (float)Math.Sqrt(VDistSq2(px, py, pz, qx, qy, qz));
	    }

	    private float VDistSq2(float px, float py, float pz, float qx, float qy, float qz)
	    {
	        float dx = qx - px;
	        float dy = qz - pz;
	        return dx*dx + dy*dy;
	    }

	    private float VCross2(float p1X, float p1Y, float p1Z, float p2X, float p2Y, float p2Z, float p3X, float p3Y, float p3Z)
	    {
	        float u1 = p2X - p1X;
	        float v1 = p2Z - p1Z;
	        float u2 = p3X - p1X;
	        float v2 = p3Z - p1Z;
	        return u1*v2 - v1*u2;
	    }

	    private void UpdateLeftFace(ref IntArray edges, int e, int s, int t, int f)
	    {
            if (edges[e + 0] == s && edges[e + 1] == t && edges[e + 2] == Undef)
                edges[e + 2] = f;
            else if (edges[e + 1] == s && edges[e + 0] == t && edges[e + 3] == Undef)
                edges[e + 3] = f;
	    }

	    private int AddEdge(ref IntArray edges, ref int nedges, int maxEdges, int s, int t, int l, int r)
	    {
	        if (nedges >= maxEdges)
	        {
	            // error
	            return Undef;
	        }

	        int e = FindEdge(edges, nedges, s, t);
            if (e == Undef)
            {
                int edge = nedges*4;
                edges[edge + 0] = s;
                edges[edge + 1] = t;
                edges[edge + 2] = l;
                edges[edge + 3] = r;
                return nedges++;
            }
	        return Undef;
	    }

	    private int FindEdge(IntArray edges, int nedges, int s, int t)
	    {
	        for (int i = 0; i < nedges; i++)
	        {
	            int e = i*4;
	            if ((edges[e + 0] == s && edges[e + 1] == t) || (edges[e + 0] == t && edges[e + 1] == s))
	                return i;
	        }
	        return Undef;
	    }

	    private float DistancePtSeg(float ptx, float pty, float ptz, float px, float py, float pz, float qx, float qy, float qz)
	    {
	        float pqx = qx - px;
	        float pqy = qy - py;
	        float pqz = qz - pz;
	        float dx = ptx - px;
	        float dy = pty - py;
	        float dz = ptz - pz;
	        float d = pqx*pqx + pqy*pqy + pqz*pqz;
	        float t = pqx*dx + pqy*dy + pqz*dz;
	        if (d > 0)
	            t /= d;
            if (t < 0)
                t = 0;
            else if (t > 1)
                t = 1;

	        dx = px + t*pqx - ptx;
	        dy = py + t*pqy - pty;
	        dz = pz + t*pqz - ptz;

	        return dx*dx + dy*dy + dz*dz;
	    }

	    private float DistToPoly(int nvert, float[] verts, float px, float py, float pz)
	    {
	        float dmin = float.MaxValue;
	        int i = 0, j = 0;
            bool c = false;
	        for (i = 0, j = nvert-1; i < nvert; j=i++)
	        {
	            int vi = i*3;
	            int vj = j*3;
	            if (((verts[vi + 2] > pz) != (verts[vj + 2] > pz)) &&
	                (px < (verts[vj + 0] - verts[vi + 0])*(pz - verts[vi + 2])/(verts[vj + 2] - verts[vi + 2]) + verts[vi + 0]))
	                c = !c;
	            dmin = Math.Min(dmin, DistancePtSeg2d(new[] {px, py, pz}, 0, verts, vj, verts, vi));
	        }
	        return c ? -dmin : dmin;
	    }

	    private int GetHeight(float fx, float fy, float fz, float cs, float ics, float ch, HeightPatch hp)
	    {
	        int ix = (int) Math.Floor(fx*ics + 0.01f);
	        int iz = (int) Math.Floor(fz*ics + 0.01f);
	        ix = Math.Max(0, Math.Min(hp.Width, ix - hp.XMin));
	        iz = Math.Max(0, Math.Min(hp.Height, iz - hp.YMin));
	        int h = hp.Data[ix + iz*hp.Width];
            if (h == UnsetHeight)
            {
                int[] offset = {-1, 0, -1, -1, 0, -1, 1, -1, 1, 0, 1, 1, 0, 1, -1, 1};
                float dmin = float.MaxValue;
                for (int i = 0; i < 8; i++)
                {
                    int nx = ix + offset[i*2 + 0];
                    int nz = iz + offset[i*2 + 1];
                    if (nx < 0 || nz < 0 || nx >= hp.Width || nz >= hp.Height) continue;
                    int nh = hp.Data[nx + nz*hp.Width];
                    if (nh == UnsetHeight) continue;

                    float d = Math.Abs(nh*ch - fy);
                    if (d < dmin)
                    {
                        h = nh;
                        dmin = d;
                    }
                }
            }
	        return h;
	    }

	    private float GetJitterX(int i)
	    {
	        return (((int) (i*0x8da6b343) & 0xffff)/65535.0f*2.0f) - 1.0f;
	    }

	    private float GetJitterY(int i)
	    {
	        return (((int)(i*0xd8163841) & 0xffff)/65535.0f*2.0f) - 1.0f;
	    }

	    private float DistToTriMesh(float px, float py, float pz, float[] verts, int nverts, IntArray tris, int ntris)
	    {
	        float dmin = float.MaxValue;
	        for (int i = 0; i < ntris; i++)
	        {
                int va = tris[i * 4 + 0] * 3;
                int vb = tris[i * 4 + 1] * 3;
                int vc = tris[i * 4 + 2] * 3;
	            float d = DistPtTri(px, py, pz, verts[va + 0], verts[va + 1], verts[va + 2], verts[vb + 0], verts[vb + 1],
	                                verts[vb + 2], verts[vc + 0], verts[vc + 1], verts[vc + 2]);
	            if (d < dmin)
	                dmin = d;
	        }
	        if (dmin == float.MaxValue) return -1;
	        return dmin;
	    }

	    private float DistPtTri(float px, float py, float pz, float ax, float ay, float az, float bx, float by, float bz, float cx, float cy, float cz)
	    {
            Sub(ref DistPtTriV0, cx, cy, cz, ax, ay, az);
            Sub(ref DistPtTriV1, bx, by, bz, ax, ay, az);
            Sub(ref DistPtTriV2, px, py, pz, ax, ay, az);

            float dot00 = Dot(DistPtTriV0, DistPtTriV0);
            float dot01 = Dot(DistPtTriV0, DistPtTriV1);
            float dot02 = Dot(DistPtTriV0, DistPtTriV2);
            float dot11 = Dot(DistPtTriV1, DistPtTriV1);
            float dot12 = Dot(DistPtTriV1, DistPtTriV2);

	        float invDenom = 1.0f/(dot00*dot11 - dot01*dot01);
	        float u = (dot11*dot02 - dot01*dot12)*invDenom;
	        float v = (dot00*dot12 - dot01*dot02)*invDenom;

	        float EPS = 1e-4f;
            if (u >= -EPS && v >= -EPS && (u + v) <= 1 + EPS)
            {
                float y = ay + DistPtTriV0[1] * u + DistPtTriV1[1] * v;
                return Math.Abs(y - py);
            }
	        return float.MaxValue;
	    }

	    private float Dot(float[] a, float[] b)
	    {
	        return a[0]*b[0] + a[2]*b[2];
	    }

        private float Dot(float ax, float ay, float az, float bx, float by, float bz)
        {
            return ax*bx + az*bz;
        }

	    private void Sub(ref float[] dest, float v1x, float v1y, float v1z, float v2x, float v2y, float v2z)
	    {
	        dest[0] = v1x - v2x;
	        dest[1] = v1y - v2y;
	        dest[2] = v1z - v2z;
	    }

	    private short GetTriFlags(float[] vertsa, int va, float[] vertsb, int vb, float[] vertsc, int vc, float[] vpoly, int npoly)
	    {
	        short flags = 0;
	        flags |= (short)(GetEdgeFlags(vertsa, va, vertsb, vb, vpoly, npoly) << 0);
	        flags |= (short)(GetEdgeFlags(vertsb, vb, vertsc, vc, vpoly, npoly) << 2);
	        flags |= (short)(GetEdgeFlags(vertsc, vc, vertsa, va, vpoly, npoly) << 4);
	        return flags;
	    }

	    private short GetEdgeFlags(float[] vertsa, int va, float[] vertsb, int vb, float[] vpoly, int npoly)
	    {
	        float thrSqr = 0.001f*0.001f;
	        for (int i = 0, j = npoly-1; i < npoly; j=i++)
	        {
	            if (DistancePtSeg2d(vertsa, va, vpoly, j*3, vpoly, i*3) < thrSqr &&
	                DistancePtSeg2d(vertsb, vb, vpoly, j*3, vpoly, i*3) < thrSqr)
	                return 1;
	        }
	        return 0;
	    }

	    private float DistancePtSeg2d(float[] vpt, int pt, float[] vp, int p, float[] vq, int q)
	    {
	        float pqx = vq[q + 0] - vp[p + 0];
	        float pqz = vq[q + 2] - vp[p + 2];
	        float dx = vpt[pt + 0] - vp[p + 0];
	        float dz = vpt[pt + 2] - vp[p + 2];
	        float d = pqx*pqx + pqz*pqz;
	        float t = pqx*dx + pqz*dz;
	        if (d > 0)
	            t /= d;
            if (t < 0)
                t = 0;
            else if (t > 1)
                t = 1;

	        dx = vp[p + 0] + t*pqx - vpt[pt + 0];
	        dz = vp[p + 2] + t*pqz - vpt[pt + 2];

	        return dx*dx + dz*dz;
	    }
	}
}
