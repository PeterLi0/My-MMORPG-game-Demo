using System;
using System.Collections;


namespace LunaNav
{
    [Serializable]
    public class ChunkyTriMesh
    {
        public ChunkyTriMeshNode[] Nodes { get; set; }
        public int NNodes { get; set; }
        public int[] Tris { get; set; }
        public int NTris { get; set; }

        public int MaxTrisPerChunk { get; set; }

        public ChunkyTriMesh(RecastVertex[] verts, int[] tris, int ntris, int trisPerChunk)
        {
            int nchunks = (ntris + trisPerChunk - 1)/trisPerChunk;
            Nodes = new ChunkyTriMeshNode[nchunks*4];
            Tris = new int[ntris*3];

            NTris = ntris;

            BoundsItem[] items = new BoundsItem[ntris];

            for (int i = 0; i < ntris; i++)
            {
                int t = i*3;
                items[i] = new BoundsItem();
                BoundsItem it = items[i];
                it.i = i;
                it.bmin[0] = it.bmax[0] = verts[tris[t]].X;
                it.bmin[1] = it.bmax[1] = verts[tris[t]].Z;
                for (int j = 1; j < 3; j++)
                {
                    int v = tris[t+j];
                    if (verts[v].X < it.bmin[0]) it.bmin[0] = verts[v].X;
                    if (verts[v].Z < it.bmin[1]) it.bmin[1] = verts[v].Z;

                    if (verts[v].X > it.bmax[0]) it.bmax[0] = verts[v].X;
                    if (verts[v].Z > it.bmax[1]) it.bmax[1] = verts[v].Z;
                }
            }

            int curTri = 0;
            int curNode = 0;

            Subdivide(ref items, ntris, 0, ntris, trisPerChunk, ref curNode, nchunks*4, ref curTri, tris);

            NNodes = curNode;

            MaxTrisPerChunk = 0;
            for (int i = 0; i < NNodes; i++)
            {
                bool isLeaf = Nodes[i].i >= 0;
                if (!isLeaf) continue;
                if (Nodes[i].n > MaxTrisPerChunk)
                    MaxTrisPerChunk = Nodes[i].n;
            }
        }

        public int GetChunksOverlappingRect(float[] bmin, float[] bmax, ref int[] ids, int maxIds)
        {
            int i = 0;
            int n = 0;
            while (i < NNodes)
            {
                ChunkyTriMeshNode node = Nodes[i];
                bool overlap = CheckOverlapRect(bmin, bmax, node.bmin, node.bmax);
                bool isLeafNode = node.i >= 0;

                if (isLeafNode && overlap)
                {
                    if (n < maxIds)
                    {
                        ids[n] = i;
                        n++;
                    }
                }

                if (overlap || isLeafNode)
                {
                    i++;
                }
                else
                {
                    int escapeIndex = -node.i;
                    i += escapeIndex;
                }
            }
            return n;
        }

        private bool CheckOverlapRect(float[] amin, float[] amax, float[] bmin, float[] bmax)
        {
            bool overlap = true;
            overlap = (amin[0] > bmax[0] || amax[0] < bmin[0]) ? false : overlap;
            overlap = (amin[1] > bmax[1] || amax[1] < bmin[1]) ? false : overlap;
            return overlap;
        }

        private void Subdivide(ref BoundsItem[] items, int nitems, int imin, int imax, int trisPerChunk, ref int curNode, int maxNodes, ref int curTri, int[] tris)
        {
            int inum = imax - imin;
            int icur = curNode;

            if (curNode > maxNodes)
                return;

            var node = Nodes[curNode];
            if (Nodes[curNode] == null)
            {
                node = new ChunkyTriMeshNode();
                Nodes[curNode] = node;
            }
            curNode++;

            if (inum <= trisPerChunk)
            {
                CalcExtends(items, nitems, imin, imax, ref node);

                node.i = curTri;
                node.n = inum;

                for (int i = imin; i < imax; i++)
                {
                    int src = items[i].i*3;
                    int dst = curTri*3;
                    curTri++;
                    Tris[dst + 0] = tris[src + 0];
                    Tris[dst + 1] = tris[src + 1];
                    Tris[dst + 2] = tris[src + 2];
                }
            }
            else
            {
                CalcExtends(items, nitems, imin, imax, ref node);

                int axis = LongestAxis(node.bmax[0] - node.bmin[0], node.bmax[1] - node.bmin[1]);

                if (axis == 0)
                {
                    // sort along x-axis
                    Array.Sort(items, imin, inum, new CompareItemX());
                }
                else if (axis == 1)
                {
                    // sort along y-axis
                    Array.Sort(items, imin, inum, new CompareItemY());
                }

                int isplit = imin + inum/2;

                Subdivide(ref items, nitems, imin, isplit, trisPerChunk, ref curNode, maxNodes, ref curTri, tris);
                Subdivide(ref items, nitems, isplit, imax, trisPerChunk, ref curNode, maxNodes, ref curTri, tris);

                int iescape = curNode - icur;
                node.i = -iescape;
            }
        }

        private void CalcExtends(BoundsItem[] items, int nitems, int imin, int imax, ref ChunkyTriMeshNode node)
        {
            node.bmin[0] = items[imin].bmin[0];
            node.bmin[1] = items[imin].bmin[1];

            node.bmax[0] = items[imin].bmax[0];
            node.bmax[1] = items[imin].bmax[1];

            for (int i = imin+1; i < imax; i++)
            {
                BoundsItem it = items[i];
                if (it.bmin[0] < node.bmin[0]) node.bmin[0] = it.bmin[0];
                if (it.bmin[1] < node.bmin[1]) node.bmin[1] = it.bmin[1];

                if (it.bmax[0] > node.bmax[0]) node.bmax[0] = it.bmax[0];
                if (it.bmax[1] > node.bmax[1]) node.bmax[1] = it.bmax[1];
            }
        }

        private int LongestAxis(float x, float y)
        {
            return y > x ? 1 : 0;
        }
    }

    public class CompareItemX : IComparer
    {
        int IComparer.Compare(object va, object vb)
        {
            BoundsItem a = va as BoundsItem;
            BoundsItem b = vb as BoundsItem;
            if (a != null && b != null)
            {

                if (a.bmin[0] < b.bmin[0])
                    return -1;
                if (a.bmin[0] > b.bmin[0])
                    return 1;
            }
            return 0;
        }
    }

    public class CompareItemY : IComparer
    {
        int IComparer.Compare(object va, object vb)
        {
            BoundsItem a = va as BoundsItem;
            BoundsItem b = vb as BoundsItem;
            if (a != null && b != null)
            {

                if (a.bmin[1] < b.bmin[1])
                    return -1;
                if (a.bmin[1] > b.bmin[1])
                    return 1;
            }
            return 0;
        }
    }
}
