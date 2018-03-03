using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class Region
	{
        public int SpanCount { get; set; }
        public int Id { get; set; }
	    public short AreaType { get; set; }
        public bool Remap { get; set; }
        public bool Visited { get; set; }
        public IntArray Connections { get; set; }
        public IntArray Floors { get; set; }

        public Region(int i)
        {
            SpanCount = 0;
            Id = i;
            AreaType = 0;
            Remap = false;
            Visited = false;
            Connections = new IntArray();
            Floors = new IntArray();
        }
	}
}
