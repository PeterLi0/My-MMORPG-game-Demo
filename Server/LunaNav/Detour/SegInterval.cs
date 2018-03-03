using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class SegInterval
	{
	    public long RefId { get; set; }
        public short TMin { get; set; }
        public short TMax { get; set; }
	}
}
