using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    public class CompareBVNodeX : IComparer
    {
        int IComparer.Compare(object va, object vb)
        {
            BVNode a = va as BVNode;
            BVNode b = va as BVNode;
            if (a != null && b != null)
            {

                if (a.BMin[0] < b.BMin[0])
                    return -1;
                if (a.BMin[0] > b.BMin[0])
                    return 1;
            }
            return 0;
        }
    }
}
