using System;

using System.Linq;


namespace LunaNav
{
    public class Crowd
    {
        public static int CrowdMaxObstAvoidanceParams = 8;

        private int _maxAgents;
        private CrowdAgent[] _agents;
        private CrowdAgent[] _activeAgents;
        private CrowdAgentAnimation[] _agentAnims;

        private PathQueue _pathq;
        private ObstacleAvoidanceParams[] _obstacleQueryParams = new ObstacleAvoidanceParams[CrowdMaxObstAvoidanceParams];
        private ObstacleAvoidanceQuery _obstacleQuery;

        private ProximityGrid _grid;
        private long[] _pathResult;
        private int _maxPathResult;

        private float[] _ext = new float[3];
        private QueryFilter _filter;

        private float _maxAgentRadius;
        private int _velocitySampleCount;
        private NavMeshQuery _navQuery;

        public static int MaxPathQueueNodes = 4096;
        public static int MaxCommonNodes = 512;
        public static int MaxItersPerUpdate = 100;
        public Crowd()
        {
            _maxAgents = 0;
            _agents = null;
            _activeAgents = null;
            _agentAnims = null;
            _obstacleQuery = null;
            _grid = null;
            _pathResult = null;
            _maxPathResult = 0;
            _maxAgentRadius = 0;
            _velocitySampleCount = 0;
            _navQuery = null;
            _filter = new QueryFilter();
        }

        private void UpdateMoveRequest(float dt)
        {
            int PathMaxAgents = 8;
            CrowdAgent[] queue = new CrowdAgent[PathMaxAgents];
            int nqueue = 0;
            Status status = 0;

            for (int i = 0; i < _maxAgents; i++)
            {
                CrowdAgent ag = _agents[i];
                if (!ag.Active) continue;
                if (ag.State == CrowdAgentState.Invalid) continue;
                if (ag.TargetState == MoveRequestState.TargetNone || ag.TargetState == MoveRequestState.TargetVelocity)
                    continue;

                if (ag.TargetState == MoveRequestState.TargetRequesting)
                {
                    long[] path = ag.Corridor.GetPath;
                    int npath = ag.Corridor.PathCount;

                    int MaxRes = 32;
                    float[] reqPos = new float[3];
                    long[] reqPath = new long[MaxRes];
                    int reqPathCount = 0;

                    int MaxIter = 20;
                    _navQuery.InitSlicedFindPath(path[0], ag.TargetRef, ag.npos, ag.TargetPos, _filter);

                    int doneIters = 0;
                    _navQuery.UpdateSlicedFindPath(MaxIter, ref doneIters);

                    if (ag.TargetReplan)
                        status = _navQuery.FinalizeSlicedFindPathPartial(path, npath, ref reqPath, ref reqPathCount, MaxRes);
                    else
                        status = _navQuery.FinalizeSlicedFindPath(ref reqPath, ref reqPathCount, MaxRes);

                    if ((status & Status.Failure) == 0 && reqPathCount > 0)
                    {
                        if (reqPath[reqPathCount - 1] != ag.TargetRef)
                        {
                            status = _navQuery.ClosestPointOnPoly(reqPath[reqPathCount - 1], ag.TargetPos, ref reqPos);
                            if ((status & Status.Failure) != 0)
                                reqPathCount = 0;
                        }
                        else
                            Helper.VCopy(ref reqPos, ag.TargetPos);
                    }
                    else
                    {
                        reqPathCount = 0;
                    }

                    if (reqPathCount <= 0)
                    {
                        Helper.VCopy(ref reqPos, ag.npos);
                        reqPos[0] = path[0];
                        reqPathCount = 1;
                    }

                    ag.Corridor.SetCorridor(reqPos, reqPath, reqPathCount);
                    ag.Boundary.Reset();

                    if (reqPath[reqPathCount - 1] == ag.TargetRef)
                    {
                        ag.TargetState = MoveRequestState.TargetValid;
                        ag.TargetReplanTime = 0.0f;
                    }
                    else
                    {
                        ag.TargetState = MoveRequestState.TargetWaitingForQueue;
                    }
                }

                if (ag.TargetState == MoveRequestState.TargetWaitingForQueue)
                {
                    nqueue = AddToPlanQueue(ag, ref queue, nqueue, PathMaxAgents);
                }
            }

            for (int i = 0; i < nqueue; i++)
            {
                CrowdAgent ag = queue[i];
                ag.TargetPathQRef = _pathq.Request(ag.Corridor.LastPoly, ag.TargetRef, ag.Corridor.Target, ag.TargetPos, _filter);
                if(ag.TargetPathQRef != PathQueue.PathQInvalid)
                    ag.TargetState = MoveRequestState.TargetWaitingForPath;
            }

            _pathq.Update(MaxItersPerUpdate);

            for (int i = 0; i < _maxAgents; i++)
            {
                CrowdAgent ag = _agents[i];
                if (!ag.Active) continue;
                if (ag.TargetState == MoveRequestState.TargetNone || ag.TargetState == MoveRequestState.TargetVelocity)
                    continue;

                if (ag.TargetState == MoveRequestState.TargetWaitingForPath)
                {
                    status = _pathq.GetRequestStatus(ag.TargetPathQRef);
                    if ((status & Status.Failure) != 0)
                    {
                        ag.TargetPathQRef = PathQueue.PathQInvalid;
                        if(ag.TargetRef > 0)
                            ag.TargetState = MoveRequestState.TargetRequesting;
                        else
                            ag.TargetState = MoveRequestState.TargetFailed;

                        ag.TargetReplanTime = 0.0f;
                    }
                    else if ((status & Status.Success) != 0)
                    {
                        long[] path = ag.Corridor.GetPath;
                        int npath = ag.Corridor.PathCount;

                        float[] targetPos = new float[3];
                        Helper.VCopy(ref targetPos, ag.TargetPos);

                        long[] res = _pathResult;
                        bool valid = true;
                        int nres = 0;
                        status = _pathq.GetPathResult(ag.TargetPathQRef, ref res, ref nres, _maxPathResult);
                        if ((status & Status.Failure) != 0 || nres <= 0)
                            valid = false;

                        if (valid)
                        {
                            if (npath > 1)
                            {
                                if ((npath - 1) + nres > _maxPathResult)
                                    nres = _maxPathResult - (npath - 1);

                                Array.Copy(res, 0, res, npath-1, nres);
                                Array.Copy(path, 0, res, 0, npath-1);
                                nres += npath - 1;

                                for (int j = 0; j < nres; j++)
                                {
                                    if (j - 1 >= 0 && j + 1 < nres)
                                    {
                                        if (res[j - 1] == res[j + 1])
                                        {
                                            Array.Copy(res, j+1, res, j-1, (nres-(j+1)));
                                            nres -= 2;
                                            j -= 2;
                                        }
                                    }
                                }
                            }

                            if (res[nres - 1] != ag.TargetRef)
                            {
                                float[] nearest = new float[3];

                                status = _navQuery.ClosestPointOnPoly(res[nres - 1], targetPos, ref nearest);
                                if ((status & Status.Success) != 0)
                                    Helper.VCopy(ref targetPos, nearest);
                                else
                                    valid = false;
                            }
                        }

                        if (valid)
                        {
                            ag.Corridor.SetCorridor(targetPos, res, nres);
                            ag.Boundary.Reset();
                            ag.TargetState = MoveRequestState.TargetValid;
                        }
                        else
                        {
                            ag.TargetState = MoveRequestState.TargetFailed;
                        }

                        ag.TargetReplanTime = 0.0f;
                    }
                }
            }
        }

        private void UpdateTopologyOptimization(CrowdAgent[] agents, int nagents, float dt)
        {
            if (nagents <= 0)
                return;

            float OptTimeThr = 0.5f;
            int OptMaxAgents = 1;
            CrowdAgent[] queue = new CrowdAgent[OptMaxAgents];
            queue[0] = new CrowdAgent();
            int nqueue = 0;

            for (int i = 0; i < nagents; i++)
            {
                CrowdAgent ag = agents[i];
                if (ag.State != CrowdAgentState.Walking) continue;
                if (ag.TargetState == MoveRequestState.TargetNone || ag.TargetState == MoveRequestState.TargetVelocity) continue;
                if ((ag.Param.UpdateFlags & UpdateFlags.OptimizeTopology) == 0) continue;
                ag.TopologyOptTime += dt;
                if (ag.TopologyOptTime >= OptTimeThr)
                    nqueue = AddToOptQueue(ag, ref queue, nqueue, OptMaxAgents);
            }

            for (int i = 0; i < nqueue; i++)
            {
                CrowdAgent ag = queue[i];
                ag.Corridor.OptimizePathTopology(_navQuery, _filter);
                ag.TopologyOptTime = 0;
            }
        }

        /// <summary>
        /// 检测路径正确性
        /// </summary>
        /// <param name="agents"></param>
        /// <param name="nagents"></param>
        /// <param name="dt"></param>
        private void CheckPathValitidy(CrowdAgent[] agents, int nagents, float dt)
        {
            int CheckLookAhead = 10;
            float TargetReplanDelay = 1.0f; // seconds

            for (int i = 0; i < nagents; i++)
            {
                CrowdAgent ag = agents[i];

                if (ag.State != CrowdAgentState.Walking) continue;
                if (ag.TargetState == MoveRequestState.TargetNone || ag.TargetState == MoveRequestState.TargetVelocity) continue;

                ag.TargetReplanTime += dt;
                bool replan = false;

                int idx = AgentIndex(ag);
                float[] agentPos = new float[3];
                long agentRef = ag.Corridor.FirstPoly;
                Helper.VCopy(ref agentPos, ag.npos);

                if (!_navQuery.IsValidPolyRef(agentRef, _filter))
                {
                    float[] nearest = new float[3];
                    agentRef = 0;
                    _navQuery.FindNearestPoly(ag.npos, _ext, _filter, ref agentRef, ref nearest);
                    Helper.VCopy(ref agentPos, nearest);

                    if (agentRef <= 0)
                    {
                        ag.Corridor.Reset(0, agentPos);
                        ag.Boundary.Reset();
                        ag.State = CrowdAgentState.Invalid;
                        continue;
                    }

                    ag.Corridor.FixPathStart(agentRef, agentPos);
                    ag.Boundary.Reset();
                    Helper.VCopy(ref ag.npos, agentPos);

                    replan = true;
                }

                if (ag.TargetState != MoveRequestState.TargetNone && ag.TargetState != MoveRequestState.TargetFailed)
                {
                    if (!_navQuery.IsValidPolyRef(ag.TargetRef, _filter))
                    {
                        float[] nearest = new float[3];
                        _navQuery.FindNearestPoly(ag.TargetPos, _ext, _filter, ref ag.TargetRef, ref nearest);
                        Helper.VCopy(ref ag.TargetPos, nearest);
                        replan = true;
                    }
                    if (ag.TargetRef <= 0)
                    {
                        ag.Corridor.Reset(agentRef, agentPos);
                        ag.TargetState = MoveRequestState.TargetNone;
                    }
                }

                if (!ag.Corridor.IsValid(CheckLookAhead, _navQuery, _filter))
                    replan = true;

                if (ag.TargetState == MoveRequestState.TargetValid)
                {
                    if (ag.TargetReplanTime > TargetReplanDelay && ag.Corridor.PathCount < CheckLookAhead &&
                        ag.Corridor.LastPoly != ag.TargetRef)
                        replan = true;
                }

                if (replan)
                {
                    if (ag.TargetState != MoveRequestState.TargetNone)
                    {
                        RequestMoveTargetReplan(idx, ag.TargetRef, ag.TargetPos);
                    }
                }
            }
        }

        private int AgentIndex(CrowdAgent agent)
        {
            for (int i = 0; i < _agents.Length; i++)
            {
                if (_agents[i] == agent)
                    return i;
            }
            return -1;
        }

        private bool RequestMoveTargetReplan(int idx, long refId, float[] pos)
        {
            if (idx < 0 || idx > _maxAgents)
                return false;

            CrowdAgent ag = _agents[idx];
            ag.TargetRef = refId;
            Helper.VCopy(ref ag.TargetPos, pos);
            ag.TargetPathQRef = PathQueue.PathQInvalid;
            ag.TargetReplan = true;
            if(ag.TargetRef > 0)
                ag.TargetState = MoveRequestState.TargetRequesting;
            else
            {
                ag.TargetState = MoveRequestState.TargetFailed;
            }

            return true;
        }

        private void Purge()
        {
            _agents = null;
            _maxAgents = 0;
            _activeAgents = null;
            _agentAnims = null;
            _pathResult = null;
            _grid = null;
            _obstacleQuery = null;
            _navQuery = null;
        }

        public bool Init(int maxAgents, float maxAgentRadius, NavMesh nav)
        {
            Purge();
            _maxAgents = maxAgents;
            _maxAgentRadius = maxAgentRadius;

            Helper.VSet(ref _ext, _maxAgentRadius*2.0f, _maxAgentRadius*1.5f, _maxAgentRadius*2.0f);

            _grid = new ProximityGrid();
            _grid.Init(_maxAgents*4, maxAgentRadius*3);

            _obstacleQuery = new ObstacleAvoidanceQuery();
            _obstacleQuery.Init(6, 8);

            for (int i = 0; i < _obstacleQueryParams.Length; i++)
            {
                _obstacleQueryParams[i] = new ObstacleAvoidanceParams
                {
                    velBias = 0.4f,
                    weightDesVel = 2.0f,
                    weightCurVel = 0.75f,
                    weightSide = 0.75f,
                    weightToi = 2.5f,
                    horizTime = 2.5f,
                    gridSize = 33,
                    adaptiveDivs = 7,
                    adaptiveRings = 2,
                    adaptiveDepth = 5
                };
            }

            _maxPathResult = 256;
            _pathResult = new long[_maxPathResult];

            _pathq = new PathQueue();
            _pathq.Init(_maxPathResult, MaxPathQueueNodes, nav);

            _agents = new CrowdAgent[_maxAgents];
            _activeAgents = new CrowdAgent[_maxAgents];
            _agentAnims = new CrowdAgentAnimation[_maxAgents];

            for (int i = 0; i < _maxAgents; i++)
            {
                _agents[i] = new CrowdAgent();
                _agents[i].Active = false;
                _agents[i].Corridor.Init(_maxPathResult);
            }

            for (int i = 0; i < _maxAgents; i++)
            {
                _agentAnims[i] = new CrowdAgentAnimation();
                _agentAnims[i].Active = false;
            }

            _navQuery = new NavMeshQuery();
            _navQuery.Init(nav, MaxCommonNodes);

            return true;
        }

        public void SetObstacleAvoidanceParams(int idx, ObstacleAvoidanceParams param)
        {
            if (idx >= 0 && idx < CrowdMaxObstAvoidanceParams)
                _obstacleQueryParams[idx] = param;
        }

        public ObstacleAvoidanceParams GetObstacleAvoidanceParams(int idx)
        {
            if (idx >= 0 && idx < CrowdMaxObstAvoidanceParams)
                return _obstacleQueryParams[idx];

            return null;
        }

        public CrowdAgent GetAgent(int idx)
        {
            return _agents[idx];
        }

        public int AgentCount
        {
            get { return _maxAgents; }
        }

        public int AddAgent(float[] pos, CrowdAgentParams param)
        {
            int idx = -1;
            for (int i = 0; i < _maxAgents; i++)
            {
                if (!_agents[i].Active)
                {
                    idx = i;
                    break;
                }
            }
            if (idx == -1)
                return -1;

            CrowdAgent ag = _agents[idx];

            float[] nearest = new float[3];
            long refId = 0;
            _navQuery.FindNearestPoly(pos, _ext, _filter, ref refId, ref nearest);

            ag.Corridor.Reset(refId, nearest);
            ag.Boundary.Reset();

            UpdateAgentParameters(idx, param);

            ag.TopologyOptTime = 0;
            ag.TargetReplanTime = 0;
            ag.NNeis = 0;

            Helper.VSet(ref ag.dvel, 0,0,0);
            Helper.VSet(ref ag.nvel, 0,0,0);
            Helper.VSet(ref ag.vel, 0,0,0);
            Helper.VCopy(ref ag.npos, nearest);

            ag.DesiredSpeed = 0;

            if(refId > 0)
                ag.State = CrowdAgentState.Walking;
            else
            {
                ag.State = CrowdAgentState.Invalid;
            }

            ag.TargetState = MoveRequestState.TargetNone;

            ag.Active = true;

            return idx;
        }

        public void UpdateAgentParameters(int idx, CrowdAgentParams param)
        {
            if (idx < 0 || idx > _maxAgents)
                return;

            _agents[idx].Param = param;
        }

        public void RemoveAgent(int idx)
        {
            if (idx >= 0 || idx < _maxAgents)
                _agents[idx].Active = false;
        }

        public bool RequestMoveTarget(int idx, long refId, float[] pos)
        {
            if (idx < 0 || idx > _maxAgents)
                return false;
            if (refId <= 0)
                return false;

            CrowdAgent ag = _agents[idx];
            ag.TargetRef = refId;
            Helper.VCopy(ref ag.TargetPos, pos);
            ag.TargetPathQRef = PathQueue.PathQInvalid;
            ag.TargetReplan = false;
            if(ag.TargetRef > 0)
                ag.TargetState = MoveRequestState.TargetRequesting;
            else
                ag.TargetState = MoveRequestState.TargetFailed;

            return true;
        }

        public bool RequestMoveVelocity(int idx, float[] vel)
        {
            if (idx < 0 || idx > _maxAgents)
                return false;

            CrowdAgent ag = _agents[idx];

            ag.TargetRef = 0;
            Helper.VCopy(ref ag.TargetPos, vel);
            ag.TargetPathQRef = PathQueue.PathQInvalid;
            ag.TargetReplan = false;
            ag.TargetState = MoveRequestState.TargetVelocity;

            return true;
        }

        public bool ResetMoveTarget(int idx)
        {
            if (idx < 0 || idx > _maxAgents)
                return false;

            CrowdAgent ag = _agents[idx];

            ag.TargetRef = 0;
            Helper.VSet(ref ag.TargetPos,0,0,0);
            ag.TargetPathQRef = PathQueue.PathQInvalid;
            ag.TargetReplan = false;
            ag.TargetState = MoveRequestState.TargetNone;

            return true;
        }

        public int GetActiveAgents(ref CrowdAgent[] agents, int maxAgents)
        {
            int n = 0;
            for (int i = 0; i < _maxAgents; i++)
            {
                if (!_agents[i].Active) continue;
                if (n < maxAgents)
                    agents[n++] = _agents[i];
            }
            return n;
        }

        public void Update(float dt, ref CrowdAgentDebugInfo debug)
        {
            _velocitySampleCount = 0;
            int debugIdx = debug != null ? debug.Idx : -1;

            CrowdAgent[] agents = _activeAgents;
            int nagents = GetActiveAgents(ref agents, _maxAgents);

            CheckPathValitidy(agents, nagents, dt);

            UpdateMoveRequest(dt);

            UpdateTopologyOptimization(agents, nagents, dt);

            _grid.Clear();
            for (int i = 0; i < nagents; i++)
            {
                CrowdAgent ag = agents[i];
                float[] p = ag.npos;
                float r = ag.Param.Radius;
                _grid.AddItem(i, p[0]-r, p[2]-r, p[0]+r, p[2]+r);
            }

            for (int i = 0; i < nagents; i++)
            {
                CrowdAgent ag = agents[i];
                if (ag.State != CrowdAgentState.Walking) continue;

                float updateThr = ag.Param.CollisionQueryRange*0.25f;
                if (Helper.VDist2DSqr(ag.npos[0], ag.npos[1], ag.npos[2], ag.Boundary.Center[0], ag.Boundary.Center[1], ag.Boundary.Center[2]) > updateThr*updateThr ||
                    !ag.Boundary.IsValid(_navQuery, _filter))
                {
                    ag.Boundary.Update(ag.Corridor.FirstPoly, ag.npos, ag.Param.CollisionQueryRange, _navQuery, _filter);
                }

                ag.NNeis = GetNeighbors(ag.npos, ag.Param.Height, ag.Param.CollisionQueryRange, ag, ref ag.Neis,
                                        CrowdAgent.CrowdAgentMaxNeighbors, agents, nagents, _grid);

                for (int j = 0; j < ag.NNeis; j++)
                {
                    ag.Neis[j].Idx = AgentIndex(agents[ag.Neis[j].Idx]);
                }
            }

            // 查找用于转向的下一个Corner
            for (int i = 0; i < nagents; i++)
            {
                CrowdAgent ag = agents[i];

                if (ag.State != CrowdAgentState.Walking)
                    continue;

                if (ag.TargetState == MoveRequestState.TargetNone || ag.TargetState == MoveRequestState.TargetVelocity)
                    continue;

                ag.NCorners = ag.Corridor.FindCorners(ref ag.CornerVerts, ref ag.CornerFlags, ref ag.CornerPolys, CrowdAgent.CrowdAgentMaxCorners, _navQuery, _filter);

                if ((ag.Param.UpdateFlags & UpdateFlags.OptimizeVisibility) != 0 && ag.NCorners > 0)
                {
                    float[] target = new float[3];
                    Array.Copy(ag.CornerVerts, Math.Min(1, ag.NCorners-1)*3, target, 0, 3);
                    ag.Corridor.OptimizePathVisibility(target, ag.Param.PathOptimizationRange, _navQuery, _filter);
                    if (debugIdx == i)
                    {
                        Helper.VCopy(ref debug.OptStart, ag.Corridor.Pos);
                        Helper.VCopy(ref debug.OptEnd, target);
                    }
                }
                else
                {
                    if (debugIdx == i)
                    {
                        Helper.VSet(ref debug.OptStart, 0,0,0);
                        Helper.VSet(ref debug.OptEnd, 0,0,0);
                    }
                }
            }

            // 触发离地连接
            for (int i = 0; i < nagents; i++)
            {
                CrowdAgent ag = agents[i];

                if (ag.State != CrowdAgentState.Walking) continue;
                if (ag.TargetState == MoveRequestState.TargetNone || ag.TargetState == MoveRequestState.TargetVelocity)
                    continue;

                float triggerRadius = ag.Param.Radius*2.25f;
                if (ag.OverOffMeshConnection(triggerRadius))
                {
                    int idx = AgentIndex(ag);
                    CrowdAgentAnimation anim = _agentAnims[idx];

                    long[] refs = new long[2];
                    if (ag.Corridor.MoveOverOffmeshConnection(ag.CornerPolys[ag.NCorners - 1], ref refs, ref anim.StartPos, ref anim.EndPos, _navQuery))
                    {
                        Helper.VCopy(ref anim.InitPos, ag.npos);
                        anim.PolyRef = refs[1];
                        anim.Active = true;
                        anim.T = 0.0f;
                        anim.TMax = (Helper.VDist2D(anim.StartPos, anim.EndPos)/ag.Param.MaxSpeed)*0.5f;

                        ag.State = CrowdAgentState.OffMesh;
                        ag.NCorners = 0;
                        ag.NNeis = 0;
                        continue;
                    }
                }
            }

            // 计算转向
            for (int i = 0; i < nagents; i++)
            {
                CrowdAgent ag = agents[i];

                if (ag.State != CrowdAgentState.Walking) continue;
                if (ag.TargetState == MoveRequestState.TargetNone) continue;

                float[] dvel = {0f,0f,0f};

                if (ag.TargetState == MoveRequestState.TargetVelocity)
                {
                    Helper.VCopy(ref dvel, ag.TargetPos);
                    ag.DesiredSpeed = Helper.VLen(ag.TargetPos);
                }
                else
                {
                    if ((ag.Param.UpdateFlags & UpdateFlags.AnticipateTurns) != 0)
                    {
                        ag.CalcSmoothSteerDirection(ref dvel);
                    }
                    else
                    {
                        ag.CalcStraightSteerDirection(ref dvel);
                    }

                    float slowDownRadius = ag.Param.Radius*2;
                    float speedScale = ag.GetDistanceToGoal(slowDownRadius)/slowDownRadius;

                    ag.DesiredSpeed = ag.Param.MaxSpeed;
                    dvel = Helper.VScale(dvel[0], dvel[1], dvel[2], ag.DesiredSpeed*speedScale);
                }

                //separation
                if ((ag.Param.UpdateFlags & UpdateFlags.Separation) != 0)
                {
                    float separationDist = ag.Param.CollisionQueryRange;
                    float invSeparationDist = 1.0f/separationDist;
                    float separationWeight = ag.Param.SeparationWeight;

                    float w = 0;
                    float[] disp = {0, 0, 0};

                    for (int j = 0; j < ag.NNeis; j++)
                    {
                        CrowdAgent nei = _agents[ag.Neis[j].Idx];

                        float[] diff = Helper.VSub(ag.npos[0], ag.npos[1], ag.npos[2], nei.npos[0], nei.npos[1], nei.npos[2]);
                        diff[1] = 0;

                        float distSqr = Helper.VLenSqr(diff);
                        if (distSqr < 0.00001f) continue;
                        if (distSqr > separationDist*separationDist) continue;
                        float dist = (float)Math.Sqrt(distSqr);
                        float weight = separationWeight*(1.0f - (dist*invSeparationDist*dist*invSeparationDist));

                        Helper.VMad(ref disp, disp, diff, weight/dist);
                        w += 1.0f;
                    }

                    if (w > 0.0001f)
                    {
                        Helper.VMad(ref dvel, dvel, disp, 1.0f/w);
                        float speedSqr = Helper.VLenSqr(dvel);
                        float desiredSpeed = (ag.DesiredSpeed*ag.DesiredSpeed);
                        if (speedSqr > desiredSpeed)
                            dvel = Helper.VScale(dvel[0], dvel[1], dvel[2], desiredSpeed/speedSqr);
                    }
                }

                Helper.VCopy(ref ag.dvel, dvel);
            }

            // 速度计划
            for (int i = 0; i < nagents; i++)
            {
                CrowdAgent ag = agents[i];
                if (ag.State != CrowdAgentState.Walking) continue;

                if ((ag.Param.UpdateFlags & UpdateFlags.ObstacleAvoidance) != 0)
                {
                    _obstacleQuery.Reset();

                    for (int j = 0; j < ag.NNeis; j++)
                    {
                        CrowdAgent nei = _agents[ag.Neis[j].Idx];
                        _obstacleQuery.AddCircle(nei.npos, nei.Param.Radius, nei.vel, nei.dvel);
                    }

                    for (int j = 0; j < ag.Boundary.SegmentCount; j++)
                    {
                        float[] s0 = new float[3];
                        float[] s1 = new float[3];
                        Array.Copy(ag.Boundary.GetSegment(j), 0, s0, 0, 3);
                        Array.Copy(ag.Boundary.GetSegment(j), 3, s1, 0, 3);
                        if (Helper.TriArea2D(ag.npos, s0, s1) < 0.0f) continue;
                        _obstacleQuery.AddSegment(s0, s1);
                    }

                    ObstacleAvoidanceDebugData vod = null;
                    if (debugIdx == i)
                        vod = debug.Vod;

                    bool adaptive = true;
                    int ns = 0;

                    ObstacleAvoidanceParams param = _obstacleQueryParams[ag.Param.ObstacleAvoidanceType];

                    if (adaptive)
                    {
                        ns = _obstacleQuery.SampleVelocityAdaptive(ag.npos, ag.Param.Radius, ag.DesiredSpeed, ag.vel,
                                                                   ag.dvel, ref ag.nvel, param, vod);
                    }
                    else
                    {
                        ns = _obstacleQuery.SampleVelocityGrid(ag.npos, ag.Param.Radius, ag.DesiredSpeed, ag.vel,
                                                               ag.dvel, ref ag.nvel, param, vod);
                    }
                    _velocitySampleCount += ns;
                }
                else
                {
                    Helper.VCopy(ref ag.nvel, ag.dvel);
                }
            }

            // 整合
            for (int i = 0; i < nagents; i++)
            {
                CrowdAgent ag = agents[i];
                if (ag.State != CrowdAgentState.Walking) continue;

                ag.Integrate(dt);
            }

            // 操纵碰撞
            float CollisionResolveFactor = 0.7f;

            for (int iter = 0; iter < 4; iter++)
            {
                for (int i = 0; i < nagents; i++)
                {
                    CrowdAgent ag = agents[i];
                    int idx0 = AgentIndex(ag);

                    if (ag.State != CrowdAgentState.Walking) continue;

                    Helper.VSet(ref ag.disp, 0,0,0);

                    float w = 0;

                    for (int j = 0; j < ag.NNeis; j++)
                    {
                        CrowdAgent nei = _agents[ag.Neis[j].Idx];
                        int idx1 = AgentIndex(nei);

                        float[] diff = Helper.VSub(ag.npos[0], ag.npos[1], ag.npos[2], nei.npos[0], nei.npos[1],
                                                   nei.npos[2]);
                        diff[1] = 0;

                        float dist = Helper.VLenSqr(diff);
                        if (dist > (ag.Param.Radius + nei.Param.Radius)*(ag.Param.Radius + nei.Param.Radius)) continue;
                        dist = (float) Math.Sqrt(dist);
                        float pen = (ag.Param.Radius + nei.Param.Radius) - dist;
                        if (dist < 0.0001f)
                        {
                            if(idx0 > idx1)
                                Helper.VSet(ref diff, -ag.dvel[2], 0, ag.dvel[0]);
                            else
                                Helper.VSet(ref diff, ag.dvel[2], 0, -ag.vel[0]);
                            pen = 0.01f;
                        }
                        else
                        {
                            pen = (1.0f/dist)*(pen*0.5f)*CollisionResolveFactor;
                        }

                        Helper.VMad(ref ag.disp, ag.disp, diff, pen);

                        w += 1.0f;
                    }

                    if (w > 0.0001f)
                    {
                        float iw = 1.0f/w;
                        ag.disp = Helper.VScale(ag.disp[0], ag.disp[1], ag.disp[2], iw);
                    }
                }

                for (int i = 0; i < nagents; i++)
                {
                    CrowdAgent ag = agents[i];
                    if (ag.State != CrowdAgentState.Walking) continue;

                    ag.npos = Helper.VAdd(ag.npos[0], ag.npos[1], ag.npos[2], ag.disp[0], ag.disp[1], ag.disp[2]);
                }
            }

            for (int i = 0; i < nagents; i++)
            {
                CrowdAgent ag = agents[i];
                if (ag.State != CrowdAgentState.Walking) continue;

                ag.Corridor.MovePosition(ag.npos, _navQuery, _filter);
                Helper.VCopy(ref ag.npos, ag.Corridor.Pos);

                if (ag.TargetState == MoveRequestState.TargetNone || ag.TargetState == MoveRequestState.TargetVelocity)
                {
                    ag.Corridor.Reset(ag.Corridor.FirstPoly, ag.npos);
                }
            }

            // Update agents using off-mesh connection
            for (int i = 0; i < _maxAgents; i++)
            {
                CrowdAgentAnimation anim = _agentAnims[i];
                if (!anim.Active) continue;
                CrowdAgent ag = agents[i];

                anim.T += dt;
                if (anim.T > anim.TMax)
                {
                    anim.Active = false;
                    ag.State = CrowdAgentState.Walking;
                    continue;
                }

                float ta = anim.TMax*0.15f;
                float tb = anim.TMax;
                if (anim.T < ta)
                {
                    float u = Tween(anim.T, 0.0f, ta);
                    Helper.VLerp(ref ag.npos, anim.InitPos[0], anim.InitPos[1], anim.InitPos[2], anim.StartPos[0], anim.StartPos[1], anim.StartPos[2], u);
                }
                else
                {
                    float u = Tween(anim.T, ta, tb);
                    Helper.VLerp(ref ag.npos, anim.StartPos[0], anim.StartPos[1], anim.StartPos[2], anim.EndPos[0], anim.EndPos[1], anim.EndPos[2], u);
                }

                Helper.VSet(ref ag.vel, 0,0,0);
                Helper.VSet(ref ag.dvel, 0,0,0);
            }
        }

        public QueryFilter Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        public float[] QueryExtents
        {
            get { return _ext; }
        }

        public int VelocitySample
        {
            get { return _velocitySampleCount; }
        }

        public ProximityGrid Grid
        {
            get { return _grid; }
        }

        public NavMeshQuery NavMeshQuery
        {
            get { return _navQuery; }
        }

        public static int AddNeighbor(int idx, float dist, ref CrowdNeighbor[] neis, int nneis, int maxNeis)
        {
            CrowdNeighbor nei = null;
            if (neis != null || neis.Length > 0)
            {
                if (nneis >= 6)
                {
                    //Console.WriteLine("--");
                }

                nei = neis[nneis];
            }
            else if (dist >= neis[nneis - 1].Dist)
            {
                if (nneis >= maxNeis)
                    return nneis;
                nei = neis[nneis];
            }
            else
            {
                int i;
                for (i = 0; i < nneis; i++)
                {
                    if (dist <= neis[i].Dist)
                        break;
                }

                int tgt = i + 1;
                int n = Math.Min(nneis - i, maxNeis - tgt);

                if(n > 0)
                    Array.Copy(neis, i, neis, tgt, n);
                nei = neis[i];
            }

            nei = new CrowdNeighbor();
            nei.Idx = idx;
            nei.Dist = dist;

            return Math.Min(nneis + 1, maxNeis);
        }

        public static int GetNeighbors(float[] pos, float height, float range, CrowdAgent skip, 
            ref CrowdNeighbor[] result, int maxResult, CrowdAgent[] agents, int nagents, ProximityGrid grid)
        {
            int n = 0;

            int MaxNeis = 32;
            int[] ids = new int[MaxNeis];
            int nids = grid.QueryItems(pos[0] - range, pos[2] - range, pos[0] + range, pos[2] + range, ref ids, MaxNeis);

            for (int i = 0; i < nids; i++)
            {
                CrowdAgent ag = agents[ids[i]];

                if (ag == skip) continue;

                float[] diff = Helper.VSub(pos[0], pos[1], pos[2], ag.npos[0], ag.npos[1], ag.npos[2]);
                if (Math.Abs(diff[1]) >= (height + ag.Param.Height)/2.0f)
                    continue;

                diff[1] = 0;
                float distSqr = Helper.VLenSqr(diff);

                if (distSqr > range*range)
                    continue;

                if (nids >= CrowdAgent.CrowdAgentMaxNeighbors) continue;

                n = AddNeighbor(ids[i], distSqr, ref result, n, maxResult);
            }

            return n;
        }

        public static int AddToOptQueue(CrowdAgent newag, ref CrowdAgent[] agents, int nagents, int maxAgents)
        {
            int slot = 0;
            if (nagents <= 0)
                slot = 0;
            else if (newag.TopologyOptTime <= agents[nagents - 1].TopologyOptTime)
            {
                if (nagents >= maxAgents)
                    return nagents;
                slot = nagents;
            }
            else
            {
                int i;
                for (i = 0; i < nagents; i++)
                {
                    if (newag.TopologyOptTime >= agents[i].TopologyOptTime)
                        break;
                }

                int tgt = i + 1;
                int n = Math.Min(nagents - i, maxAgents - tgt);

                if(n > 0)
                    Array.Copy(agents, i, agents, tgt, n);
                slot = i;
            }

            agents[slot] = newag;

            return Math.Min(nagents + 1, maxAgents);
        }

        public static int AddToPlanQueue(CrowdAgent newag, ref CrowdAgent[] agents, int nagents, int maxAgents)
        {
            int slot = 0;
            if (nagents <= 0)
                slot = 0;
            else if (newag.TargetReplanTime <= agents[nagents - 1].TargetReplanTime)
            {
                if (nagents >= maxAgents)
                    return nagents;
                slot = nagents;
            }
            else
            {
                int i;
                for (i = 0; i < nagents; i++)
                {
                    if (newag.TargetReplanTime >= agents[i].TargetReplanTime)
                        break;
                }

                int tgt = i + 1;
                int n = Math.Min(nagents - i, maxAgents - tgt);

                if (n > 0)
                    Array.Copy(agents, i, agents, tgt, n);
                slot = i;
            }

            agents[slot] = newag;

            return Math.Min(nagents + 1, maxAgents);
        }

        public float Tween(float t, float t0, float t1)
        {
            return Math.Max(0.0f, Math.Min(1.0f, (t - t0)/(t1 - t0)));
        }
    }
}