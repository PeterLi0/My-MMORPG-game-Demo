using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class OffMeshConnection
	{
        public float[] Pos { get; set; }
        public float Rad { get; set; }
        public int Poly { get; set; }
        public short Flags { get; set; }
        public short Side { get; set; }
        public long UserId { get; set; }

        public OffMeshConnection()
        {
            Pos = new float[6];
        }
	}
}
