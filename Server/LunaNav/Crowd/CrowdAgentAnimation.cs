namespace LunaNav
{
    public class CrowdAgentAnimation
    {
        public bool Active;
        public float[] InitPos = new float[3];
        public float[] StartPos = new float[3];
        public float[] EndPos = new float[3];
        public long PolyRef;
        public float T;
        public float TMax;
    }
}