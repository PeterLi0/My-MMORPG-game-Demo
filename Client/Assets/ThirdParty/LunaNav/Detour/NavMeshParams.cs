using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class NavMeshParams
	{
        public float[] Orig { get; set; }
        public float TileWidth { get; set; }
        public float TileHeight { get; set; }
        public int MaxTiles { get; set; }
        public int MaxPolys { get; set; }

        public NavMeshParams()
        {
            Orig = new float[3];
        }
	}
}
