using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class Link
	{
        //public PolyRef Ref { get; set; }
        public long Ref { get; set; }
        public long Next { get; set; }
        public short Edge { get; set; }
        public short Side { get; set; }
        public short BMin { get; set; }
        public short BMax { get; set; }
	}
}
