using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class Edge
	{
        public int[] Vert = new int[2];
        public int[] PolyEdge = new int[2];
        public int[] Poly = new int[2];
	}
}
