
using LunaNav;
using Luna3D;

public class LunaNavAgent
{
    private Crowd crowd;

    public Vector3 targetPos;

    public float Radius = 0.6f;
    public float Height = 2.0f;
    public float MaxAcceleration = 8.0f;
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

    private RoleTransform _transform;

    private bool _enabled = false;

    private CrowdAgent _crowdAgent;

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

    public void Init( RoleTransform transform, LunaNavmeshQuery navmeshQuery)
    {
        _navQuery = navmeshQuery;
        _transform = transform;


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

            Luna3D.Vector3 pos = _transform.GetPosition();
            AgentId = _navQuery.Crowd.AddAgent(new[] { pos.x, pos.y, pos.z }, param);
            //ResetTarget();

            _crowdAgent = crowd.GetAgent(AgentId);
            float[] resetpos = _crowdAgent.npos;
            _transform.SetPosition(new Vector3(resetpos[0], resetpos[1], resetpos[2]));     
        }
        else
        {
            System.Console.WriteLine("Scene does not have a Nav Mesh Query, one must be added.");
        }
    }


    public void Move(Vector3 dest)
    {
        if (_enabled)
        {
            float[] endPos = new float[3]{ dest.x, dest.y, dest.z };

            long endRef = 0;
            float[] nearestPt = new float[3];

            _navMeshQuery.FindNearestPoly(endPos, new[] { 2f, 4f, 2f }, filter, ref endRef, ref nearestPt);
            crowd.RequestMoveTarget(AgentId, endRef, nearestPt);
            targetPos = dest;
        }        
    }


    public void End()
    {
        crowd.RemoveAgent(AgentId);
    }

    public void Update(float dt)
    {
       float[] pos = _crowdAgent.npos;
        //Vector3 oldpos = _transform.GetPosition();
        Vector3 newpos = new Vector3(pos[0], pos[1], pos[2]);
        //float distance = Vector3.Distance(newpos, oldpos);

        //if(distance > 0.001f)

        _transform.SetPosition(newpos);
        

        //_timeSinceUpdate += dt;

        //if (_timeSinceUpdate >= WaitForUpdate && targetPos != Target.transform.position)
        //{
        //    ResetTarget();
        //    _timeSinceUpdate = 0.0f;
        //}
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
