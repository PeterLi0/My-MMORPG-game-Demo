using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
    public class BoundsItem
    {
        public float[] bmin { get; set; }
        public float[] bmax { get; set; }
        public int i { get; set; }

        public BoundsItem()
        {
            bmin = new float[2];
            bmax = new float[2];
            i = 0;
        }

    }
}
