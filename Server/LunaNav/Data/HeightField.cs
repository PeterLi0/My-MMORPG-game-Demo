using System;
using System.Collections.Generic;

namespace LunaNav
{
    [Serializable]
    public class HeightField
    {
        //< The width of the heightfield. (Along the x-axis in cell units.)
        public int Width { get; set; }
        //< The height of the heightfield. (Along the z-axis in cell units.)
        public int Height { get; set; }
        //< The minimum bounds in world space. [(x, y, z)]
	    public float[] Bmin = new float[3];
        //< The maximum bounds in world space. [(x, y, z)]
	    public float[] Bmax = new float[3];		
        //< The size of each cell. (On the xz-plane.)
        public float Cs { get; set; }
        //< The height of each cell. (The minimum increment along the y-axis.)
        public float Ch { get; set; }
        //< Heightfield of spans (width*height).
	    public Span[] Spans;
        //< Linked list of span pools.
        public SpanPool Pools;
        //< The next free span.
        public Span Freelist;
        protected float[] d = new float[12];

        public static uint NullArea = 0;

        public HeightField(int width, int height, float[] bmin, float[] bmax, float cs, float ch)
        {
            Width = width;
            Height = height;
            Array.Copy(bmin, 0, Bmin, 0, 3);
            Array.Copy(bmax, 0, Bmax, 0, 3);
            Cs = cs;
            Ch = ch;
            Spans = new Span[width*height];
        }

        public void RasterizeTriangles(Geometry geom, short[] areas, int flagMergeThr)
        {
            RasterizeTriangles(geom, geom.Triangles, geom.NumTriangles, areas, flagMergeThr);
        }

        public void RasterizeTriangles(Geometry geom, List<int> triangles, int numTriangles, short[] areas,
                                       int flagMergeThr)
        {
            float ics = 1.0f / Cs;
            float ich = 1.0f / Ch;

            for (int i = 0; i < numTriangles; i++)
            {
                int v0 = triangles[i * 3 + 0];
                int v1 = triangles[i * 3 + 1];
                int v2 = triangles[i * 3 + 2];

                RasterizeTriangle(geom.Vertexes, v0, v1, v2, areas[i], ics, ich, flagMergeThr);
            }            
        }

        private void RasterizeTriangle(List<RecastVertex> vertexes, int v0, int v1, int v2, short area, float ics, float ich, int flagMergeThr)
        {
            int w = Width;
            int h = Height;
            RecastVertex tempMin, tempMax;
            
            float by = Bmax[1] - Bmin[1];
            tempMin = new RecastVertex(vertexes[v0]);
            tempMax = new RecastVertex(vertexes[v0]);
            tempMin = RecastVertex.Min(tempMin, vertexes[v1]);
            tempMin = RecastVertex.Min(tempMin, vertexes[v2]);
            tempMax = RecastVertex.Max(tempMax, vertexes[v1]);
            tempMax = RecastVertex.Max(tempMax, vertexes[v2]);

            if (!OverlapBounds(Bmin, Bmax, tempMin.ToArray(), tempMax.ToArray()))
                return;

            int x0 = (int)((tempMin[0] - Bmin[0]) * ics);
            int y0 = (int)((tempMin[2] - Bmin[2]) * ics);
            int x1 = (int)((tempMax[0] - Bmin[0]) * ics);
            int y1 = (int)((tempMax[2] - Bmin[2]) * ics);

            x0 = Math.Max(0, Math.Min(x0, w - 1));
            y0 = Math.Max(0, Math.Min(y0, h - 1));
            x1 = Math.Max(0, Math.Min(x1, w - 1));
            y1 = Math.Max(0, Math.Min(y1, h - 1));

            float[] inArray = new float[7*3], outArray = new float[7*3], inrowArray = new float[7*3];

            for (int y = y0; y <= y1; y++)
            {
                Array.Copy(vertexes[v0].ToArray(), 0, inArray, 0, 3);
                Array.Copy(vertexes[v1].ToArray(), 0, inArray, 1*3, 3);
                Array.Copy(vertexes[v2].ToArray(), 0, inArray, 2*3, 3);
                int nvrow = 3;
                float cz = Bmin[2] + y*Cs;
                nvrow = ClipPoly(inArray, nvrow, ref outArray, 0, 1, -cz);
                if (nvrow < 3) continue;
                nvrow = ClipPoly(outArray, nvrow, ref inrowArray, 0, -1, cz + Cs);
                if (nvrow < 3) continue;

                for (int x = x0; x <= x1; x++)
                {
                    int nv = nvrow;
                    float cx = Bmin[0] + x*Cs;
                    nv = ClipPoly(inrowArray, nv, ref outArray, 1, 0, -cx);
                    if (nv < 3) continue;
                    nv = ClipPoly(outArray, nv, ref inArray, -1, 0, cx + Cs);
                    if (nv < 3) continue;

                    float smin = inArray[1], smax = inArray[1];
                    for(int i = 1; i < nv; i++)
                    {
                        smin = Math.Min(smin, inArray[i*3 + 1]);
                        smax = Math.Max(smax, inArray[i*3 + 1]);
                    }
                    smin -= Bmin[1];
                    smax -= Bmin[1];

                    if(smax.CompareTo(0.0f) < 0 || smin.CompareTo(by) > 0) continue;
                    if (smin.CompareTo(0.0f) < 0) smin = 0;
                    if (smax.CompareTo(by) > 0) smax = by;

                    int ismin = Math.Max(0, Math.Min((int)Math.Floor(smin * ich), short.MaxValue));
                    int ismax = Math.Max(ismin+1, Math.Min((int)Math.Floor(smax * ich), short.MaxValue));

                    AddSpan(x, y, ismin, ismax, area, flagMergeThr);
                }
            }
        }

        private void AddSpan(int x, int y, int smin, int smax, short area, int flagMergeThr)
        {
            int idx = x + y*Width;
            Span s = AllocSpan();
            s.SMin = (uint) smin;
            s.SMax = (uint) smax;
            s.Area = (uint) area;
            s.Next = null;

            if(Spans[idx] == null)
            {
                Spans[idx] = s;
                return;
            }

            Span prev = null;
            Span cur = Spans[idx];

            while(cur != null)
            {
                if(cur.SMin > s.SMax)
                {
                    break;
                }
                else if(cur.SMax < s.SMin)
                {
                    prev = cur;
                    cur = cur.Next;
                }
                else
                {
                    if(cur.SMin < s.SMin)
                    {
                        s.SMin = cur.SMin;
                    }
                    if(cur.SMax > s.SMax)
                    {
                        s.SMax = cur.SMax;
                    }

                    if(Math.Abs((int)s.SMax-(int)cur.SMax) <= flagMergeThr)
                    {
                        s.Area = Math.Max(s.Area, cur.Area);
                    }

                    Span next = cur.Next;
                    FreeSpan(cur);
                    if(prev != null)
                    {
                        prev.Next = next;
                    }
                    else
                    {
                        Spans[idx] = next;
                    }
                    cur = next;
                }
            }

            if(prev != null)
            {
                s.Next = prev.Next;
                prev.Next = s;
            }
            else
            {
                s.Next = Spans[idx];
                Spans[idx] = s;
            }
        }

        private void FreeSpan(Span ptr)
        {
            if (ptr == null) return;
            ptr.Next = Freelist;
            Freelist = ptr;
        }

        private Span AllocSpan()
        {
            if (Freelist == null)
            {
                Freelist = new Span();                    
            }

            Span it = Freelist;
            Freelist = Freelist.Next;
            return it;
        }

        private int ClipPoly(float[] inArray, int n, ref float[] outArray, int pnx, int pnz, float pd)
        {
            for (int i = 0; i < n; i++)
            {
                d[i] = pnx*inArray[i*3 + 0] + pnz*inArray[i*3 + 2] + pd;
            }

            int m = 0;
            for (int i = 0, j = n-1; i < n; j=i, i++)
            {
                bool ina = d[j] >= 0;
                bool inb = d[i] >= 0;
                if(ina != inb)
                {
                    float s = d[j]/(d[j] - d[i]);
                    outArray[m * 3 + 0] = inArray[j * 3 + 0] + (inArray[i * 3 + 0] - inArray[j * 3 + 0]) * s;
                    outArray[m * 3 + 1] = inArray[j * 3 + 1] + (inArray[i * 3 + 1] - inArray[j * 3 + 1]) * s;
                    outArray[m * 3 + 2] = inArray[j * 3 + 2] + (inArray[i * 3 + 2] - inArray[j * 3 + 2]) * s;
                    m++;
                }
                if(inb)
                {
                    outArray[m * 3 + 0] = inArray[i * 3 + 0];
                    outArray[m * 3 + 1] = inArray[i * 3 + 1];
                    outArray[m * 3 + 2] = inArray[i * 3 + 2];
                    m++;
                }
            }
            return m;
        }

        private bool OverlapBounds(float[] amin, float[] amax, float[] bmin, float[] bmax)
        {
            bool overlap = true;
            overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
            overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
            overlap = (amin[2] > bmax[2] || amax[2] < bmin[2]) ? false : overlap;
            return overlap;
        }

        public void FilterLowHangingWalkableObstacles(int walkableClimb)
        {
            int w = Width;
            int h = Height;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Span ps = null;
                    bool previousWalkable = false;
                    uint previousArea = NullArea;

                    for (Span s = Spans[x+y*w]; s!=null; ps = s, s = s.Next)
                    {
                        bool walkable = s.Area != NullArea;

                        if (!walkable && previousWalkable)
                        {
                            if (Math.Abs((int) s.SMax - (int) ps.SMax) <= walkableClimb)
                                s.Area = previousArea;
                        }
                        previousWalkable = walkable;
                        previousArea = s.Area;
                    }
                }
            }
        }

        public void FilterLedgeSpans(int walkableHeight, int walkableClimb)
        {
            int w = Width;
            int h = Height;
            int MaxHeight = 0xffff;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    for (Span s = Spans[x + y*w]; s != null; s = s.Next)
                    {
                        if (s.Area == NullArea)
                            continue;

                        int bot = (int) s.SMax;
                        int top = s.Next != null ? (int) s.Next.SMin : MaxHeight;

                        int minh = MaxHeight;

                        int asmin = (int)s.SMax;
                        int asmax = (int)s.SMax;

                        for (int dir = 0; dir < 4; dir++)
                        {
                            int dx = x + Helper.GetDirOffsetX(dir);
                            int dy = y + Helper.GetDirOffsetY(dir);
                            if (dx < 0 || dy < 0 || dx >= w || dy >= h)
                            {
                                minh = Math.Min(minh, -walkableClimb - bot);
                                continue;
                            }
                            Span ns = Spans[dx + dy*w];
                            int nbot = -walkableClimb;
                            int ntop = ns != null ? (int)ns.SMin : MaxHeight;

                            if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
                                minh = Math.Min(minh, nbot - bot);

                            for (ns = Spans[dx + dy*w]; ns != null; ns = ns.Next)
                            {
                                nbot = (int) ns.SMax;
                                ntop = ns.Next != null ? (int)ns.Next.SMin : MaxHeight;
                                if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
                                {
                                    minh = Math.Min(minh, nbot - bot);
                                    if (Math.Abs(nbot - bot) <= walkableClimb)
                                    {
                                        if (nbot < asmin) asmin = nbot;
                                        if (nbot > asmax) asmax = nbot;
                                    }
                                }
                            }
                        }

                        if (minh < -walkableClimb)
                            s.Area = NullArea;

                        if ((asmax - asmin) > walkableClimb)
                        {
                            s.Area = NullArea;
                        }
                    }
                }
            }
        }

        public void FilterWalkableLowHeightSpans(int walkableHeight)
        {
            int w = Width;
            int h = Height;
            int MaxHeight = 0xffff;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    for (Span s = Spans[x + y*w]; s != null; s = s.Next)
                    {
                        int bot = (int) s.SMax;
                        int top = s.Next != null ? (int) s.Next.SMin : MaxHeight;
                        if ((top - bot) <= walkableHeight)
                            s.Area = NullArea;
                    }
                }
            }
        }

        public int GetHeightFieldSpanCount()
        {
            int w = Width;
            int h = Height;
            int spanCount = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    for (Span s = Spans[x + y*w]; s != null; s = s.Next)
                    {
                        if (s.Area != NullArea)
                            spanCount++;
                    }
                }
            }
            return spanCount;
        }
    }
}