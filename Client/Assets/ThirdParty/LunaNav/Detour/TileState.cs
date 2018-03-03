using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class TileState
	{
        public int Magic { get; set; }
        public int Version { get; set; }
        public long Ref { get; set; }

        public PolyState[] PolyStates { get; set; }
	}
}
