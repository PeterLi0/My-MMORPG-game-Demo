//using System;
//using LunaNav;
//using UnityEngine;
//using System.Collections;

//[AddComponentMenu("Recast/Detour Actor")]
//[Serializable]
//public class DetourActor : MonoBehaviour
//{
//    private Crowd crowd;

//    public GameObject Target;
//    public Vector3 targetPos;
//    public float Radius = 0.6f;
//    public float Height = 2.0f;
//    public float MaxAcceleration = 8.0f;
//    public float MaxSpeed = 3.5f;
//    public float CollisionQueryRange = 0.6f*12.0f;
//    public float PathOptimizationRange = 0.6f*30.0f;
//    public UpdateFlags UpdateFlags = 0;
//    public short ObstacleAvoidanceType = 3;
//    public float SeparationWeight = 2.0f;
//    public float WaitForUpdate = 0.0f;
//    private float _timeSinceUpdate = 0.0f;
//    private NavMeshQuery _navMeshQuery;
//    private QueryFilter filter;
//    public CrowdAgentParams param;

//    public int AgentId { get; set; }

//    // Use this for initialization
//    void Start ()
//    {

//        var navQuery = FindObjectOfType(typeof(RecastNavMeshQuery)) as RecastNavMeshQuery;
//        if (navQuery != null)
//        {
//            _navMeshQuery = navQuery._navMeshQuery;
//            filter = navQuery.filter;
//            crowd = navQuery.Crowd;

//            param = new CrowdAgentParams
//            {
//                Radius = Radius,
//                Height = Height,
//                MaxAcceleration = MaxAcceleration,
//                MaxSpeed = MaxSpeed,
//                CollisionQueryRange = CollisionQueryRange,
//                PathOptimizationRange = PathOptimizationRange,
//                UpdateFlags = UpdateFlags,
//                ObstacleAvoidanceType = ObstacleAvoidanceType,
//                SeparationWeight = SeparationWeight
//            };
//            AgentId = navQuery.Crowd.AddAgent(new[] { transform.position.x, transform.position.y, transform.position.z }, param);
//            ResetTarget();
//        }
//        else
//        {
//            Debug.LogError("Scene does not have a Nav Mesh Query, one must be added.");
//        }
//    }

//    public CrowdAgentParams AgentParams
//    {
//        get
//        {
//            var navQuery = FindObjectOfType(typeof(RecastNavMeshQuery)) as RecastNavMeshQuery;
//            if (navQuery != null)
//            {
//                return navQuery.Crowd.GetAgent(AgentId).Param;
//            }
//            return null;
//        }
//    }

//    // Update is called once per frame
//    void Update () {
//        float[] pos = crowd.GetAgent(AgentId).npos;
//        transform.position = new Vector3(pos[0], pos[1], pos[2]);
//        _timeSinceUpdate += Time.deltaTime;
//        if (_timeSinceUpdate >= WaitForUpdate && targetPos != Target.transform.position)
//        {
//            ResetTarget();
//            _timeSinceUpdate = 0.0f;
//        }
//    }

//    public void ResetTarget()
//    {
//        if (Target != null)
//        {
//            float[] endPos = { Target.transform.position.x, Target.transform.position.y, Target.transform.position.z };
//            long endRef = 0;
//            float[] nearestPt = new float[3];
//            _navMeshQuery.FindNearestPoly(endPos, new[] { 2f, 4f, 2f }, filter, ref endRef, ref nearestPt);
//            crowd.RequestMoveTarget(AgentId, endRef, nearestPt);
//            targetPos = Target.transform.position;
//        }

//    }

//    public void ResetValues()
//    {
//        Radius = 0.6f;
//        Height = 2.0f;
//        MaxAcceleration = 8.0f;
//        MaxSpeed = 3.5f;
//        CollisionQueryRange = 0.6f*12.0f;
//        PathOptimizationRange = 0.6f*30.0f;
//        UpdateFlags = 0;
//        ObstacleAvoidanceType = 3;
//        SeparationWeight = 2.0f;
//        WaitForUpdate = 0.0f;
//    }
//}
