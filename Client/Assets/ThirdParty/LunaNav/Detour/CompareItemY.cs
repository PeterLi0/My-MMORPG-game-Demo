using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    public class CompareBVNodeY : IComparer
    {
        int IComparer.Compare(object va, object vb)
        {
            BVNode a = va as BVNode;
            BVNode b = va as BVNode;
            if (a != null && b != null)
            {

                if (a.BMin[1] < b.BMin[1])
                    return -1;
                if (a.BMin[1] > b.BMin[1])
                    return 1;
            }
            return 0;
        }
    }
}
