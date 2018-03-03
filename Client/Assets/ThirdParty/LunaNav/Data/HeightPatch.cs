using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class HeightPatch
	{
	    public int[] Data { get; set; }
        public int XMin { get; set; }
        public int YMin { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
	}
}
