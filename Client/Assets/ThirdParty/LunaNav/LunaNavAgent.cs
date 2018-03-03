
using LunaNav;
using UnityEngine;

public class LunaNavAgent
{
    private Crowd crowd;

    public float Radius = 0.6f;
    public float Height = 2.0f;
    public float MaxAcceleration = 100f;
    public float MaxSpeed = 3.5f;

    public float CollisionQueryRange = 0f;
    public float PathOptimizationRange = 0f;

    public UpdateFlags UpdateFlags = 0;
    public short ObstacleAvoidanceType = 3;
    public float SeparationWeight = 2.0f;
    public float WaitForUpdate = 0.0f;
    private float _timeSinceUpdate = 0.0f;
    private NavMeshQuery _navMeshQuery;
    private QueryFilter filter;
    public CrowdAgentParams param;

    public int AgentId { get; set; }

    private LunaNavmeshQuery _navQuery;

    private Transform _transform;

    private bool _enabled = false;

    private CrowdAgent _crowdAgent;

    private float _smoothSpeed = 3f;

    private System.Action _reached;

    /// <summary>
    /// 移动事件
    /// </summary>
    private System.Action _move;

    //private Role _host;

    private bool _startMove = false;

    public bool enabled
    {
        get
        {
            return _enabled;
        }
        set
        {
            _enabled = value;
            _crowdAgent.State = _enabled == true ? CrowdAgentState.Walking : CrowdAgentState.Invalid;
        }
    }
    public CrowdAgentParams AgentParams
    {
        get
        {
            if (_navQuery != null)
            {
                return _navQuery.Crowd.GetAgent(AgentId).Param;
            }
            return null;
        }
    }


    public void Init(LunaNavmeshQuery navmesh, Transform transform, System.Action idle, System.Action move)
    {
        _navQuery = navmesh;

        _transform = transform;
        _reached = idle;
        _move = move;

        if (_navQuery != null)
        {
            _navMeshQuery = _navQuery._navMeshQuery;
            filter = _navQuery.filter;
            crowd = _navQuery.Crowd;

            param = new CrowdAgentParams
            {
                Radius = Radius,
                Height = Height,
                MaxAcceleration = MaxAcceleration,
                MaxSpeed = MaxSpeed,
                CollisionQueryRange = CollisionQueryRange,
                PathOptimizationRange = PathOptimizationRange,
                UpdateFlags = UpdateFlags,
                ObstacleAvoidanceType = ObstacleAvoidanceType,
                SeparationWeight = SeparationWeight
            };

            Vector3 pos = _transform.position;
            AgentId = _navQuery.Crowd.AddAgent(new[] { pos.x, pos.y, pos.z }, param);
            //ResetTarget();

            _crowdAgent = crowd.GetAgent(AgentId);
            float[] resetpos = _crowdAgent.npos;
            _transform.position = new Vector3(resetpos[0], resetpos[1], resetpos[2]);            
        }
        else
        {
            UnityEngine.Debug.LogError("Scene does not have a Nav Mesh Query, one must be added.");
        }
    }


    public void Move(Vector3 dest)
    {
        if (_enabled)
        {
            float[] endPos = new float[3]{ dest.x, dest.y, dest.z };

            long endRef = 0;
            float[] nearestPt = new float[3];

            Status status = _navMeshQuery.FindNearestPoly(endPos, new[] { 2f, 4f, 2f }, filter, ref endRef, ref nearestPt);
            if(status == Status.Success)
            {
                crowd.RequestMoveTarget(AgentId, endRef, nearestPt);
            }
            _startMove = true;
        }        
    }


    public void End()
    {
        crowd.RemoveAgent(AgentId);
    }

    public void Update(float dt)
    {
        float[] pos = crowd.GetAgent(AgentId).npos;
        Vector3 oldpos = _transform.position;
        Vector3 newpos = new Vector3(pos[0], pos[1], pos[2]);
        float distance = Vector3.Distance(newpos, oldpos);

        if (_startMove)
        {
            _move();
            if (distance < 0.01f)
            {
                _reached();
                _startMove = false;
            }
            else
            {
                _transform.position = newpos;

                Vector3 dvec = new Vector3(_crowdAgent.dvel[0], _crowdAgent.dvel[1], _crowdAgent.dvel[2]);
                if (dvec != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(dvec);
                    _transform.rotation = Quaternion.Slerp(_transform.rotation, rotation, _smoothSpeed * dt);
                }
            }
        }
    }

    public void ResetValues()
    {
        Radius = 0.6f;
        Height = 2.0f;
        MaxAcceleration = 8.0f;
        MaxSpeed = 3.5f;
        CollisionQueryRange = 0.6f * 12.0f;
        PathOptimizationRange = 0.6f * 30.0f;
        UpdateFlags = 0;
        ObstacleAvoidanceType = 3;
        SeparationWeight = 2.0f;
        WaitForUpdate = 0.0f;
    }
}
