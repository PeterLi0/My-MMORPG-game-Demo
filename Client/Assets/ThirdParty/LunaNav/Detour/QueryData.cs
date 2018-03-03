using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class QueryData
	{
        public Status Status { get; set; }
        public Node LastBestNode { get; set; }
        public float LastBestNodeCost { get; set; }
        public long StartRef { get; set; }
        public long EndRef { get; set; }
        public float[] StartPos { get; set; }
        public float[] EndPos { get; set; }
        public QueryFilter Filter { get; set; }

        public QueryData()
        {
            Status = Status.Failure;
            StartPos = new float[3];
            EndPos = new float[3];
        }
	}
}
