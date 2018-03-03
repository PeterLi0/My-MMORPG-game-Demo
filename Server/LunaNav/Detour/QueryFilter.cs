using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LunaNav
{
    [Serializable]
	public class QueryFilter
	{

        public QueryFilter()
        {
            _areaCost = new float[NavMeshBuilder.MaxAreas];
            for (int i = 0; i < NavMeshBuilder.MaxAreas; i++)
            {
                _areaCost[i] = 1.0f;
            }
            ExcludeFlags = 0;
            IncludeFlags = 0xffff;
        }

        private float[] _areaCost;
	    public int IncludeFlags { get; set; }
        public int ExcludeFlags { get; set; }

        public bool PassFilter(long refId, MeshTile tile, Poly poly)
        {
            return (poly.Flags & IncludeFlags) != 0 && (poly.Flags & ExcludeFlags) == 0;
        }

        public float GetCost(float pax, float pay, float paz, float pbx, float pby, float pbz,
                             long prevRef, MeshTile prevTile, Poly prevPoly,
                             long curRef, MeshTile curTile, Poly curPoly,
                             long nextRef, MeshTile nextTile, Poly nextPoly)
        {
            return Helper.VDist(pax, pay, paz, pbx, pby, pbz)*_areaCost[curPoly.Area];
        }

        public float GetAreaCost(int i)
        {
            return _areaCost[i];
        }

        public void SetAreaCost(int i, float cost)
        {
            _areaCost[i] = cost;
        }
	}
}
