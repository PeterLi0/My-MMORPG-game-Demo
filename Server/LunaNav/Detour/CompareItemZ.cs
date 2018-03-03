using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    public class CompareBVNodeZ : IComparer
    {
        int IComparer.Compare(object va, object vb)
        {
            BVNode a = va as BVNode;
            BVNode b = va as BVNode;
            if (a != null && b != null)
            {

                if (a.BMin[2] < b.BMin[2])
                    return -1;
                if (a.BMin[2] > b.BMin[2])
                    return 1;
            }
            return 0;
        }
    }
}
