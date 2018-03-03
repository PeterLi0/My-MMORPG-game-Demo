namespace LunaNav
{
    public class ObstacleAvoidanceParams
    {
        public float velBias;
        public float weightDesVel;
        public float weightCurVel;
        public float weightSide;
        public float weightToi;
        public float horizTime;
        public short gridSize;
        public short adaptiveDivs;
        public short adaptiveRings;
        public short adaptiveDepth;

        public ObstacleAvoidanceParams()
        {

        }

        public ObstacleAvoidanceParams(ObstacleAvoidanceParams param)
        {
            velBias = param.velBias;
            weightDesVel = param.weightDesVel;
            weightCurVel = param.weightCurVel;
            weightSide = param.weightSide;
            weightToi = param.weightToi;
            horizTime = param.horizTime;
            gridSize = param.gridSize;
            adaptiveDivs = param.adaptiveDivs;
            adaptiveRings = param.adaptiveRings;
            adaptiveDepth = param.adaptiveDepth;
        }
    }
}