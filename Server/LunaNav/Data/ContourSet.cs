using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class ContourSet
	{
        public Contour[] Conts { get; set; }
        public int NConts { get; set; }
        public float[] BMin { get; set; }
        public float[] BMax { get; set; }
        public float Cs { get; set; }
        public float Ch { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BorderSize { get; set; }
	    public static int ContourRegMask = 0xffff;
	    public static int BorderVertex =  0x10000;
	    public static int AreaBorder =    0x20000;

        public ContourSet(CompactHeightfield cfh, float maxError, int maxEdgeLen, int buildFlags = BuildContourFlags.ContourTessWallEdges)
        {
            int w = cfh.Width;
            int h = cfh.Height;
            int borderSize = cfh.BorderSize;

            BMin = new float[3];
            BMax = new float[3];
            Array.Copy(cfh.BMin, BMin, 3);
            Array.Copy(cfh.BMax, BMax, 3);

            if (borderSize > 0)
            {
                float pad = borderSize*cfh.Cs;
                BMin[0] += pad;
                BMin[2] += pad;
                BMax[0] -= pad;
                BMax[2] -= pad;
            }

            Cs = cfh.Cs;
            Ch = cfh.Ch;
            Width = cfh.Width - cfh.BorderSize*2;
            Height = cfh.Height - cfh.BorderSize*2;
            BorderSize = cfh.BorderSize;
            int maxContours = Math.Max(cfh.MaxRegions, 8);
            Conts = new Contour[maxContours];
            for (int i = 0; i < maxContours; i++)
            {
                Conts[i] = new Contour();
            }
            NConts = 0;

            char[] flags = new char[cfh.SpanCount];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    CompactCell c = cfh.Cells[x + y*w];
                        for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
                        {
                            if (i == 4782)
                            {
                                int z = 0;
                            }
                            int res = 0;
                            CompactSpan s = cfh.Spans[i];
                            if (s.Reg == 0 || (s.Reg & CompactHeightfield.BorderReg) != 0)
                            {
                                flags[i] = (char) 0;
                                continue;
                            }
                            for (int dir = 0; dir < 4; dir++)
                            {
                                int r = 0;
                                if (s.GetCon(dir) != CompactHeightfield.NotConnected)
                                {
                                    int ax = x + Helper.GetDirOffsetX(dir);
                                    int ay = y + Helper.GetDirOffsetY(dir);
                                    int ai = (int) cfh.Cells[ax + ay*w].Index + s.GetCon(dir);
                                    r = cfh.Spans[ai].Reg;
                                }
                                if (r == cfh.Spans[i].Reg)
                                {
                                    res |= (1 << dir);
                                }
                            }
                            flags[i] = (char) (res ^ 0xf);
                        }
                }
            }

            IntArray verts = new IntArray(256);
            IntArray simplified = new IntArray(64);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    CompactCell c = cfh.Cells[x + y*w];
                        for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
                        {
                            if (flags[i] == 0 || flags[i] == 0xf)
                            {
                                flags[i] = (char) 0;
                                continue;
                            }

                            int reg = cfh.Spans[i].Reg;
                            if (reg == 0 || (reg & CompactHeightfield.BorderReg) != 0) continue;
                            uint area = cfh.Areas[i];

                            verts.Resize(0);
                            simplified.Resize(0);

                            WalkContour(x, y, i, cfh, ref flags, ref verts);

                            SimplifyContour(ref verts, ref simplified, maxError, maxEdgeLen, buildFlags);
                            RemoveDegenerateSegments(ref simplified);

                            if ((simplified.Size/4) >= 3)
                            {
                                // We need more space than we allocated...
                                if (NConts >= maxContours)
                                {
                                    int oldMax = maxContours;
                                    maxContours *= 2;
                                    Contour[] newConts = new Contour[maxContours];
                                    for (int j = 0; j < maxContours; j++)
                                    {
                                        newConts[j] = new Contour();
                                    }
                                    for (int j = 0; j < NConts; j++)
                                    {
                                        newConts[j] = Conts[j];
                                    }
                                    Conts = newConts;
                                }
                                Contour cont = Conts[NConts++];
                                cont.NVerts = simplified.Size/4;
                                cont.Verts = new int[cont.NVerts*4];
                                Array.Copy(simplified.ToArray(), cont.Verts, cont.NVerts*4);
                                if (borderSize > 0)
                                {
                                    for (int j = 0; j < cont.NVerts; j++)
                                    {
                                        int v = j*4;
                                        cont.Verts[v + 0] -= borderSize;
                                        cont.Verts[v + 2] -= borderSize;
                                    }
                                }

                                cont.NRVerts = verts.Size/4;
                                cont.RVerts = new int[cont.NRVerts*4];
                                Array.Copy(verts.ToArray(), cont.RVerts, cont.NRVerts*4);
                                if (borderSize > 0)
                                {
                                    for (int j = 0; j < cont.NRVerts; j++)
                                    {
                                        int v = j*4;
                                        cont.RVerts[v + 0] -= borderSize;
                                        cont.RVerts[v + 2] -= borderSize;
                                    }
                                }

                                cont.Reg = reg;
                                cont.Area = (short) area;
                            }
                        }
                }
            }

            // check and merge droppings
            for (int i = 0; i < NConts; i++)
            {
                Contour cont = Conts[i];
                if (CalcAreaOfPolygon2D(cont.Verts, cont.NVerts) < 0)
                {
                    int mergeIdx = -1;
                    for (int j = 0; j < NConts; j++)
                    {
                        if (i == j) continue;
                        if (Conts[j].NVerts > 0 && Conts[j].Reg == cont.Reg)
                        {
                            if (CalcAreaOfPolygon2D(Conts[j].Verts, Conts[j].NVerts) > 0)
                            {
                                mergeIdx = j;
                                break;
                            }
                        }
                    }
                    if (mergeIdx == -1)
                    {
                        // error
                    }
                    else
                    {
                        Contour mcont = Conts[mergeIdx];
                        int ia = 0, ib = 0;
                        GetClosestIndices(mcont.Verts, mcont.NVerts, cont.Verts, cont.NVerts, ref ia, ref ib);
                        if (ia == -1 || ib == -1)
                        {
                            // bad merge
                            continue;
                        }
                        if(!MergeContours(ref mcont, ref cont, ia, ib))
                        {
                            // Merge failed
                            continue;
                        }
                    }
                }
            }
        }

	    private void WalkContour(int x, int y, int i, CompactHeightfield cfh, ref char[] flags, ref IntArray points)
	    {
	        char dir = (char)0;
	        while ((flags[i] & (1 << dir)) == 0)
	            dir++;

	        char startDir = dir;
			char tempDir = dir;
	        int starti = i;

	        uint area = cfh.Areas[i];
	        int iter = 0;
            while (++iter < 40000)
            {
                if ((flags[i] & (1 << dir)) > 0)
                {
                    bool isBorderVertex = false;
                    bool isAreaBorder = false;
                    int px = x;
                    int py = GetCornerHeight(x, y, i, tempDir, cfh, ref isBorderVertex);
                    int pz = y;

                    if (dir == (char) 0)
                    {
                        pz++;
                    }
                    else if (dir == (char) 1)
                    {
                        px++;
                        pz++;
                    }
                    else if (dir == (char) 2)
                    {
                        px++;
                    }

                    int r = 0;
                    CompactSpan s = cfh.Spans[i];
                    if (s.GetCon(dir) != CompactHeightfield.NotConnected)
                    {
                        int ax = x + Helper.GetDirOffsetX(dir);
                        int ay = y + Helper.GetDirOffsetY(dir);
                        int ai = (int) cfh.Cells[ax + ay*cfh.Width].Index + s.GetCon(dir);
                        r = cfh.Spans[ai].Reg;
                        if (area != cfh.Areas[ai])
                        {
                            isAreaBorder = true;
                        }
                    }
                    if (isBorderVertex)
                        r |= BorderVertex;
                    if (isAreaBorder)
                        r |= AreaBorder;

                    points.Push(px);
                    points.Push(py);
                    points.Push(pz);
                    points.Push(r);

                    flags[i] &= (char)~(1 << dir);
                    dir = (char)((dir + 1) & 0x3); // rotate CW
                }
                else
                {
                    int ni = -1;
                    int nx = x + Helper.GetDirOffsetX(dir);
                    int ny = y + Helper.GetDirOffsetY(dir);
                    CompactSpan s = cfh.Spans[i];
                    if (s.GetCon(dir) != CompactHeightfield.NotConnected)
                    {
                        CompactCell nc = cfh.Cells[nx + ny*cfh.Width];
                        ni = (int) nc.Index + s.GetCon(dir);
                    }
                    if (ni == -1)
                    {
                        // error
                        return;
                    }
                    x = nx;
                    y = ny;
                    i = ni;
                    dir = (char)((dir + 3) & 0x3);
                }
                if (starti == i && startDir == dir)
                {
                    break;
                }
            }
	    }

	    private int GetCornerHeight(int x, int y, int i, int dir, CompactHeightfield cfh, ref bool isBorderVertex)
	    {
	        CompactSpan s = cfh.Spans[i];
	        int ch = s.Y;
	        int dirp = (dir + 1) & 0x3;

            uint[] regs = {0,0,0,0};

	        regs[0] = (uint)(cfh.Spans[i].Reg | (cfh.Areas[i] << 16));

            if (s.GetCon(dir) != CompactHeightfield.NotConnected)
            {
                int ax = x + Helper.GetDirOffsetX(dir);
                int ay = y + Helper.GetDirOffsetY(dir);
                int ai = (int) cfh.Cells[ax + ay*cfh.Width].Index + s.GetCon(dir);
                CompactSpan aspan  = cfh.Spans[ai];
                ch = Math.Max(ch, aspan.Y);
                regs[1] = (uint)(cfh.Spans[ai].Reg | (cfh.Areas[ai] << 16));
                if (aspan.GetCon(dirp) != CompactHeightfield.NotConnected)
                {
                    int ax2 = ax + Helper.GetDirOffsetX(dirp);
                    int ay2 = ay + Helper.GetDirOffsetY(dirp);
                    int ai2 = (int) cfh.Cells[ax2 + ay2*cfh.Width].Index + aspan.GetCon(dirp);
                    CompactSpan as2 = cfh.Spans[ai2];
                    ch = Math.Max(ch, as2.Y);
                    regs[2] = (uint)(cfh.Spans[ai2].Reg | (cfh.Areas[ai2] << 16));
                }
            }
            if (s.GetCon(dirp) != CompactHeightfield.NotConnected)
            {
                int ax = x + Helper.GetDirOffsetX(dirp);
                int ay = y + Helper.GetDirOffsetY(dirp);
                int ai = (int)cfh.Cells[ax + ay * cfh.Width].Index + s.GetCon(dirp);
                CompactSpan aspan = cfh.Spans[ai];
                ch = Math.Max(ch, aspan.Y);
                regs[3] = (uint)(cfh.Spans[ai].Reg | (cfh.Areas[ai] << 16));
                if (aspan.GetCon(dir) != CompactHeightfield.NotConnected)
                {
                    int ax2 = ax + Helper.GetDirOffsetX(dir);
                    int ay2 = ay + Helper.GetDirOffsetY(dir);
                    int ai2 = (int)cfh.Cells[ax2 + ay2 * cfh.Width].Index + aspan.GetCon(dir);
                    CompactSpan as2 = cfh.Spans[ai2];
                    ch = Math.Max(ch, as2.Y);
                    regs[2] = (uint)(cfh.Spans[ai2].Reg | (cfh.Areas[ai2] << 16));
                }
            }

	        for (int j = 0; j < 4; j++)
	        {
	            int a = j;
	            int b = (j + 1) & 0x3;
	            int c = (j + 2) & 0x3;
	            int d = (j + 3) & 0x3;

	            bool twoSameExts = (regs[a] & regs[b] & CompactHeightfield.BorderReg) != 0 && regs[a] == regs[b];
                bool twoInts = ((regs[c] | regs[d]) & CompactHeightfield.BorderReg) == 0;
	            bool intsSameArea = (regs[c] >> 16) == (regs[d] >> 16);
	            bool noZeros = regs[a] != 0 && regs[b] != 0 && regs[c] != 0 && regs[d] != 0;
                if (twoSameExts && twoInts && intsSameArea && noZeros)
                {
                    isBorderVertex = true;
                    break;
                }

	        }

	        return ch;
	    }

	    private void SimplifyContour(ref IntArray points, ref IntArray simplified, float maxError, int maxEdgeLen, int buildFlags)
	    {
	        bool hasConnections = false;
	        for (int i = 0; i < points.Size; i+= 4)
	        {
	            if ((points[i + 3] & ContourRegMask) != 0)
	            {
	                hasConnections = true;
	                break;
	            }
	        } 
	        if (hasConnections)
	        {
	            for (int i = 0, ni = points.Size/4; i < ni; i++)
	            {
	                int ii = (i + 1)%ni;
	                bool differentRegs = (points[i*4 + 3] & ContourRegMask) != (points[ii*4 + 3] & ContourRegMask);
	                bool areaBorders = (points[i*4 + 3] & AreaBorder) != (points[ii*4 + 3] & AreaBorder);
                    if (differentRegs || areaBorders)
                    {
                        simplified.Push(points[i * 4 + 0]);
                        simplified.Push(points[i * 4 + 1]);
                        simplified.Push(points[i * 4 + 2]);
                        simplified.Push(i);
                    }
	            }
	        }

	        if (simplified.Size == 0)
	        {
	            int llx = points[0];
	            int lly = points[1];
	            int llz = points[2];
	            int lli = 0;
	            int urx = points[0];
	            int ury = points[1];
	            int urz = points[2];
	            int uri = 0;
	            for (int i = 0; i < points.Size; i += 4)
	            {
	                int x = points[i + 0];
	                int y = points[i + 1];
	                int z = points[i + 2];
	                if (x < llx || (x == llx && z < llz))
	                {
	                    llx = x;
	                    lly = y;
	                    llz = z;
	                    lli = i/4;
	                }
                    if (x > urx || (x == urx && z > urz))
                    {
                        urx = x;
                        ury = y;
                        urz = z;
                        uri = i/4;
                    }
	            }
                simplified.Push(llx);
                simplified.Push(lly);
                simplified.Push(llz);
                simplified.Push(lli);

                simplified.Push(urx);
                simplified.Push(ury);
                simplified.Push(urz);
                simplified.Push(uri);
            }

	        int pn = points.Size/4;
            // go through all the points, as more points are added on to the end
	        for (int i = 0; i < simplified.Size/4;)
	        {
	            int ii = (i + 1)%(simplified.Size/4);

	            int ax = simplified[i*4 + 0];
	            int az = simplified[i*4 + 2];
	            int ai = simplified[i*4 + 3];

	            int bx = simplified[ii*4 + 0];
	            int bz = simplified[ii*4 + 2];
	            int bi = simplified[ii*4 + 3];

	            float maxd = 0;
	            int maxi = -1;
	            int ci, cinc, endi;

                if (bx > ax || (bx == ax && bz > az))
                {
                    cinc = 1;
                    ci = (ai + cinc)%pn;
                    endi = bi;
                }
                else
                {
                    cinc = pn - 1;
                    ci = (bi + cinc)%pn;
                    endi = ai;
                }

                if ((points[ci*4 + 3] & ContourRegMask) == 0 || (points[ci*4 + 3] & AreaBorder) != 0)
                {
                    while (ci != endi)
                    {
                        float d = DistancePtSeg(points[ci*4 + 0], points[ci*4 + 2], ax, az, bx, bz);
                        if (d > maxd)
                        {
                            maxd = d;
                            maxi = ci;
                        }
                        ci = (ci + cinc)%pn;
                    }
                }

                // if max deviation is larger than accepted error add new point
				float errorSqrd = maxError*maxError;
                if (maxi != -1 && maxd > (maxError*maxError))
                {
                    simplified.Resize(simplified.Size+4);
                    int n = simplified.Size/4;
                    for (int j = n - 1; j > i; --j)
                    {
                        simplified[j * 4 + 0] = simplified[(j - 1) * 4 + 0];
                        simplified[j * 4 + 1] = simplified[(j - 1) * 4 + 1];
                        simplified[j * 4 + 2] = simplified[(j - 1) * 4 + 2];
                        simplified[j * 4 + 3] = simplified[(j - 1) * 4 + 3];
                    }

                    simplified[(i + 1) * 4 + 0] = points[maxi * 4 + 0];
                    simplified[(i + 1) * 4 + 1] = points[maxi * 4 + 1];
                    simplified[(i + 1) * 4 + 2] = points[maxi * 4 + 2];
                    simplified[(i + 1) * 4 + 3] = maxi;
                }
                else
                {
                    i++;
                }
	        }

            // split too long edges
            if (maxEdgeLen > 0 &&
                (buildFlags & (BuildContourFlags.ContourTessWallEdges | BuildContourFlags.ContourTessAreaEdges)) != 0)
            {
                for (int i = 0; i < simplified.Size / 4; )
                {
                    int ii = (i + 1) % (simplified.Size / 4);

                    int ax = simplified[i * 4 + 0];
                    int az = simplified[i * 4 + 2];
                    int ai = simplified[i * 4 + 3];

                    int bx = simplified[ii * 4 + 0];
                    int bz = simplified[ii * 4 + 2];
                    int bi = simplified[ii * 4 + 3];

                    int maxi = -1;
                    int ci = (ai + 1) % pn;

                    bool tess = false;

                    if ((buildFlags & BuildContourFlags.ContourTessWallEdges) != 0 &&
                        (points[ci * 4 + 3] & ContourRegMask) == 0)
                        tess = true;
                    if ((buildFlags & BuildContourFlags.ContourTessAreaEdges) != 0 &&
                        (points[ci * 4 + 3] & AreaBorder) != 0)
                        tess = true;

                    if (tess)
                    {
                        int dx = bx - ax;
                        int dz = bz - az;
                        if (dx * dx + dz * dz > maxEdgeLen * maxEdgeLen)
                        {
                            int n = bi < ai ? (bi + pn - ai) : (bi - ai);
                            if (n > 1)
                            {
                                if (bx > ax || (bx == ax && bz > az))
                                    maxi = (int)(ai + n / 2f) % pn;
                                else
                                {
                                    maxi = (int)(ai + (n + 1) / 2f) % pn;
                                }
                            }
                        }
                    }

                    if (maxi != -1)
                    {
                        simplified.Resize(simplified.Size + 4);
                        int n = simplified.Size / 4;
                        for (int j = n - 1; j > i; j--)
                        {
                            simplified[j * 4 + 0] = simplified[(j - 1) * 4 + 0];
                            simplified[j * 4 + 1] = simplified[(j - 1) * 4 + 1];
                            simplified[j * 4 + 2] = simplified[(j - 1) * 4 + 2];
                            simplified[j * 4 + 3] = simplified[(j - 1) * 4 + 3];
                        }
                        simplified[(i + 1) * 4 + 0] = points[maxi * 4 + 0];
                        simplified[(i + 1) * 4 + 1] = points[maxi * 4 + 1];
                        simplified[(i + 1) * 4 + 2] = points[maxi * 4 + 2];
                        simplified[(i + 1) * 4 + 3] = maxi;
                    }
                    else
                    {
                        i++;
                    }
                }
            }

	        for (int i = 0; i < simplified.Size/4; i++)
	        {
	            int ai = (simplified[i*4 + 3] + 1)%pn;
	            int bi = simplified[i*4 + 3];
	            simplified[i*4 + 3] = (points[ai*4 + 3] & (ContourRegMask | AreaBorder)) |
	                                  (points[bi*4 + 3] & BorderVertex);
	        }
	    }

	    private float DistancePtSeg(int x, int z, int px, int pz, int qx, int qz)
	    {
	        float pqx = (float) (qx - px);
	        float pqz = (float) (qz - pz);
	        float dx = (float) (x - px);
	        float dz = (float) (z - pz);
	        float d = pqx*pqx + pqz*pqz;
	        float t = pqx*dx + pqz*dz;
	        if (d > 0)
	            t /= d;
            if (t < 0)
                t = 0;
            else if (t > 1)
                t = 1;

	        dx = px + t*pqx - x;
	        dz = pz + t*pqz - z;

	        return dx*dx + dz*dz;
	    }

	    private void RemoveDegenerateSegments(ref IntArray simplified)
	    {
	        for (int i = 0; i < simplified.Size/4; i++)
	        {
	            int ni = i + 1;
	            if (ni >= simplified.Size/4)
	                ni = 0;

                if (simplified[i*4 + 0] == simplified[ni*4 + 0] && simplified[i*4 + 2] == simplified[ni*4 + 2])
                {
                    for (int j = i; j < simplified.Size/4-1; j++)
                    {
                        simplified[j * 4 + 0] = simplified[(j + 1) * 4 + 0];
                        simplified[j * 4 + 1] = simplified[(j + 1) * 4 + 1];
                        simplified[j * 4 + 2] = simplified[(j + 1) * 4 + 2];
                        simplified[j * 4 + 3] = simplified[(j + 1) * 4 + 3];
                    }
                    simplified.Resize(simplified.Size-4);
                }
	        }
	    }

	    private int CalcAreaOfPolygon2D(int[] verts, int nVerts)
	    {
	        int area = 0;
	        for (int i = 0, j = nVerts-1; i < nVerts; j=i++)
	        {
	            int vi = i*4;
	            int vj = j*4;
                area += verts[vi + 0] * verts[vj + 2] - verts[vj + 0] * verts[vi + 2];
	        }
	        return (area + 1)/2;
	    }

	    private void GetClosestIndices(int[] vertsa, int nvertsa, int[] vertsb, int nvertsb, ref int ia, ref int ib)
	    {
	        int closestDist = int.MaxValue;
	        ia = -1;
	        ib = -1;
	        for (int i = 0; i < nvertsa; i++)
	        {
	            int iNext = (i + 1)%nvertsa;
	            int iPrev = (i + nvertsa - 1)%nvertsa;
	            int va = i*4;
	            int van = iNext*4;
	            int vap = iPrev*4;

	            for (int j = 0; j < nvertsb; j++)
	            {
	                int vb = j*4;
                    // vb must be "in front" of va
                    if (ILeft(vertsa, vap, vertsa, va, vertsb, vb) && ILeft(vertsa, va, vertsa, van, vertsb, vb))
                    {
                        int dx = vertsb[vb + 0] - vertsa[va + 0];
                        int dz = vertsb[vb + 2] - vertsa[va + 2];
                        int d = dx*dx + dz*dz;
                        if (d < closestDist)
                        {
                            ia = i;
                            ib = j;
                            closestDist = d;
                        }
                    }
	            }
	        }
	    }

	    private bool ILeft(int[] a, int ia, int[] b, int ib, int[] c, int ic)
	    {
	        return (b[ib + 0] - a[ia + 0])*(c[ic + 2] - a[ia + 2]) - (c[ic + 0] - a[ia + 0])*(b[ib + 2] - a[ia + 2]) <= 0;
	    }

	    private bool MergeContours(ref Contour ca, ref Contour cb, int ia, int ib)
	    {
	        int maxVerts = ca.NVerts + cb.NVerts + 2;
            int[] verts = new int[maxVerts*4];
	        int nv = 0;

	        for (int i = 0; i < ca.NVerts; i++)
	        {
	            int dst = nv*4;
	            int src = ((ia + i)%ca.NVerts)*4;
                verts[dst + 0] = ca.Verts[src + 0];
                verts[dst + 1] = ca.Verts[src + 1];
                verts[dst + 2] = ca.Verts[src + 2];
                verts[dst + 3] = ca.Verts[src + 3];
	            nv++;
	        }

	        for (int i = 0; i < cb.NVerts; i++)
	        {
	            int dst = nv*4;
	            int src = ((ib + i)%cb.NVerts)*4;
                verts[dst + 0] = cb.Verts[src + 0];
                verts[dst + 1] = cb.Verts[src + 1];
                verts[dst + 2] = cb.Verts[src + 2];
                verts[dst + 3] = cb.Verts[src + 3];
	            nv++;
	        }

	        ca.Verts = verts;
	        ca.NVerts = nv;
	        cb.Verts = null;
	        cb.NVerts = 0;

	        return true;
	    }
	}
}
