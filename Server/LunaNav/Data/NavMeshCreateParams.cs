using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
	public class NavMeshCreateParams
	{
        public int[] Verts { get; set; }
        public int VertCount { get; set; }
        public int[] Polys { get; set; }
        public int[] PolyFlags { get; set; }
        public short[] PolyAreas { get; set; }
        public int PolyCount { get; set; }
        public int Nvp { get; set; }

        public long[] DetailMeshes { get; set; }
        public float[] DetailVerts { get; set; }
        public int DetailVertsCount { get; set; }
        public short[] DetailTris { get; set; }
        public int DetailTriCount { get; set; }

        public float[] OffMeshConVerts { get; set; }
        public float[] OffMeshConRad { get; set; }
        public int[] OffMeshConFlags { get; set; }
        public int[] OffMeshConAreas { get; set; }

        public int[] OffMeshConDir { get; set; }
        public long[] OffMeshConUserId { get; set; }
        public int OffMeshConCount { get; set; }

        public long UserId { get; set; }
        public int TileX { get; set; }
        public int TileY { get; set; }
        public int TileLayer { get; set; }
        public float[] BMin { get; set; }
        public float[] BMax { get; set; }

        public float WalkableHeight { get; set; }
        public float WalkableRadius { get; set; }
        public float WalkableClimb { get; set; }
        public float Cs { get; set; }
        public float Ch { get; set; }

        public bool BuildBvTree { get; set; }
	}
}
