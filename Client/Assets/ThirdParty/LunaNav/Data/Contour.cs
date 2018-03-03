using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class Contour
	{
        public int[] Verts { get; set; }
        public int NVerts { get; set; }
        public int[] RVerts { get; set; }
        public int NRVerts { get; set; }
        public int Reg { get; set; }
        public short Area { get; set; }
	}
}
