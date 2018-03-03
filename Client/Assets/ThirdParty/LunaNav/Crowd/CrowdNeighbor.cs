namespace LunaNav
{
    public class CrowdNeighbor
    {
        public int Idx;
        public float Dist;

        public CrowdNeighbor Clone()
        {
            return this.MemberwiseClone() as CrowdNeighbor;
        }
    }
}