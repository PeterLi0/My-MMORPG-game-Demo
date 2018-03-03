using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class MeshHeader
	{
	    public int Magic { get; set; }
        public int Version { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Layer { get; set; }
        public long UserId { get; set; }
        public int PolyCount { get; set; }
        public int VertCount { get; set; }
        public int MaxLinkCount { get; set; }
        public int DetailMeshCount { get; set; }

        public int DetailVertCount { get; set; }

        public int DetailTriCount { get; set; }
        public int BVNodeCount { get; set; }
        public int OffMeshConCount { get; set; }
        public int OffMeshBase { get; set; }
        public float WalkableHeight { get; set; }
        public float WalkableRadius { get; set; }
        public float WalkableClimb { get; set; }
        public float[] BMin { get; set; }
        public float[] BMax { get; set; }
        public long TileRef { get; set; }

        public float BVQuantFactor { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("PolyCount: {0}, VertCount: {1}, DetailVertCount: {2}\n", PolyCount, VertCount,
                                 DetailVertCount);

            return builder.ToString();
        }
	}
}
