using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class Node
	{
        public float[] Pos { get; set; }
        public float Cost { get; set; }
        public float Total { get; set; }
        public long PIdx { get; set; }
        public long Flags { get; set; }
	    public long Id { get; set; }

	    public static int NullIdx = ~0;
	    public static int NodeOpen = 0x01;
	    public static int NodeClosed = 0x02;

        public Node()
        {
            Pos = new float[3];
        }
	}
}
