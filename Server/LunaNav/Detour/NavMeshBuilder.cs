using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaNav;

namespace LunaNav
{
    [Serializable]
	public class NavMeshBuilder
    {
        public MeshHeader Header { get; set; }
        public float[] NavVerts { get; set; }
        public Poly[] NavPolys { get; set; }
        public Link[] NavLinks { get; set; }
        public PolyDetail[] NavDMeshes { get; set; }
        public float[] NavDVerts { get; set; }
        public short[] NavDTris { get; set; }
        public BVNode[] NavBvTree { get; set; }
        public OffMeshConnection[] OffMeshCons { get; set; }

        public static int MaxAreas = 64;


        public static int VertsPerPoly = 6;
        public static short PolyTypeGround = 0;
        public static short PolyTypeOffMeshConnection = 1;

        public static int ExtLink = 0x8000;
        public static short OffMeshConBiDir = 1;
        public NavMeshBuilder()
        {
            
        }

        public NavMeshBuilder(NavMeshCreateParams param)
        {
            if(param.Nvp > VertsPerPoly)
                throw new ArgumentException("Too many Verts per Poly for NavMeshBuilder");
            if(param.VertCount >= 0xffff)
                throw new ArgumentException("Too many total verticies for NavMeshBuilder");
            if(param.VertCount == 0 || param.Verts == null)
                throw new ArgumentException("No vertices, cannot generate nav mesh");
            if(param.PolyCount == 0 || param.Polys == null)
                throw new ArgumentException("No Polygons, cannot generate nav mesh");

            int nvp = param.Nvp;

            short[] offMeshConClass = new short[0];
            int storedOffMeshConCount = 0;
            int offMeshConLinkCount = 0;

            if (param.OffMeshConCount > 0)
            {
                offMeshConClass = new short[param.OffMeshConCount*2];

                float hmin = float.MaxValue;
                float hmax = float.MinValue;

                if (param.DetailVerts != null && param.DetailVertsCount > 0)
                {
                    for (int i = 0; i < param.DetailVertsCount; i++)
                    {
                        int h = i*3 + 1;
                        hmin = Math.Min(hmin, param.DetailVerts[h]);
                        hmax = Math.Max(hmax, param.DetailVerts[h]);
                    }
                }
                else
                {
                    for (int i = 0; i < param.VertCount; i++)
                    {
                        int iv = i*3;
                        float h = param.BMin[1] + param.Verts[iv + 1]*param.Ch;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }
                }
                hmin -= param.WalkableClimb;
                hmax += param.WalkableClimb;
                float[] bmin = new float[3], bmax = new float[3];
                Array.Copy(param.BMin, bmin, 3);
                Array.Copy(param.BMax, bmax, 3);

                bmin[1] = hmin;
                bmax[1] = hmax;

                for (int i = 0; i < param.OffMeshConCount; i++)
                {
                    int p0 = (i*2 + 0)*3;
                    int p1 = (i*2 + 1)*3;
                    offMeshConClass[i*2 + 0] = ClassifyOffMeshPoint(param.OffMeshConVerts[p0 + 0],
                                                                    param.OffMeshConVerts[p0 + 1],
                                                                    param.OffMeshConVerts[p0 + 2], bmin, bmax);
                    offMeshConClass[i * 2 + 1] = ClassifyOffMeshPoint(param.OffMeshConVerts[p1 + 0],
                                                                    param.OffMeshConVerts[p1 + 1],
                                                                    param.OffMeshConVerts[p1 + 2], bmin, bmax);

                    if (offMeshConClass[i*2 + 0] == 0xff)
                    {
                        if (param.OffMeshConVerts[p0 + 1] < bmin[1] || param.OffMeshConVerts[p0 + 1] > bmax[1])
                            offMeshConClass[i*2 + 0] = 0;
                    }

                    if (offMeshConClass[i*2 + 0] == 0xff)
                        offMeshConLinkCount++;
                    if (offMeshConClass[i*2 + 1] == 0xff)
                        offMeshConLinkCount++;

                    if (offMeshConClass[i*2 + 0] == 0xff)
                        storedOffMeshConCount++;
                }
            }

            int totPolyCount = param.PolyCount + storedOffMeshConCount;
            int totVertCount = param.VertCount + storedOffMeshConCount*2;

            int edgeCount = 0;
            int portalCount = 0;
            for (int i = 0; i < param.PolyCount; i++)
            {
                int p = i*2*nvp;
                for (int j = 0; j < nvp; j++)
                {
                    if (param.Polys[p + j] == PolyMesh.MeshNullIdx) break;
                    edgeCount++;

                    if ((param.Polys[p + nvp + j] & 0x8000) != 0)
                    {
                        int dir = param.Polys[p + nvp + j] & 0xf;
                        if (dir != 0xf)
                            portalCount++;
                    }
                }
            }

            int maxLinkCount = edgeCount + portalCount*2 + offMeshConLinkCount*2;

            int uniqueDetailVertCount = 0;
            int detailTryCount = 0;
            if (param.DetailMeshes != null)
            {
                detailTryCount = param.DetailTriCount;
                for (int i = 0; i < param.PolyCount; i++)
                {
                    int p = i*nvp*2;
                    int ndv = (int)param.DetailMeshes[i*4 + 1];
                    int nv = 0;
                    for (int j = 0; j < nvp; j++)
                    {
                        if (param.Polys[p + j] == PolyMesh.MeshNullIdx) break;
                        nv++;
                    }
                    ndv -= nv;
                    uniqueDetailVertCount += ndv;
                }
            }
            else
            {
                uniqueDetailVertCount = 0;
                detailTryCount = 0;
                for (int i = 0; i < param.PolyCount; i++)
                {
                    int p = i*nvp*2;
                    int nv = 0;
                    for (int j = 0; j < nvp; j++)
                    {
                        if (param.Polys[p + j] == PolyMesh.MeshNullIdx) break;
                        nv++;
                    }
                    detailTryCount += nv - 2;
                }
            }

            // Initialize the header and all nav data.
            Header = new MeshHeader
                         {
                             Magic = Helper.NavMeshMagic,
                             Version = Helper.NavMeshVersion,
                             X = param.TileX,
                             Y = param.TileY,
                             Layer = param.TileLayer,
                             UserId = param.UserId,
                             PolyCount = totPolyCount,
                             VertCount = totVertCount,
                             MaxLinkCount = maxLinkCount,
                             DetailMeshCount = param.PolyCount,
                             DetailVertCount = uniqueDetailVertCount,
                             DetailTriCount = detailTryCount,
                             BVQuantFactor = 1.0f/param.Cs,
                             OffMeshBase = param.PolyCount,
                             WalkableHeight = param.WalkableHeight,
                             WalkableRadius = param.WalkableRadius,
                             WalkableClimb = param.WalkableClimb,
                             OffMeshConCount = storedOffMeshConCount,
                             BVNodeCount = param.BuildBvTree ? param.PolyCount*2 : 0,
                             BMin = new float[3],
                             BMax = new float[3]
                         };
            Array.Copy(param.BMin, Header.BMin, 3);
            Array.Copy(param.BMax, Header.BMax, 3);

            NavVerts = new float[totVertCount*3];
            NavPolys = new Poly[totPolyCount];
            for (int i = 0; i < totPolyCount; i++)
            {
                NavPolys[i] = new Poly();
            }
            NavLinks = new Link[maxLinkCount];
            for (int i = 0; i < maxLinkCount; i++)
            {
                NavLinks[i] = new Link();
            }
            NavDMeshes = new PolyDetail[param.PolyCount];
            for (int i = 0; i < param.PolyCount; i++)
            {
                NavDMeshes[i] = new PolyDetail();
            }
            NavDVerts = new float[3*uniqueDetailVertCount];
            NavDTris = new short[4*detailTryCount];
            NavBvTree = param.BuildBvTree ? new BVNode[param.PolyCount*2] : new BVNode[0];
            if (param.BuildBvTree)
            {
                for (int i = 0; i < param.PolyCount*2; i++)
                {
                    NavBvTree[i] = new BVNode();
                }
            }
            OffMeshCons = new OffMeshConnection[storedOffMeshConCount];
            for (int i = 0; i < storedOffMeshConCount; i++)
            {
                OffMeshCons[i] = new OffMeshConnection();
            }


            int offMeshVertsBase = param.VertCount;
            int offMeshPolyBase = param.PolyCount;

            // store vertices
            // Mesh
            for (int i = 0; i < param.VertCount; i++)
            {
                int iv = i*3;
                int v = i*3;
                NavVerts[v + 0] = param.BMin[0] + param.Verts[iv + 0]*param.Cs;
                NavVerts[v + 1] = param.BMin[1] + param.Verts[iv + 1]*param.Ch;
                NavVerts[v + 2] = param.BMin[2] + param.Verts[iv + 2]*param.Cs;
            }
            // off-link
            int n = 0;
            for (int i = 0; i < param.OffMeshConCount; i++)
            {
                if (offMeshConClass[i*2 + 0] == 0xff)
                {
                    int linkv = i*2*3;
                    int v = (offMeshVertsBase + n*2)*3;
                    Array.Copy(param.OffMeshConVerts, linkv, NavVerts, v, 3);
                    Array.Copy(param.OffMeshConVerts, linkv+3, NavVerts, v+3, 3);
                    n++;
                }
            }

            // store polygons
            // mesh
            int src = 0;
            for (int i = 0; i < param.PolyCount; i++)
            {
                Poly p = NavPolys[i];
                p.VertCount = 0;
                p.Flags = param.PolyFlags[i];
                p.Area = param.PolyAreas[i];
                p.Type = PolyTypeGround;
                for (int j = 0; j < nvp; j++)
                {
                    if (param.Polys[src + j] == PolyMesh.MeshNullIdx) break;
                    p.Verts[j] = param.Polys[src + j];
                    if ((param.Polys[src + nvp + j] & 0x8000) != 0)
                    {
                        int dir = param.Polys[src + nvp + j] & 0xf;
                        if (dir == 0xf)
                            p.Neis[j] = 0;
                        else if (dir == 0)
                            p.Neis[j] = ExtLink | 4;
                        else if (dir == 1)
                            p.Neis[j] = ExtLink | 2;
                        else if (dir == 2)
                            p.Neis[j] = ExtLink | 0;
                        else if (dir == 3)
                            p.Neis[j] = ExtLink | 6;
                    }
                    else
                    {
                        p.Neis[j] = param.Polys[src + nvp + j] + 1;
                    }
                    p.VertCount++;
                }
                src += nvp*2;
            }
            // off mesh
            n = 0;
            for (int i = 0; i < param.OffMeshConCount; i++)
            {
                if (offMeshConClass[i*2 + 0] == 0xff)
                {
                    Poly p = NavPolys[offMeshPolyBase + n];
                    p.VertCount = 2;
                    p.Verts[0] = (offMeshVertsBase + n*2 + 0);
                    p.Verts[1] = (offMeshVertsBase + n*2 + 1);
                    p.Flags = param.OffMeshConFlags[i];
                    p.Area = (short)param.OffMeshConAreas[i];
                    p.Type = PolyTypeOffMeshConnection;
                    n++;
                }
            }

            // Store detail meshes and verts
            if (param.DetailMeshes != null)
            {
                int vbase = 0;
                for (int i = 0; i < param.PolyCount; i++)
                {
                    PolyDetail dtl = NavDMeshes[i];
                    int vb = (int)param.DetailMeshes[i*4 + 0];
                    int ndv = (int)param.DetailMeshes[i*4 + 1];
                    int nv = NavPolys[i].VertCount;
                    dtl.VertBase = vbase;
                    dtl.VertCount = (short)(ndv - nv);
                    dtl.TriBase = param.DetailMeshes[i*4 + 2];
                    dtl.TriCount = (short)param.DetailMeshes[i*4 + 3];
                    if (ndv - nv > 0)
                    {
                        Array.Copy(param.DetailVerts, (vb+nv)*3, NavDVerts, vbase*3, (ndv-nv)*3);
                        vbase += (short) (ndv - nv);
                    }
                }
                Array.Copy(param.DetailTris, NavDTris, param.DetailTriCount*4);
            }
            else
            {
                // Create dummy detail mesh
                int tbase = 0;
                for (int i = 0; i < param.PolyCount; i++)
                {
                    PolyDetail dtl = NavDMeshes[i];
                    int nv = NavPolys[i].VertCount;
                    dtl.VertBase = 0;
                    dtl.VertCount = 0;
                    dtl.TriBase = tbase;
                    dtl.TriCount = (short)(nv - 2);
                    for (int j = 2; j < nv; j++)
                    {
                        int t = tbase*4;
                        NavDTris[t + 0] = 0;
                        NavDTris[t + 1] = (short) (j - 1);
                        NavDTris[t + 2] = (short) j;
                        NavDTris[t + 3] = (1 << 2);
                        if (j == 2) NavDTris[t + 3] |= (1 << 0);
                        if (j == nv - 1) NavDTris[t + 3] |= (1 << 4);
                        tbase++;
                    }
                }
            }

            // Store and create BVTree
            if (param.BuildBvTree)
            {
                CreateBVTree(param.Verts, param.VertCount, param.Polys, param.PolyCount, nvp, param.Cs, param.Ch, param.PolyCount*2);
            }

            // store off-mesh connections
            n = 0;
            for (int i = 0; i < param.OffMeshConCount; i++)
            {
                if (offMeshConClass[i*2 + 0] == 0xff)
                {
                    OffMeshConnection con = OffMeshCons[n];
                    con.Poly = offMeshPolyBase + n;
                    int endPts = i*2*3;
                    Array.Copy(param.OffMeshConVerts, endPts, con.Pos, 0, 3);
                    Array.Copy(param.OffMeshConVerts, endPts+3, con.Pos, 3, 3);
                    con.Rad = param.OffMeshConRad[i];
                    con.Flags = param.OffMeshConDir[i] > 0 ? OffMeshConBiDir : (short)0;
                    con.Side = offMeshConClass[i*2 + 1];
                    if (param.OffMeshConUserId != null)
                        con.UserId = param.OffMeshConUserId[i];
                    n++;
                }
            }
        }

        private int CreateBVTree(int[] verts, int nverts, int[] polys, int npolys, int nvp, float cs, float ch, int nnodes)
        {
            BVNode[] items = new BVNode[npolys];
            for (int i = 0; i < npolys; i++)
            {
                items[i] = new BVNode();
            }
            for (int i = 0; i < npolys; i++)
            {
                BVNode it = items[i];
                it.I = i;

                int[] p = new int[polys.Length - (i * nvp * 2)];
                Array.Copy(polys, i * nvp * 2, p, 0, p.Length);

                it.BMin[0] = it.BMax[0] = verts[p[0] * 3 + 0];
                it.BMin[1] = it.BMax[1] = verts[p[0] * 3 + 1];
                it.BMin[2] = it.BMax[2] = verts[p[0] * 3 + 2];

                for (int j = 1; j < nvp; j++)
                {
                    if (p[j] == PolyMesh.MeshNullIdx) break;
                    int x = verts[p[j] * 3 + 0];
                    int y = verts[p[j] * 3 + 1];
                    int z = verts[p[j] * 3 + 2];

                    if (x < it.BMin[0]) it.BMin[0] = x;
                    if (y < it.BMin[1]) it.BMin[1] = y;
                    if (z < it.BMin[2]) it.BMin[2] = z;

                    if (x > it.BMax[0]) it.BMax[0] = x;
                    if (y > it.BMax[1]) it.BMax[1] = y;
                    if (z > it.BMax[2]) it.BMax[2] = z;
                }
                it.BMin[1] = (int)Math.Floor(it.BMin[1] * ch / cs);
                it.BMax[1] = (int)Math.Ceiling(it.BMax[1] * ch / cs);
            }
            int curNode = 0;
            BVNode[] temp = NavBvTree;
            Subdivide(items, npolys, 0, npolys, ref curNode, ref temp);
            NavBvTree = temp;
            return curNode;
        }

        private void Subdivide(BVNode[] items, int nitems, int imin, int imax, ref int curNode, ref BVNode[] nodes)
        {
            int inum = imax - imin;
            int icur = curNode;

            BVNode node = nodes[curNode++];

            if (inum == 1)
            {
                node.BMin[0] = items[imin].BMin[0];
                node.BMin[1] = items[imin].BMin[1];
                node.BMin[2] = items[imin].BMin[2];

                node.BMax[0] = items[imin].BMax[0];
                node.BMax[1] = items[imin].BMax[1];
                node.BMax[2] = items[imin].BMax[2];

                node.I = items[imin].I;
            }
            else
            {
                int[] tempBMin = new int[3], tempBMax = new int[3];
                CalcExtends(items, nitems, imin, imax, ref tempBMin, ref tempBMax);

                Array.Copy(tempBMin, node.BMin, 3);
                Array.Copy(tempBMax, node.BMax, 3);

                int axis = LongestAxis(node.BMax[0] - node.BMin[0],
                                       node.BMax[1] - node.BMin[1],
                                       node.BMax[2] - node.BMin[2]);
                if (axis == 0)
                {
                    Array.Sort(items, imin, inum, new CompareBVNodeX());
                }
                else if (axis == 1)
                {
                    Array.Sort(items, imin, inum, new CompareBVNodeY());
                }
                else
                {
                    Array.Sort(items, imin, inum, new CompareBVNodeZ());
                }

                int isplit = imin + inum / 2;

                Subdivide(items, nitems, imin, isplit, ref curNode, ref nodes);
                Subdivide(items, nitems, isplit, imax, ref curNode, ref nodes);

                int iescape = curNode - icur;
                node.I = -iescape;
            }
        }

        private void CalcExtends(BVNode[] items, int nitems, int imin, int imax, ref int[] bmin, ref int[] bmax)
        {
            bmin[0] = items[imin].BMin[0];
            bmin[1] = items[imin].BMin[1];
            bmin[2] = items[imin].BMin[2];

            bmax[0] = items[imin].BMax[0];
            bmax[1] = items[imin].BMax[1];
            bmax[2] = items[imin].BMax[2];

            for (int i = imin + 1; i < imax; ++i)
            {
                BVNode it = items[i];
                if (it.BMin[0] < bmin[0]) bmin[0] = it.BMin[0];
                if (it.BMin[1] < bmin[1]) bmin[1] = it.BMin[1];
                if (it.BMin[2] < bmin[2]) bmin[2] = it.BMin[2];

                if (it.BMax[0] > bmax[0]) bmax[0] = it.BMax[0];
                if (it.BMax[1] > bmax[1]) bmax[1] = it.BMax[1];
                if (it.BMax[2] > bmax[2]) bmax[2] = it.BMax[2];
            }
        }

        private int LongestAxis(int x, int y, int z)
        {
            int axis = 0;
            float maxVal = x;
            if (y > maxVal)
            {
                axis = 1;
                maxVal = y;
            }
            if (z > maxVal)
            {
                axis = 2;
                maxVal = z;
            }
            return axis;
        }

        private short ClassifyOffMeshPoint(float ptx, float pty, float ptz, float[] bmin, float[] bmax)
        {
            const short XP = 1 << 0;
            const short ZP = 1 << 1;
            const short XM = 1 << 2;
            const short ZM = 1 << 3;

            short outcode = 0;
            outcode |= (ptx >= bmax[0]) ? XP : (short)0;
            outcode |= (ptz >= bmax[2]) ? ZP : (short)0;
            outcode |= (ptx < bmin[0]) ? XM : (short)0;
            outcode |= (ptz < bmin[2]) ? ZM : (short)0;

            switch (outcode)
            {
                case XP: return 0;
                case XP | ZP: return 1;
                case ZP: return 2;
                case XM | ZP: return 3;
                case XM: return 4;
                case XM | ZM: return 5;
                case ZM: return 6;
                case XP | ZM: return 7;
            }
            return 0xff;
        }
	}
}
