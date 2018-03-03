using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public struct CompactSpan
	{
        public int Y { get; set; }
        public int Reg { get; set; }
        public long Con { get; set; }
        //private int[] Con;
        public int H { get; set; }

	    public void SetCon(int dir, int i)
	    {
            //if (Con == null)
            //{
            //    Con = new int[4];
            //}
            //Con[dir] = i;
            int shift = dir * 6;
            long con = Con;
            Con = (uint)(con & ~(0x3f << shift)) | ((i & 0x3f) << shift);
	    }

        public int GetCon(int dir)
        {
            //if (Con == null)
            //{
            //    Con = new int[4];
            //}
            //return Con[dir];
            int shift = dir * 6;
            return (int)((Con >> shift) & 0x3f);
        }
	}
}
