using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class Poly
	{
        public long FirstLink { get; set; }
        public int[] Verts { get; set; }
        public int[] Neis { get; set; }
        public int Flags { get; set; }
        public short VertCount { get; set; }
	    public short _areaAndType;

	    public short Area
	    {
	        get { return (short)(_areaAndType & 0x3f); }
            set { _areaAndType = (short)((_areaAndType & 0xc0) | (value & 0x3f)); }
	    }

	    public short Type
	    {
            get { return (short)(_areaAndType >> 6); }
            set { _areaAndType = (short) ((_areaAndType & 0x3f) | (value << 6)); }
	    }

        public Poly()
        {
            Verts = new int[NavMeshBuilder.VertsPerPoly];
            Neis = new int[NavMeshBuilder.VertsPerPoly];
        }
	}
}
