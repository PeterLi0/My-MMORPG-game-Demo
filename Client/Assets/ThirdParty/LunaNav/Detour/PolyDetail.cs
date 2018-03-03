using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class PolyDetail
	{
        public long VertBase { get; set; }
        public long TriBase { get; set; }
        public short VertCount { get; set; }
        public short TriCount { get; set; }

        public override string ToString()
        {
            return String.Format("VertBase: {0}, VertCount: {1}, TriBase: {2}, TriCount: {3}", VertBase, VertCount,
                                 TriBase, TriCount);
        }
	}
}
