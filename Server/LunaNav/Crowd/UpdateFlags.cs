using System;

namespace LunaNav
{
    [Flags]
    public enum UpdateFlags
    {
        AnticipateTurns = 1,
        ObstacleAvoidance = 2,
        Separation = 4,
        OptimizeVisibility = 8,
        OptimizeTopology = 16
    }
}