using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class CompactHeightfield
	{
        public int Width { get; set; }
        public int Height { get; set; }
        public int SpanCount { get; set; }
        public int WalkableHeight { get; set; }
        public int WalkableClimb { get; set; }
        public int BorderSize { get; set; }
        public int MaxDistance { get; set; }
        public int MaxRegions { get; set; }
        public float[] BMin { get; set; }
        public float[] BMax { get; set; }
        public float Cs { get; set; }
        public float Ch { get; set; }
        public CompactCell[] Cells { get; set; }
        public CompactSpan[] Spans { get; set; }
	    public int[] Dist { get; set; }
        public uint[] Areas { get; set; }

	    public static int NotConnected = 0x3f;
	    public static int BorderReg = 0x8000;

        public CompactHeightfield(int walkableHeight, int walkableClimb, HeightField hf)
        {
            int w = hf.Width;
            int h = hf.Height;
            int spanCount = hf.GetHeightFieldSpanCount();

            Width = w;
            Height = h;
            SpanCount = spanCount;
            WalkableHeight = walkableHeight;
            WalkableClimb = walkableClimb;
            MaxRegions = 0;
            BMin = new float[3];
            BMax = new float[3];
            Array.Copy(hf.Bmin, 0, BMin, 0, 3);
            Array.Copy(hf.Bmax, 0, BMax, 0, 3);
            BMax[1] += walkableHeight*hf.Ch;
            Cs = hf.Cs;
            Ch = hf.Ch;
            Cells = new CompactCell[w*h];
            Spans = new CompactSpan[spanCount];
            Areas = new uint[spanCount];
            int MaxHeight = 0xffff;

            int idx = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Span s = hf.Spans[x + y*w];
                    if (s == null) continue;
                    
                    Cells[x + y * w].Index = (uint)idx;
                    Cells[x + y * w].Count = 0;

                    while (s != null)
                    {
                        if (s.Area != HeightField.NullArea)
                        {
                            int bot = (int) s.SMax;
                            int top = s.Next != null ? (int) s.Next.SMin : MaxHeight;
                            Spans[idx].Y = Math.Max(0, Math.Min(bot, MaxHeight));
                            Spans[idx].H = Math.Max(0, Math.Min(top - bot, 255));
                            Areas[idx] = s.Area;
                            idx++;
                            Cells[x + y * w].Count++;
                        }
                        s = s.Next;
                    }
                }
            }

            int MaxLayers = NotConnected - 1;
            int tooHighNeighbor = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    CompactCell c = Cells[x + y*w];
                        for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
                        {
                            for (int dir = 0; dir < 4; dir++)
                            {
                                Spans[i].SetCon(dir, NotConnected);
                                int nx = x + Helper.GetDirOffsetX(dir);
                                int ny = y + Helper.GetDirOffsetY(dir);

                                if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                                    continue;

                                CompactCell nc = Cells[nx + ny*w];
                                    for (int k = (int) nc.Index, nk = (int) (nc.Index + nc.Count); k < nk; k++)
                                    {
                                        CompactSpan ns = Spans[k];
                                        int bot = Math.Max(Spans[i].Y, ns.Y);
                                        int top = Math.Min(Spans[i].Y + Spans[i].H, ns.Y + ns.H);

                                        if ((top - bot) >= walkableHeight && Math.Abs(ns.Y - Spans[i].Y) <= walkableClimb)
                                        {
                                            int lidx = k - (int) nc.Index;
                                            if (lidx < 0 || lidx > MaxLayers)
                                            {
                                                tooHighNeighbor = Math.Max(tooHighNeighbor, lidx);
                                                continue;
                                            }
                                            Spans[i].SetCon(dir, lidx);
                                            break;
                                        }
                                    }
                            }
                        }
                }
            }
        }

	    public bool ErodeWalkableArea(int radius)
	    {
	        int w = Width;
	        int h = Height;

            short[] dist = new short[SpanCount];
	        for (int i = 0; i < SpanCount; i++)
	        {
	            dist[i] = 0xff;
	        }

	        for (int y = 0; y < h; y++)
	        {
	            for (int x = 0; x < w; x++)
	            {
	                CompactCell c = Cells[x + y*w];

	                    for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
	                    {
	                        if (Areas[i] == HeightField.NullArea)
	                        {
	                            dist[i] = 0;
	                        }
	                        else
	                        {
	                            CompactSpan s = Spans[i];
	                            int nc = 0;
	                            for (int dir = 0; dir < 4; dir++)
	                            {
	                                if (s.GetCon(dir) != NotConnected)
	                                {
	                                    int nx = x + Helper.GetDirOffsetX(dir);
	                                    int ny = y + Helper.GetDirOffsetY(dir);
	                                    int nidx = (int) Cells[nx + ny*w].Index + s.GetCon(dir);
	                                    if (Areas[nidx] != HeightField.NullArea)
	                                    {
	                                        nc++;
	                                    }
	                                }
	                            }
	                            if (nc != 4)
	                                dist[i] = 0;
	                        }
	                    }
	            }
	        }
	        short nd;
	        for (int y = 0; y < h; y++)
	        {
	            for (int x = 0; x < w; x++)
	            {
	                CompactCell c = Cells[x + y*w];
	                    for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
	                    {
	                        CompactSpan s = Spans[i];

	                        if (s.GetCon(0) != NotConnected)
	                        {
	                            int ax = x + Helper.GetDirOffsetX(0);
	                            int ay = y + Helper.GetDirOffsetY(0);
	                            int ai = (int) Cells[ax + ay*w].Index + s.GetCon(0);
	                            CompactSpan aspan = Spans[ai];
	                            nd = (short) Math.Min((int) dist[ai] + 2, 255);
	                            if (nd < dist[i])
	                                dist[i] = nd;

	                            if (aspan.GetCon(3) != NotConnected)
	                            {
	                                int aax = ax + Helper.GetDirOffsetX(3);
	                                int aay = ay + Helper.GetDirOffsetY(3);
	                                int aai = (int) Cells[aax + aay*w].Index + aspan.GetCon(3);
	                                nd = (short) Math.Min(dist[aai] + 3, 255);
	                                if (nd < dist[i])
	                                    dist[i] = nd;
	                            }
	                        }
	                        if (s.GetCon(3) != NotConnected)
	                        {
	                            int ax = x + Helper.GetDirOffsetX(3);
	                            int ay = y + Helper.GetDirOffsetY(3);
	                            int ai = (int) Cells[ax + ay*w].Index + s.GetCon(3);
	                            CompactSpan aspan = Spans[ai];
	                            nd = (short) Math.Min((int) dist[ai] + 2, 255);
	                            if (nd < dist[i])
	                                dist[i] = nd;

	                            if (aspan.GetCon(2) != NotConnected)
	                            {
	                                int aax = ax + Helper.GetDirOffsetX(2);
	                                int aay = ay + Helper.GetDirOffsetY(2);
	                                int aai = (int) Cells[aax + aay*w].Index + aspan.GetCon(2);
	                                nd = (short) Math.Min(dist[aai] + 3, 255);
	                                if (nd < dist[i])
	                                    dist[i] = nd;
	                            }
	                        }
	                }
	            }
	        }

	        for (int y = h-1; y >= 0; y--)
	        {
	            for (int x = w - 1; x >= 0; x--)
	            {
	                CompactCell c = Cells[x + y*w];
	                    for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
	                    {
	                        CompactSpan s = Spans[i];
	                        if (s.GetCon(2) != NotConnected)
	                        {
	                            int ax = x + Helper.GetDirOffsetX(2);
	                            int ay = y + Helper.GetDirOffsetY(2);
	                            int ai = (int) Cells[ax + ay*w].Index + s.GetCon(2);
	                            CompactSpan aspan = Spans[ai];
	                            nd = (short) Math.Min((int) dist[ai] + 2, 255);
	                            if (nd < dist[i])
	                                dist[i] = nd;

	                            if (aspan.GetCon(1) != NotConnected)
	                            {
	                                int aax = ax + Helper.GetDirOffsetX(1);
	                                int aay = ay + Helper.GetDirOffsetY(1);
	                                int aai = (int) Cells[aax + aay*w].Index + aspan.GetCon(1);
	                                nd = (short) Math.Min(dist[aai] + 3, 255);
	                                if (nd < dist[i])
	                                    dist[i] = nd;
	                            }
	                        }
	                        if (s.GetCon(1) != NotConnected)
	                        {
	                            int ax = x + Helper.GetDirOffsetX(1);
	                            int ay = y + Helper.GetDirOffsetY(1);
	                            int ai = (int) Cells[ax + ay*w].Index + s.GetCon(1);
	                            CompactSpan aspan = Spans[ai];
	                            nd = (short) Math.Min((int) dist[ai] + 2, 255);
	                            if (nd < dist[i])
	                                dist[i] = nd;

	                            if (aspan.GetCon(0) != NotConnected)
	                            {
	                                int aax = ax + Helper.GetDirOffsetX(0);
	                                int aay = ay + Helper.GetDirOffsetY(0);
	                                int aai = (int) Cells[aax + aay*w].Index + aspan.GetCon(0);
	                                nd = (short) Math.Min(dist[aai] + 3, 255);
	                                if (nd < dist[i])
	                                    dist[i] = nd;
	                            }
	                        }
	                    }
	            }
	        }

	        short thr = (short)(radius*2);
            for (int i = 0; i < SpanCount; i++)
            {
                if (dist[i] < thr)
                {
                    Areas[i] = HeightField.NullArea;
                }
            }

	        return true;
	    }

	    public bool BuildDistanceField()
	    {
            int[] src = new int[SpanCount];
            int[] dst = new int[SpanCount];

	        int maxDist = 0;

	        CalculateDistanceField(ref src, ref maxDist);
	        MaxDistance = maxDist;

	        Dist = BoxBlur(1, src, ref dst);

	        return true;
	    }

	    private int[] BoxBlur(int thr, int[] src, ref int[] dst)
	    {
	        int w = Width;
	        int h = Height;

	        thr *= 2;
	        for (int y = 0; y < h; y++)
	        {
	            for (int x = 0; x < w; x++)
	            {
	                CompactCell c = Cells[x + y*w];
	                    for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
	                    {
	                        CompactSpan s = Spans[i];
	                        int cd = src[i];
	                        if (cd <= thr)
	                        {
	                            dst[i] = cd;
	                            continue;
	                        }

	                        int d = cd;
	                        for (int dir = 0; dir < 4; dir++)
	                        {
	                            if (s.GetCon(dir) != NotConnected)
	                            {
	                                int ax = x + Helper.GetDirOffsetX(dir);
	                                int ay = y + Helper.GetDirOffsetY(dir);
	                                int ai = (int) Cells[ax + ay*w].Index + s.GetCon(dir);
	                                d += src[ai];

	                                CompactSpan aspan = Spans[ai];
	                                int dir2 = (dir + 1) & 0x3;
	                                if (aspan.GetCon(dir2) != NotConnected)
	                                {
	                                    int ax2 = ax + Helper.GetDirOffsetX(dir2);
	                                    int ay2 = ay + Helper.GetDirOffsetY(dir2);
	                                    int ai2 = (int) Cells[ax2 + ay2*w].Index + aspan.GetCon(dir2);
	                                    d += src[ai2];
	                                }
	                                else
	                                {
	                                    d += cd;
	                                }
	                            }
	                            else
	                            {
	                                d += cd*2;
	                            }
	                        }
	                        dst[i] = (d + 5)/9;
	                    }
	            }
	        }
	        return dst;
	    }

	    private void CalculateDistanceField(ref int[] src, ref int maxDist)
	    {
	        int w = Width;
	        int h = Height;
	        for (int i = 0; i < SpanCount; i++)
	            src[i] = 0xffff;

	        for (int y = 0; y < h; y++)
	        {
	            for (int x = 0; x < w; x++)
	            {
	                CompactCell c = Cells[x + y*w];
	                    for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
	                    {
	                        CompactSpan s = Spans[i];
	                        uint area = Areas[i];

	                        int nc = 0;
	                        for (int dir = 0; dir < 4; dir++)
	                        {
	                            if (s.GetCon(dir) != NotConnected)
	                            {
	                                int ax = x + Helper.GetDirOffsetX(dir);
	                                int ay = y + Helper.GetDirOffsetY(dir);
	                                int ai = (int) Cells[ax + ay*w].Index + s.GetCon(dir);
	                                if (area == Areas[ai])
	                                    nc++;
	                            }
	                        }
	                        if (nc != 4)
	                            src[i] = 0;
	                    }
	            }
	        }

	        for (int y = 0; y < h; y++)
	        {
	            for (int x = 0; x < w; x++)
	            {
	                CompactCell c = Cells[x + y*w];
	                    for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
	                    {
	                        CompactSpan s = Spans[i];
	                        if (s.GetCon(0) != NotConnected)
	                        {
	                            int ax = x + Helper.GetDirOffsetX(0);
	                            int ay = y + Helper.GetDirOffsetY(0);
	                            int ai = (int) Cells[ax + ay*w].Index + s.GetCon(0);
	                            CompactSpan aspan = Spans[ai];
	                            if (src[ai] + 2 < src[i])
	                                src[i] = src[ai] + 2;

	                            if (aspan.GetCon(3) != NotConnected)
	                            {
	                                int aax = ax + Helper.GetDirOffsetX(3);
	                                int aay = ay + Helper.GetDirOffsetY(3);
	                                int aai = (int) Cells[aax + aay*w].Index + aspan.GetCon(3);
	                                if (src[aai] + 3 < src[i])
	                                    src[i] = src[aai] + 3;
	                            }
	                        }
	                        if (s.GetCon(3) != NotConnected)
	                        {
	                            int ax = x + Helper.GetDirOffsetX(3);
	                            int ay = y + Helper.GetDirOffsetY(3);
	                            int ai = (int) Cells[ax + ay*w].Index + s.GetCon(3);
	                            CompactSpan aspan = Spans[ai];
	                            if (src[ai] + 2 < src[i])
	                                src[i] = src[ai] + 2;

	                            if (aspan.GetCon(2) != NotConnected)
	                            {
	                                int aax = ax + Helper.GetDirOffsetX(2);
	                                int aay = ay + Helper.GetDirOffsetY(2);
	                                int aai = (int) Cells[aax + aay*w].Index + aspan.GetCon(2);
	                                if (src[aai] + 3 < src[i])
	                                    src[i] = src[aai] + 3;
	                            }
	                        }
	                    }
	            }
	        }

            //pass 2
            for (int y = h-1; y >= 0; y--)
            {
                for (int x = w-1; x >= 0; x--)
                {
                    CompactCell c = Cells[x + y * w];
                        for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
                        {
                            CompactSpan s = Spans[i];
                            if (s.GetCon(2) != NotConnected)
                            {
                                int ax = x + Helper.GetDirOffsetX(2);
                                int ay = y + Helper.GetDirOffsetY(2);
                                int ai = (int) Cells[ax + ay*w].Index + s.GetCon(2);
                                CompactSpan aspan = Spans[ai];
                                if (src[ai] + 2 < src[i])
                                    src[i] = src[ai] + 2;

                                if (aspan.GetCon(1) != NotConnected)
                                {
                                    int aax = ax + Helper.GetDirOffsetX(1);
                                    int aay = ay + Helper.GetDirOffsetY(1);
                                    int aai = (int) Cells[aax + aay*w].Index + aspan.GetCon(1);
                                    if (src[aai] + 3 < src[i])
                                        src[i] = src[aai] + 3;
                                }
                            }
                            if (s.GetCon(1) != NotConnected)
                            {
                                int ax = x + Helper.GetDirOffsetX(1);
                                int ay = y + Helper.GetDirOffsetY(1);
                                int ai = (int) Cells[ax + ay*w].Index + s.GetCon(1);
                                CompactSpan aspan = Spans[ai];
                                if (src[ai] + 2 < src[i])
                                    src[i] = src[ai] + 2;

                                if (aspan.GetCon(0) != NotConnected)
                                {
                                    int aax = ax + Helper.GetDirOffsetX(0);
                                    int aay = ay + Helper.GetDirOffsetY(0);
                                    int aai = (int) Cells[aax + aay*w].Index + aspan.GetCon(0);
                                    if (src[aai] + 3 < src[i])
                                        src[i] = src[aai] + 3;
                                }
                            }
                        }
                }
            }

	        maxDist = 0;
            for (int i = 0; i < SpanCount; i++)
            {
                maxDist = Math.Max(src[i], maxDist);
            }
	    }

	    public bool BuildRegions(int borderSize, int minRegionArea, int mergeRegionArea)
	    {
	        int w = Width;
	        int h = Height;

            //int[] buf = new int[SpanCount*4];

            IntArray stack = new IntArray(1024);
            IntArray visited = new IntArray(1024);

	        int[] srcReg = new int[SpanCount];
	        int[] srcDist = new int[SpanCount];
	        int[] dstReg = new int[SpanCount];
	        int[] dstDist = new int[SpanCount];

	        int regionId = 1;
	        int level = (MaxDistance + 1) & ~1;

	        int expandIters = 8;

            if (borderSize > 0)
            {
                int bw = Math.Min(w, borderSize);
                int bh = Math.Min(h, borderSize);

                PaintRectRegion(0, bw, 0, h, regionId | BorderReg, ref srcReg); regionId++;
                PaintRectRegion(w-bw, w, 0, h, regionId | BorderReg, ref srcReg); regionId++;
                PaintRectRegion(0, w, 0, bh, regionId | BorderReg, ref srcReg); regionId++;
                PaintRectRegion(0, w, h-bh, h, regionId | BorderReg, ref srcReg); regionId++;

                BorderSize = borderSize;
            }

            while (level > 0)
            {
                level = level >= 2 ? level - 2 : 0;

                if (ExpandRegions(expandIters, level, ref srcReg, ref srcDist, ref dstReg, ref dstDist, ref stack) != srcReg)
                {
                    int[] @t = srcReg;
                    srcReg = dstReg;
                    dstReg = @t;
                    @t = srcDist;
                    srcDist = dstDist;
                    dstDist = @t;
                }

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        CompactCell c = Cells[x + y*w];
                            for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
                            {
                                if (Dist[i] < level || srcReg[i] != 0 || Areas[i] == HeightField.NullArea) continue;
                                if (FloodRegion(x, y, i, level, regionId, ref srcReg, ref srcDist, ref stack))
                                    regionId++;
                            }
                    }
                }
            }

	        if (ExpandRegions(expandIters*8, 0, ref srcReg, ref srcDist, ref dstReg, ref dstDist, ref stack) != srcReg)
	        {
	            int[] t = srcReg;
	            srcReg = dstReg;
	            dstReg = t;
	            t = srcDist;
	            srcDist = dstDist;
	            dstDist = t;
	        }

	        MaxRegions = regionId;
	        if (!FilterSmallRegions(minRegionArea, mergeRegionArea, ref srcReg))
	            return false;

	        for (int i = 0; i < SpanCount; i++)
	        {
	            Spans[i].Reg = srcReg[i];
	        }
	        return true;
	    }

	    private bool FilterSmallRegions(int minRegionArea, int mergeRegionArea, ref int[] srcReg)
	    {
	        int w = Width;
	        int h = Height;

	        int nreg = MaxRegions + 1;
            Region[] regions = new Region[nreg];

	        for (int i = 0; i < nreg; i++)
	        {
	            regions[i] = new Region(i);
	        }

	        for (int y = 0; y < h; y++)
	        {
	            for (int x = 0; x < w; x++)
	            {
	                CompactCell c = Cells[x + y*w];
	                    for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
	                    {
	                        int r = srcReg[i];
	                        if (r <= 0 || r >= nreg)
	                            continue;
	                        Region reg = regions[r];
	                        reg.SpanCount++;

	                        for (int j = (int) c.Index; j < ni; j++)
	                        {
	                            if (i == j) continue;
	                            int floorId = srcReg[j];
	                            if (floorId <= 0 || floorId >= nreg) continue;
	                            AddUniqueFloorRegion(ref reg, floorId);
	                        }

	                        if (reg.Connections.Size > 0)
	                            continue;

	                        reg.AreaType = (short) Areas[i];

	                        int ndir = -1;
	                        for (int dir = 0; dir < 4; dir++)
	                        {
	                            if (IsSolidEdge(srcReg, x, y, i, dir))
	                            {
	                                ndir = dir;
	                                break;
	                            }
	                        }

	                        if (ndir != -1)
	                        {
	                            WalkContour(x, y, i, ndir, ref srcReg, ref reg);
	                        }
	                    }
	            }
	        }

            // remove too small regions.
            IntArray stack = new IntArray(32);
            IntArray trace = new IntArray(32);
	        for (int i = 0; i < nreg; i++)
	        {
	            Region reg = regions[i];
	            if (reg.Id <= 0 || (reg.Id & BorderReg) != 0) continue;
                if (reg.SpanCount == 0) continue;
	            if (reg.Visited) continue;

	            bool connectsToBorder = false;
	            int spanCount = 0;
	            stack.Resize(0);
                trace.Resize(0);

	            reg.Visited = true;
                stack.Push(i);

                while (stack.Size > 0)
                {
                    int ri = stack.Pop();

                    Region creg = regions[ri];
                    spanCount += creg.SpanCount;
                    trace.Push(ri);

                    for (int j = 0; j < creg.Connections.Size; j++)
                    {
                        if ((creg.Connections[j] & BorderReg) != 0)
                        {
                            connectsToBorder = true;
                            continue;
                        }
                        Region neireg = regions[creg.Connections[j]];
                        if (neireg.Visited) continue;
                        if (neireg.Id <= 0 || (neireg.Id & BorderReg) != 0) continue;
                        stack.Push(neireg.Id);
                        neireg.Visited = true;
                    }
                }

                // if the region's size is too small, remove it.
                if (spanCount < minRegionArea && !connectsToBorder)
                {
                    for (int j = 0; j < trace.Size; j++)
                    {
                        regions[trace[j]].SpanCount = 0;
                        regions[trace[j]].Id = 0;
                    }
                }
	        }

	        int mergeCount = 0;
	        do
	        {
	            mergeCount = 0;
	            for (int i = 0; i < nreg; i++)
	            {
	                Region reg = regions[i];
	                if (reg.Id <= 0 || (reg.Id & BorderReg) != 0) continue;
                    if(reg.SpanCount == 0) continue;

	                if (reg.SpanCount > mergeRegionArea && IsRegionConnectedToBorder(reg)) continue;

                    // small region with more than 1 connection
                    // or region which is not connected to a border at all.
                    // find smallest neighbor
	                int smallest = int.MaxValue;
	                int mergeId = reg.Id;
	                for (int j = 0; j < reg.Connections.Size; j++)
	                {
	                    if ((reg.Connections[j] & BorderReg) != 0) continue;
	                    Region mreg = regions[reg.Connections[j]];
                        if(mreg.Id <= 0 || (mreg.Id & BorderReg) != 0) continue;
                        if (mreg.SpanCount < smallest && CanMergeWithRegion(reg, mreg) && CanMergeWithRegion(mreg, reg))
                        {
                            smallest = mreg.SpanCount;
                            mergeId = mreg.Id;
                        }
	                }
                    if (mergeId != reg.Id)
                    {
                        int oldId = reg.Id;
                        Region target = regions[mergeId];

                        if (MergeRegions(ref target, ref reg))
                        {
                            for (int j = 0; j < nreg; j++)
                            {
                                if (regions[j].Id == 0 || (regions[j].Id & BorderReg) != 0) continue;
                                if (regions[j].Id == oldId)
                                {
                                    regions[j].Id = mergeId;
                                }
                                Region reg2 = regions[j];
                                ReplaceNeighbor(ref reg2, oldId, mergeId);
                            }
                            mergeCount++;
                        }
                    }
	            }
	        } while (mergeCount > 0);

            // compress regions Ids
	        for (int i = 0; i < nreg; i++)
	        {
	            regions[i].Remap = false;
	            if (regions[i].Id == 0 || (regions[i].Id & BorderReg) != 0) continue;
	            regions[i].Remap = true;
	        }

	        int regIdGen = 0;
	        for (int i = 0; i < nreg; i++)
	        {
	            if (!regions[i].Remap) continue;

	            int oldId = regions[i].Id;
	            int newId = ++regIdGen;
	            for (int j = i; j < nreg; j++)
	            {
	                if (regions[j].Id == oldId)
	                {
	                    regions[j].Id = newId;
	                    regions[j].Remap = false;
	                }
	            }
	        }

	        MaxRegions = regIdGen;

            // remap regions
	        for (int i = 0; i < SpanCount; i++)
	        {
	            if ((srcReg[i] & BorderReg) == 0)
	                srcReg[i] = regions[srcReg[i]].Id;
	        }

	        return true;
	    }

	    private bool MergeRegions(ref Region rega, ref Region regb)
	    {
	        int aid = rega.Id;
	        int bid = regb.Id;

	        IntArray acon = new IntArray(rega.Connections.Size);
	        for (int i = 0; i < rega.Connections.Size; i++)
	        {
	            acon[i] = rega.Connections[i];
	        }
	        IntArray bcon = regb.Connections;

	        int insa = -1;
	        for (int i = 0; i < acon.Size; i++)
	        {
	            if (acon[i] == bid)
	            {
	                insa = i;
	                break;
	            }
	        }
	        if (insa == -1)
	            return false;

	        int insb = -1;
	        for (int i = 0; i < bcon.Size; i++)
	        {
	            if (bcon[i] == aid)
	            {
	                insb = i;
	                break;
	            }
	        }
	        if (insb == -1)
	            return false;

            rega.Connections.Resize(0);
	        for (int i = 0, ni = acon.Size; i < ni - 1; i++)
	        {
	            rega.Connections.Push(acon[(insa+1+i)%ni]);
	        }
            for (int i = 0, ni = bcon.Size; i < ni - 1; i++)
            {
                rega.Connections.Push(bcon[(insb+1+i)%ni]);
            }
            RemoveAdjacentNeighbors(ref rega);

            for (int j = 0; j < regb.Floors.Size; j++)
            {
                AddUniqueFloorRegion(ref rega, regb.Floors[j]);
            }
	        rega.SpanCount += regb.SpanCount;
	        regb.SpanCount = 0;
            regb.Connections.Resize(0);

	        return true;
	    }

	    private void ReplaceNeighbor(ref Region reg, int oldId, int newId)
	    {
	        bool neiChanged = false;
	        for (int i = 0; i < reg.Connections.Size; i++)
	        {
	            if (reg.Connections[i] == oldId)
	            {
	                reg.Connections[i] = newId;
	                neiChanged = true;
	            }
	        }
	        for (int i = 0; i < reg.Floors.Size; i++)
	        {
	            if (reg.Floors[i] == oldId)
	                reg.Floors[i] = newId;
	        }
	        if (neiChanged)
	            RemoveAdjacentNeighbors(ref reg);
	    }

	    private void RemoveAdjacentNeighbors(ref Region reg)
	    {
	        for (int i = 0; i < reg.Connections.Size && reg.Connections.Size > 1;)
	        {
	            int ni = (i + 1)%reg.Connections.Size;
                if (reg.Connections[i] == reg.Connections[ni])
                {
                    for (int j = 0; j < reg.Connections.Size-1; j++)
                    {
                        reg.Connections[j] = reg.Connections[j + 1];
                    }
                    reg.Connections.Pop();
                }
                else
                {
                    i++;
                }
	        }
	    }

	    private bool CanMergeWithRegion(Region rega, Region regb)
	    {
	        if (rega.AreaType != regb.AreaType)
	            return false;
	        int n = 0;
            for (int i = 0; i < rega.Connections.Size; i++)
            {
                if (rega.Connections[i] == regb.Id)
                    n++;
            }
	        if (n > 1)
	            return false;
	        for (int i = 0; i < rega.Floors.Size; i++)
	        {
	            if (rega.Floors[i] == regb.Id)
	                return false;
	        }
	        return true;
	    }

	    private bool IsRegionConnectedToBorder(Region reg)
	    {
	        for (int i = 0; i < reg.Connections.Size; i++)
	        {
	            if (reg.Connections[i] == 0)
	                return true;
	        }
	        return false;
	    }

	    private void WalkContour(int x, int y, int i, int dir, ref int[] srcReg, ref Region cont)
	    {
	        int startDir = dir;
	        int starti = i;

	        CompactSpan ss = Spans[i];
	        int curReg = 0;
            if (ss.GetCon(dir) != NotConnected)
            {
                int ax = x + Helper.GetDirOffsetX(dir);
                int ay = y + Helper.GetDirOffsetY(dir);
                int ai = (int) Cells[ax + ay*Width].Index + ss.GetCon(dir);
                curReg = srcReg[ai];
            }
            cont.Connections.Push(curReg);

	        int iter = 0;
            while (++iter < 40000)
            {
                CompactSpan s = Spans[i];
                if (IsSolidEdge(srcReg, x, y, i, dir))
                {
                    int r = 0;
                    if (s.GetCon(dir) != NotConnected)
                    {
                        int ax = x + Helper.GetDirOffsetX(dir);
                        int ay = y + Helper.GetDirOffsetY(dir);
                        int ai = (int) Cells[ax + ay*Width].Index + s.GetCon(dir);
                        r = srcReg[ai];
                    }
                    if (r != curReg)
                    {
                        curReg = r;
                        cont.Connections.Push(curReg);
                    }

                    dir = (dir + 1) & 0x3; // rotate CW
                }
                else
                {
                    int ni = -1;
                    int nx = x + Helper.GetDirOffsetX(dir);
                    int ny = y + Helper.GetDirOffsetY(dir);
                    if (s.GetCon(dir) != NotConnected)
                    {
                        CompactCell nc = Cells[nx + ny*Width];
                        ni = (int) nc.Index + s.GetCon(dir);
                    }
                    if (ni == -1)
                    {
                        // Should no happen
                        return;
                    }
                    x = nx;
                    y = ny;
                    i = ni;
                    dir = (dir + 3) & 0x3; // rotate CCW
                }

                if (starti == i && startDir == dir)
                {
                    break;
                }
            }

            // Remove adjecent duplicates
            if (cont.Connections.Size > 1)
            {
                for (int j = 0; j < cont.Connections.Size; )
                {
                    int nj = (j + 1) % cont.Connections.Size;
                    if (cont.Connections[j] == cont.Connections[nj])
                    {
                        for (int k = j; k < cont.Connections.Size - 1; k++)
                        {
                            cont.Connections[k] = cont.Connections[k + 1];
                        }
                        cont.Connections.Pop();
                    }
                    else
                    {
                        j++;
                    }
                }
            }
	    }

	    private bool IsSolidEdge(int[] srcReg, int x, int y, int i, int dir)
	    {
	        CompactSpan s = Spans[i];
	        int r = 0;
            if (s.GetCon(dir) != NotConnected)
            {
                int ax = x + Helper.GetDirOffsetX(dir);
                int ay = y + Helper.GetDirOffsetY(dir);
                int ai = (int) Cells[ax + ay*Width].Index + s.GetCon(dir);
                r = srcReg[ai];
            }
	        if (r == srcReg[i])
	            return false;
	        return true;
	    }

	    private void AddUniqueFloorRegion(ref Region reg, int n)
	    {
            // Only add n if it isn't in the array already
	        for (int i = 0; i < reg.Floors.Size; i++)
	        {
	            if (reg.Floors[i] == n)
	                return;
	        }
            reg.Floors.Push(n);
	    }

	    private bool FloodRegion(int x, int y, int i, int level, int r, ref int[] srcReg, ref int[] srcDist, ref IntArray stack)
	    {
	        int w = Width;
	        uint area = Areas[i];

	        stack.Resize(0);
            stack.Push(x);
            stack.Push(y);
            stack.Push(i);
	        srcReg[i] = r;
	        srcDist[i] = 0;

	        int lev = level >= 2 ? level - 2 : 0;
	        int count = 0;

            while (stack.Size > 0)
            {
                int ci = stack.Pop();
                int cy = stack.Pop();
                int cx = stack.Pop();

                CompactSpan cs = Spans[ci];

                int ar = 0;
                for (int dir = 0; dir < 4; dir++)
                {
                    if (cs.GetCon(dir) != NotConnected)
                    {
                        int ax = cx + Helper.GetDirOffsetX(dir);
                        int ay = cy + Helper.GetDirOffsetY(dir);
                        int ai = (int) Cells[ax + ay*w].Index + cs.GetCon(dir);
                        if (Areas[ai] != area) continue;
                        int nr = srcReg[ai];
                        if ((nr & BorderReg) != 0) continue;
                        if (nr != 0 && nr != r)
                            ar = nr;
                        CompactSpan aspan = Spans[ai];

                        int dir2 = (dir + 1) & 0x3;
                        if (aspan.GetCon(dir2) != NotConnected)
                        {
                            int ax2 = ax + Helper.GetDirOffsetX(dir2);
                            int ay2 = ay + Helper.GetDirOffsetY(dir2);
                            int ai2 = (int) Cells[ax2 + ay2*w].Index + aspan.GetCon(dir2);
                            if (Areas[ai2] != area) continue;
                            int nr2 = srcReg[ai2];
                            if (nr2 != 0 && nr2 != r)
                                ar = nr2;
                        }
                    }
                }
                if (ar != 0)
                {
                    srcReg[ci] = 0;
                    continue;
                }
                count++;

                for (int dir = 0; dir < 4; dir++)
                {
                    if (cs.GetCon(dir) != NotConnected)
                    {
                        int ax = cx + Helper.GetDirOffsetX(dir);
                        int ay = cy + Helper.GetDirOffsetY(dir);
                        int ai = (int) Cells[ax + ay*w].Index + cs.GetCon(dir);
                        if(Areas[ai] != area) continue;
                        if (Dist[ai] >= lev && srcReg[ai] == 0)
                        {
                            srcReg[ai] = r;
                            srcDist[ai] = 0;
                            stack.Push(ax);
                            stack.Push(ay);
                            stack.Push(ai);
                            
                        }
                    }
                }
            }
	        return count > 0;
	    }

	    private int[] ExpandRegions(int maxIter, int level, ref int[] srcReg, ref int[] srcDist, ref int[] dstReg, ref int[] dstDist, ref IntArray stack)
	    {
	        int w = Width;
	        int h = Height;

            stack.Resize(0);
	        for (int y = 0; y < h; y++)
	        {
	            for (int x = 0; x < w; x++)
	            {
	                CompactCell c = Cells[x + y*w];
	                    for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
	                    {
	                        if (Dist[i] >= level && srcReg[i] == 0 && Areas[i] != HeightField.NullArea)
	                        {
	                            stack.Push(x);
	                            stack.Push(y);
	                            stack.Push(i);
	                        }
	                    }
	            }
	        }

	        int iter = 0;
	        while (stack.Size > 0)
	        {
	            int failed = 0;

                Buffer.BlockCopy(srcReg, 0, dstReg, 0, sizeof(int)*SpanCount);
                Buffer.BlockCopy(srcDist, 0, dstDist, 0, sizeof(int) * SpanCount);
                //Array.Copy(srcReg, dstReg, SpanCount);
                //Array.Copy(srcDist, dstDist, SpanCount);

                for (int j = 0; j < stack.Size; j += 3)
                {
                    int x = stack[j + 0];
                    int y = stack[j + 1];
                    int i = stack[j + 2];
                    if (i < 0)
                    {
                        failed++;
                        continue;
                    }

                    int r = srcReg[i];
                    int d2 = short.MaxValue;
                    uint area = Areas[i];
                    CompactSpan s = Spans[i];
                    for (int dir = 0; dir < 4; dir++)
                    {
                        if (s.GetCon(dir) == NotConnected) continue;
                        int ax = x + Helper.GetDirOffsetX(dir);
                        int ay = y + Helper.GetDirOffsetY(dir);
                        int ai = (int) Cells[ax + ay*w].Index + s.GetCon(dir);
                        if (Areas[ai] != area) continue;
                        if (srcReg[ai] > 0 && (srcReg[ai] & BorderReg) == 0)
                        {
                            if (srcDist[ai] + 2 < d2)
                            {
                                r = srcReg[ai];
                                d2 = srcDist[ai] + 2;
                            }
                        }
                    }
                    if (r != 0)
                    {
                        stack[j + 2] = -1;
                        dstReg[i] = r;
                        dstDist[i] = d2;
                    }
                    else
                    {
                        failed++;
                    }
                }
                int[] temp = srcReg;
                srcReg = dstReg;
	            dstReg = temp;
	            temp = srcDist;
	            srcDist = dstDist;
	            dstDist = temp;

	            if (failed*3 == stack.Size)
	                break;

                if (level > 0)
                {
                    ++iter;
                    if (iter >= maxIter)
                        break;
                }
	        }
	        return srcReg;
	    }

	    private void PaintRectRegion(int minx, int maxx, int miny, int maxy, int regId, ref int[] srcReg)
	    {
	        int w = Width;
	        for (int y = miny; y < maxy; y++)
	        {
	            for (int x = minx; x < maxx; x++)
	            {
	                CompactCell c = Cells[x + y*w];
	                    for (int i = (int) c.Index, ni = (int) (c.Index + c.Count); i < ni; i++)
	                    {
	                        if (Areas[i] != HeightField.NullArea)
	                            srcReg[i] = regId;
	                    }
	            }
	        }
	    }
	}
}
