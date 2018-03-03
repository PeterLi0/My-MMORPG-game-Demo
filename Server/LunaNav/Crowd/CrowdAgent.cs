using System;



namespace LunaNav
{
    public class CrowdAgent
    {
        public static int CrowdAgentMaxNeighbors = 6;
        public static int CrowdAgentMaxCorners = 4;

        public bool Active;
        public CrowdAgentState State;

        public PathCorridor Corridor;
        public LocalBoundary Boundary;

        public float TopologyOptTime;

        public CrowdNeighbor[] Neis = new CrowdNeighbor[CrowdAgentMaxNeighbors];
        public int NNeis;
        public float DesiredSpeed;

        public float[] npos = new float[3];
        public float[] disp = new float[3];
        public float[] dvel = new float[3];
        public float[] nvel = new float[3];
        public float[] vel = new float[3];

        public CrowdAgentParams Param;
        public float[] CornerVerts = new float[CrowdAgentMaxCorners*3];
        public short[] CornerFlags = new short[CrowdAgentMaxCorners];
        public long[] CornerPolys = new long[CrowdAgentMaxCorners];

        public int NCorners;
        public MoveRequestState TargetState;
        public long TargetRef;
        public float[] TargetPos = new float[3];
        public long TargetPathQRef;
        public bool TargetReplan;
        public float TargetReplanTime;

        public CrowdAgent()
        {
            Corridor = new PathCorridor();
            Boundary = new LocalBoundary();
            for (int i = 0; i < CrowdAgentMaxNeighbors; i++)
            {
                Neis[i] = new CrowdNeighbor();
            }
        }

        public void Integrate(float dt)
        {
            float maxDelta = Param.MaxAcceleration*dt;
            float[] dv = Helper.VSub(nvel[0], nvel[1], nvel[2], vel[0], vel[1], vel[2]);

            float ds = Helper.VLen(dv);
            if (ds > maxDelta)
                dv = Helper.VScale(dv[0], dv[1], dv[2], maxDelta/ds);

            vel = Helper.VAdd(vel[0], vel[1], vel[2], dv[0], dv[1], dv[2]);

            if(Helper.VLen(vel) > 0.0001f)
                Helper.VMad(ref npos, npos, vel, dt);
            else
            {
                Helper.VSet(ref vel, 0, 0, 0);
            }
        }

        public bool OverOffMeshConnection(float radius)
        {
            if (NCorners <= 0)
                return false;

            bool offMeshConnection = (CornerFlags[NCorners - 1] & NavMeshQuery.StraightPathOffMeshConnection) != 0;
            if (offMeshConnection)
            {
                float distSq = Helper.VDist2DSqr(npos[0], npos[1], npos[2], CornerVerts[(NCorners - 1)*3 + 0], CornerVerts[(NCorners - 1)*3 + 1], CornerVerts[(NCorners - 1)*3 + 2]);
                if (distSq < radius*radius)
                    return true;
            }
            return false;
        }

        public float GetDistanceToGoal(float range)
        {
            if (NCorners <= 0)
                return range;

            bool endOfPath = (CornerFlags[NCorners - 1] & NavMeshQuery.StraightPathEnd) != 0;
            if (endOfPath)
            {
                float[] temp = new float[3];
                Array.Copy(CornerVerts, (NCorners-1)*3, temp, 0, 3);
                return Math.Min(Helper.VDist2D(npos, temp), range);
            }

            return range;
        }

        public void CalcSmoothSteerDirection(ref float[] dir)
        {
            if (NCorners <= 0)
            {
                Helper.VSet(ref dir, 0,0,0);
                return;
            }

            int ip0 = 0;
            int ip1 = Math.Min(1, NCorners - 1);
            float[] p0 = new float[3], p1 = new float[3];

            Array.Copy(CornerVerts, ip0*3, p0, 0, 3);
            Array.Copy(CornerVerts, ip1*3, p1, 0, 3);

            float[] dir0 = Helper.VSub(p0[0], p0[1], p0[2], npos[0], npos[1], npos[2]);
            float[] dir1 = Helper.VSub(p1[0], p1[1], p1[2], npos[0], npos[1], npos[2]);
            dir0[1] = 0;
            dir1[1] = 0;

            float len0 = Helper.VLen(dir0);
            float len1 = Helper.VLen(dir1);

            if (len1 > 0.001f)
                dir1 = Helper.VScale(dir1[0], dir1[1], dir1[2], 1.0f/len1);

            dir[0] = dir0[0] - dir1[0]*len0*0.5f;
            dir[1] = 0;
            dir[2] = dir0[2] - dir1[2]*len0*0.5f;

            Helper.VNormalize(ref dir);
        }

        public void CalcStraightSteerDirection(ref float[] dir)
        {
            if (NCorners <= 0)
            {
                Helper.VSet(ref dir, 0,0,0);
                return;
            }

            dir = Helper.VSub(CornerVerts[0], CornerVerts[1], CornerVerts[2], npos[0], npos[1], npos[2]);
            dir[1] = 0;
            Helper.VNormalize(ref dir);
        }
    }

}