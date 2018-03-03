using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class BuildContourFlags
	{
	    public const int ContourTessWallEdges = 0x01;
	    public const int ContourTessAreaEdges = 0x02;
	}
}
