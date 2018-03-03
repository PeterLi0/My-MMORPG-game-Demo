using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class BVNode
	{
        public int[] BMin { get; set; }
        public int[] BMax { get; set; }
        public int I { get; set; }

        public BVNode()
        {
            BMin = new int[3];
            BMax = new int[3];
        }
	}
}
