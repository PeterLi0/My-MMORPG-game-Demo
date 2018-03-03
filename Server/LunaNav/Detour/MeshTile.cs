using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class MeshTile
	{
        public long Salt { get; set; }

        public long LinksFreeList { get; set; }
        public MeshHeader Header { get; set; }
        public Poly[] Polys { get; set; }
        public float[] Verts { get; set; }
        public Link[] Links { get; set; }
        public PolyDetail[] DetailMeshes { get; set; }

        public float[] DetailVerts { get; set; }
        public short[] DetailTris { get; set; }
        public BVNode[] BVTree { get; set; }

        public OffMeshConnection[] OffMeshCons { get; set; }

        public NavMeshBuilder Data { get; set; }
        public int Flags { get; set; }
        public MeshTile Next { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Salt: {0}\n", Salt);
            foreach (PolyDetail pd in DetailMeshes)
            {
                builder.AppendFormat("DetailMesh: {0}\n", pd);
            }

            return builder.ToString();
        }
	}
}
