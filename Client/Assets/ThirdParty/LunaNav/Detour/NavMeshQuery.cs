using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LunaNav
{
    [System.Serializable]
	public class NavMeshQuery
	{
        public NodePool NodePool { get; set; }
        public NavMesh NavMesh { get; set; }
	    public NodePool _tinyNodePool;
        public NodeQueue _openList;
        public QueryData _query;

	    public static float HScale = 0.999f;
        public delegate float RandFunc();

	    public static short StraightPathStart = 0x01;
	    public static short StraightPathEnd = 0x02;
	    public static short StraightPathOffMeshConnection = 0x04;

	    public static short StraightPathAreaCrossings = 0x01;
	    public static short StraightPathAllCrossings = 0x02;
        public NavMeshQuery()
        {
            
        }

        public Status Init(NavMesh navMesh, int maxNodes)
        {
            NavMesh = navMesh;
            if (NodePool == null || NodePool.MaxNodes < maxNodes)
            {
                NodePool = new NodePool(maxNodes, (int)Helper.NextPow2(maxNodes/4));
            }
            else
            {
                NodePool.Clear();
            }

            if (_tinyNodePool == null)
            {
                _tinyNodePool = new NodePool(64, 32);
            }
            else
            {
                _tinyNodePool.Clear();
            }

            if (_openList == null || _openList.Capacity < maxNodes)
            {
                _openList = new NodeQueue(maxNodes);
            }
            else
            {
                _openList.Clear();
            }

            return Status.Success;
        }

        public Status FindPath(long startRef, long endRef, float[] startPos, float[] endPos, QueryFilter filter,
                               ref long[] path, ref int pathCount, int maxPath)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");
            if(NodePool == null)
                throw new ApplicationException("NodePool is not initialized");
            if(_openList == null)
                throw new ApplicationException("OpenList is not initialized");

            pathCount = 0;

            if (startRef <= 0 || endRef <= 0 || maxPath <= 0)
            {
                return Status.Failure | Status.InvalidParam;
            }

            if(!NavMesh.IsValidPolyRef(startRef) || !NavMesh.IsValidPolyRef(endRef))
                return Status.Failure | Status.InvalidParam;

            if (startRef == endRef)
            {
                path[0] = startRef;
                pathCount = 1;
                return Status.Success;
            }

            NodePool.Clear();
            _openList.Clear();

            Node startNode = NodePool.GetNode(startRef);
            Array.Copy(startPos, startNode.Pos, 3);
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = Helper.VDist(startPos[0], startPos[1], startPos[2], endPos[0], endPos[1], endPos[2])*HScale;
            startNode.Id = startRef;
            startNode.Flags = Node.NodeOpen;
            _openList.Push(startNode);

            Node lastBestNode = startNode;
            float lastBestNodeCost = startNode.Total;

            Status status = Status.Success;

            while (!_openList.Empty())
            {
                Node bestNode = _openList.Pop();
                bestNode.Flags &= ~Node.NodeOpen;
                bestNode.Flags |= Node.NodeClosed;

                if (bestNode.Id == endRef)
                {
                    lastBestNode = bestNode;
                    break;
                }

                long bestRef = bestNode.Id;
                MeshTile bestTile = null;
                Poly bestPoly = null;
                NavMesh.GetTileAndPolyByRefUnsafe(bestRef, out bestTile, out bestPoly);

                long parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;

                if (bestNode.PIdx > 0)
                    parentRef = NodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                if(parentRef > 0)
                    NavMesh.GetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);

                for (long i = bestPoly.FirstLink; i != NavMesh.NullLink; i = bestTile.Links[i].Next)
                {
                    long neighborRef = bestTile.Links[i].Ref;

                    if (neighborRef <= 0 || neighborRef == parentRef)
                        continue;

                    MeshTile neighbourTile = null;
                    Poly neighbourPoly = null;
                    NavMesh.GetTileAndPolyByRefUnsafe(neighborRef, out neighbourTile, out neighbourPoly);

                    if (!filter.PassFilter(neighborRef, neighbourTile, neighbourPoly))
                        continue;

                    Node neighbourNode = NodePool.GetNode(neighborRef);
                    if (neighbourNode == null)
                    {
                        status |= Status.OutOfNodes;
                        continue;
                    }

                    if (neighbourNode.Flags == 0)
                    {
                        float[] pos = new float[3];
                        GetEdgeMidPoint(bestRef, bestPoly, bestTile, neighborRef, neighbourPoly, neighbourTile, ref pos);
                        Array.Copy(pos, neighbourNode.Pos, 3);
                    }

                    float cost = 0;
                    float heuristic = 0;

                    if (neighborRef == endRef)
                    {
                        float curCost = filter.GetCost(bestNode.Pos[0], bestNode.Pos[1], bestNode.Pos[2],
                                                       neighbourNode.Pos[0], neighbourNode.Pos[1], neighbourNode.Pos[2],
                                                       parentRef, parentTile, parentPoly, bestRef, bestTile, bestPoly,
                                                       neighborRef, neighbourTile, neighbourPoly);
                        float endCost = filter.GetCost(neighbourNode.Pos[0], neighbourNode.Pos[1], neighbourNode.Pos[2],
                                                       endPos[0], endPos[1], endPos[2], bestRef, bestTile, bestPoly,
                                                       neighborRef, neighbourTile, neighbourPoly, 0, null, null);
                        cost = bestNode.Cost + curCost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        float curCost = filter.GetCost(bestNode.Pos[0], bestNode.Pos[1], bestNode.Pos[2],
                                                       neighbourNode.Pos[0], neighbourNode.Pos[1], neighbourNode.Pos[2],
                                                       parentRef, parentTile, parentPoly, bestRef, bestTile, bestPoly,
                                                       neighborRef, neighbourTile, neighbourPoly);
                        cost = bestNode.Cost + curCost;
                        heuristic =
                            Helper.VDist(neighbourNode.Pos[0], neighbourNode.Pos[1], neighbourNode.Pos[2], endPos[0],
                                         endPos[1], endPos[2])*HScale;
                    }

                    float total = cost + heuristic;

                    if((neighbourNode.Flags & Node.NodeOpen) != 0 && total >= neighbourNode.Total)
                        continue;
                    if ((neighbourNode.Flags & Node.NodeClosed) != 0 && total >= neighbourNode.Total)
                        continue;

                    neighbourNode.PIdx = NodePool.GetNodeIdx(bestNode);
                    neighbourNode.Id = neighborRef;
                    neighbourNode.Flags = (neighbourNode.Flags & ~Node.NodeClosed);
                    neighbourNode.Cost = cost;
                    neighbourNode.Total = total;

                    if ((neighbourNode.Flags & Node.NodeOpen) != 0)
                    {
                        _openList.Modify(neighbourNode);
                    }
                    else
                    {
                        neighbourNode.Flags |= Node.NodeOpen;
                        _openList.Push(neighbourNode);
                    }

                    if (heuristic < lastBestNodeCost)
                    {
                        lastBestNodeCost = heuristic;
                        lastBestNode = neighbourNode;
                    }
                }
            }

            if (lastBestNode.Id != endRef)
            {
                status |= Status.PartialResult;
            }

            Node prev = null;
            Node node = lastBestNode;
            do
            {
                Node next = NodePool.GetNodeAtIdx(node.PIdx);
                node.PIdx = NodePool.GetNodeIdx(prev);
                prev = node;
                node = next;
            } while (node != null);

            node = prev;
            int n = 0;
            do
            {
                path[n++] = node.Id;
                if (n >= maxPath)
                {
                    status |= Status.BufferTooSmall;
                    break;
                }
                node = NodePool.GetNodeAtIdx(node.PIdx);
            } while (node != null);

            pathCount = n;

            return status;
        }

	    public Status FindStraightPath(float[] startPos, float[] endPos, long[] path, int pathSize,
	                                   ref float[] straightPath, ref short[] straightPathFlags,
	                                   ref long[] straightPathRefs,
	                                   ref int straightPathCount, int maxStraightPath, int options = 0)
	    {
	        if (NavMesh == null)
	            throw new ApplicationException("NavMesh is not initialized");
	        if (maxStraightPath <= 0)
	            return Status.Failure | Status.InvalidParam;
            if(path[0] <= 0)
                return Status.Failure | Status.InvalidParam;

	        Status stat = 0;
            float[] closestStartPos = new float[3];
            if((ClosestPointOnPolyBoundary(path[0], startPos, ref closestStartPos) & Status.Failure) != 0)
                return Status.Failure | Status.InvalidParam;

            float[] closestEndPos = new float[3];
            if((ClosestPointOnPolyBoundary(path[pathSize-1], endPos, ref closestEndPos) & Status.Failure) != 0)
                return Status.Failure | Status.InvalidParam;

	        stat = AppendVertex(closestStartPos, StraightPathStart, path[0], ref straightPath, ref straightPathFlags,
	                            ref straightPathRefs, ref straightPathCount, maxStraightPath);
	        if (stat != Status.InProgress)
	            return stat;

            if (pathSize > 1)
            {
                float[] portalApex = new float[3], portalLeft = new float[3], portalRight = new float[3];
                Array.Copy(closestStartPos, portalApex, 3);
                Array.Copy(portalApex, portalLeft, 3);
                Array.Copy(portalApex, portalRight, 3);

                int apexIndex = 0;
                int leftIndex = 0;
                int rightIndex = 0;

                short leftPolyType = 0;
                short rightPolyType = 0;

                long leftPolyRef = path[0];
                long rightPolyRef = path[0];

                for (int i = 0; i < pathSize; i++)
                {
                    float[] left = new float[3], right = new float[3];
                    short fromType = 0, toType = 0;

                    if (i + 1 < pathSize)
                    {
                        if ((GetPortalPoints(path[i], path[i + 1], ref left, ref right, ref fromType, ref toType) &
                             Status.Failure) != 0)
                        {
                            if ((ClosestPointOnPolyBoundary(path[i], endPos, ref closestEndPos) & Status.Failure) != 0)
                            {
                                return Status.Failure | Status.InvalidParam;
                            }

                            if ((options & (StraightPathAreaCrossings | StraightPathAllCrossings)) != 0)
                            {
                                stat = AppendPortals(apexIndex, i, closestEndPos, path, ref straightPath,
                                                     ref straightPathFlags, ref straightPathRefs, ref straightPathCount,
                                                     maxStraightPath, options);
                            }
                            stat = AppendVertex(closestEndPos, 0, path[i], ref straightPath, ref straightPathFlags,
                                                ref straightPathRefs, ref straightPathCount, maxStraightPath);

                            return Status.Success | Status.PartialResult |
                                   ((straightPathCount >= maxStraightPath) ? Status.BufferTooSmall : 0);
                        }

                        if (i == 0)
                        {
                            float t;
                            if (
                                Helper.DistancePtSegSqr2D(portalApex[0], portalApex[1], portalApex[2], left[0], left[1],
                                                          left[2], right[0], right[1], right[2], out t) <
                                0.001f*0.001f)
                                continue;
                        }
                    }
                    else
                    {
                        Array.Copy(closestEndPos, left, 3);
                        Array.Copy(closestEndPos, right, 3);

                        fromType = toType = NavMeshBuilder.PolyTypeGround;
                    }

                    if (Helper.TriArea2D(portalApex, portalRight, right) <= 0.0f)
                    {
                        if (Helper.VEqual(portalApex[0], portalApex[1], portalApex[2], portalRight[0], portalRight[1], portalRight[2]) ||
                            Helper.TriArea2D(portalApex, portalLeft, right) > 0.0f)
                        {
                            Array.Copy(right, portalRight, 3);
                            rightPolyRef = (i + 1 < pathSize) ? path[i + 1] : 0;
                            rightPolyType = toType;
                            rightIndex = i;
                        }
                        else
                        {
                            if ((options & (StraightPathAreaCrossings | StraightPathAllCrossings)) != 0)
                            {
                                stat = AppendPortals(apexIndex, leftIndex, portalLeft, path, ref straightPath,
                                                     ref straightPathFlags, ref straightPathRefs, ref straightPathCount,
                                                     maxStraightPath, options);
                                if (stat != Status.InProgress)
                                    return stat;
                            }

                            Array.Copy(portalLeft, portalApex, 3);
                            apexIndex = leftIndex;

                            short flags = 0;
                            if (leftPolyRef <= 0)
                                flags = StraightPathEnd;
                            else if (leftPolyType == NavMeshBuilder.PolyTypeOffMeshConnection)
                                flags = StraightPathOffMeshConnection;
                            long refId = leftPolyRef;

                            stat = AppendVertex(portalApex, flags, refId, ref straightPath, ref straightPathFlags,
                                                ref straightPathRefs, ref straightPathCount, maxStraightPath);
                            if (stat != Status.InProgress)
                                return stat;

                            Array.Copy(portalApex, portalLeft, 3);
                            Array.Copy(portalApex, portalRight, 3);
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            i = apexIndex;

                            continue;
                        }
                    }

                    if (Helper.TriArea2D(portalApex, portalLeft, left) >= 0.0f)
                    {
                        if (Helper.VEqual(portalApex[0], portalApex[1], portalApex[2], portalLeft[0], portalLeft[1],
                                          portalLeft[2]) || Helper.TriArea2D(portalApex, portalRight, left) < 0.0f)
                        {
                            Array.Copy(left, portalLeft, 3);
                            leftPolyRef = (i + 1 < pathSize) ? path[i + 1] : 0;
                            leftPolyType = toType;
                            leftIndex = i;
                        }
                        else
                        {
                            if ((options & (StraightPathAreaCrossings | StraightPathAllCrossings)) != 0)
                            {
                                stat = AppendPortals(apexIndex, rightIndex, portalRight, path, ref straightPath,
                                                     ref straightPathFlags, ref straightPathRefs, ref straightPathCount,
                                                     maxStraightPath, options);
                                if (stat != Status.InProgress)
                                    return stat;
                            }

                            Array.Copy(portalRight, portalApex, 3);
                            apexIndex = rightIndex;

                            short flags = 0;
                            if (rightPolyRef <= 0)
                                flags = StraightPathEnd;
                            else if (rightPolyType == NavMeshBuilder.PolyTypeOffMeshConnection)
                                flags = StraightPathOffMeshConnection;
                            long refId = rightPolyRef;

                            stat = AppendVertex(portalApex, flags, refId, ref straightPath, ref straightPathFlags,
                                                ref straightPathRefs, ref straightPathCount, maxStraightPath);
                            if (stat != Status.InProgress)
                                return stat;

                            Array.Copy(portalApex, portalLeft, 3);
                            Array.Copy(portalApex, portalRight, 3);
                            leftIndex = apexIndex;
                            rightIndex = apexIndex;

                            i = apexIndex;

                            continue;
                        }
                    }
                }

                if ((options & (StraightPathAreaCrossings | StraightPathAllCrossings)) != 0)
                {
                    stat = AppendPortals(apexIndex, pathSize - 1, closestEndPos, path, ref straightPath,
                                         ref straightPathFlags, ref straightPathRefs, ref straightPathCount,
                                         maxStraightPath, options);
                    if (stat != Status.InProgress)
                        return stat;
                }
            }
	        stat = AppendVertex(closestEndPos, StraightPathEnd, 0, ref straightPath, ref straightPathFlags, ref straightPathRefs,
	                            ref straightPathCount, maxStraightPath);

            return Status.Success | ((straightPathCount >= maxStraightPath) ? Status.BufferTooSmall : 0);
	    }

        public Status InitSlicedFindPath(long startRef, long endRef, float[] startPos, float[] endPos, QueryFilter filter)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");
            if (NodePool == null)
                throw new ApplicationException("NodePool is not initialized");
            if (_openList == null)
                throw new ApplicationException("OpenList is not initialized");

            _query = new QueryData();
            _query.Status = Status.Failure;
            _query.StartRef = startRef;
            _query.EndRef = endRef;
            Array.Copy(startPos, _query.StartPos, 3);
            Array.Copy(endPos, _query.EndPos, 3);
            _query.Filter = filter;

            if(startRef <= 0 || endRef <= 0)
                return Status.Failure | Status.InvalidParam;

            if(!NavMesh.IsValidPolyRef(startRef) || !NavMesh.IsValidPolyRef(endRef))
                return Status.Failure | Status.InvalidParam;

            if (startRef == endRef)
            {
                _query.Status = Status.Success;
                return Status.Success;
            }

            NodePool.Clear();
            _openList.Clear();

            Node startNode = NodePool.GetNode(startRef);
            Array.Copy(startPos, startNode.Pos, 3);
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = Helper.VDist(startPos[0], startPos[1], startPos[2], endPos[0], endPos[1], endPos[2])*HScale;
            startNode.Id = startRef;
            startNode.Flags = Node.NodeOpen;
            _openList.Push(startNode);
                
            _query.Status = Status.InProgress;
            _query.LastBestNode = startNode;
            _query.LastBestNodeCost = startNode.Total;

            return _query.Status;
        }

        public Status UpdateSlicedFindPath(int maxIter, ref int doneIters)
        {
            if ((_query.Status & Status.InProgress) == 0)
                return _query.Status;

            if (!NavMesh.IsValidPolyRef(_query.StartRef) || !NavMesh.IsValidPolyRef(_query.EndRef))
            {
                _query.Status = Status.Failure;
                return Status.Failure;
            }

            int iter = 0;
            while (iter < maxIter && !_openList.Empty())
            {
                iter++;

                Node bestNode = _openList.Pop();
                bestNode.Flags &= ~Node.NodeOpen;
                bestNode.Flags |= Node.NodeClosed;

                if (bestNode.Id == _query.EndRef)
                {
                    _query.LastBestNode = bestNode;
                    Status details = _query.Status & Status.DetailMask;
                    _query.Status = Status.Success | details;
                    doneIters = iter;
                    return _query.Status;
                }

                long bestRef = bestNode.Id;
                MeshTile bestTile = null;
                Poly bestPoly = null;
                if ((NavMesh.GetTileAndPolyByRef(bestRef, ref bestTile, ref bestPoly) & Status.Failure) != 0)
                {
                    _query.Status = Status.Failure;
                    doneIters = iter;
                    return _query.Status;
                }

                long parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                if (bestNode.PIdx > 0)
                    parentRef = NodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                if (parentRef > 0)
                {
                    if ((NavMesh.GetTileAndPolyByRef(parentRef, ref parentTile, ref parentPoly) & Status.Failure) != 0)
                    {
                        _query.Status = Status.Failure;
                        doneIters = iter;
                        return _query.Status;
                    }
                }

                for (long i = bestPoly.FirstLink; i != NavMesh.NullLink; i = bestTile.Links[i].Next)
                {
                    long neighborRef = bestTile.Links[i].Ref;

                    if (neighborRef <= 0 || neighborRef == parentRef)
                        continue;

                    MeshTile neighborTile = null;
                    Poly neighborPoly = null;
                    NavMesh.GetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    if (!_query.Filter.PassFilter(neighborRef, neighborTile, neighborPoly))
                        continue;

                    Node neighborNode = NodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                    {
                        _query.Status |= Status.OutOfNodes;
                        continue;
                    }

                    if (neighborNode.Flags == 0)
                    {
                        float[] tempPos = new float[3];
                        GetEdgeMidPoint(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile,
                                        ref tempPos);
                        Array.Copy(tempPos, neighborNode.Pos, 3);
                    }

                    float cost = 0;
                    float heuristic = 0;

                    if (neighborRef == _query.EndRef)
                    {
                        float curCost = _query.Filter.GetCost(bestNode.Pos[0], bestNode.Pos[1], bestNode.Pos[2],
                                                              neighborNode.Pos[0], neighborNode.Pos[1], neighborNode.Pos[2],
                                                              parentRef, parentTile, parentPoly,
                                                              bestRef, bestTile, bestPoly,
                                                              neighborRef, neighborTile, neighborPoly);
                        float endCost = _query.Filter.GetCost(neighborNode.Pos[0], neighborNode.Pos[1], neighborNode.Pos[2],
                                                              _query.EndPos[0], _query.EndPos[1], _query.EndPos[2],
                                                              bestRef, bestTile, bestPoly,
                                                              neighborRef, neighborTile, neighborPoly,
                                                              0, null, null);
                        cost = bestNode.Cost + curCost + endCost;
                        heuristic = 0;
                    }
                    else
                    {
                        float curCost = _query.Filter.GetCost(bestNode.Pos[0], bestNode.Pos[1], bestNode.Pos[2],
                                                              neighborNode.Pos[0], neighborNode.Pos[1], neighborNode.Pos[2],
                                                              parentRef, parentTile, parentPoly,
                                                              bestRef, bestTile, bestPoly,
                                                              neighborRef, neighborTile, neighborPoly);
                        cost = bestNode.Cost + curCost;
                        heuristic =
                            Helper.VDist(neighborNode.Pos[0], neighborNode.Pos[1], neighborNode.Pos[2], _query.EndPos[0],
                                         _query.EndPos[1], _query.EndPos[2])*HScale;
                    }

                    float total = cost + heuristic;

                    if ((neighborNode.Flags & Node.NodeOpen) != 0 && total >= neighborNode.Total)
                        continue;
                    if ((neighborNode.Flags & Node.NodeClosed) != 0 && total >= neighborNode.Total)
                        continue;

                    neighborNode.PIdx = NodePool.GetNodeIdx(bestNode);
                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = (neighborNode.Flags & ~Node.NodeClosed);
                    neighborNode.Cost = cost;
                    neighborNode.Total = total;

                    if ((neighborNode.Flags & Node.NodeOpen) != 0)
                    {
                        _openList.Modify(neighborNode);
                    }
                    else
                    {
                        neighborNode.Flags |= Node.NodeOpen;
                        _openList.Push(neighborNode);
                    }

                    if (heuristic < _query.LastBestNodeCost)
                    {
                        _query.LastBestNodeCost = heuristic;
                        _query.LastBestNode = neighborNode;
                    }
                }
            }
            if (_openList.Empty())
            {
                Status details = _query.Status & Status.DetailMask;
                _query.Status = Status.Success | details;
            }

            doneIters = iter;

            return _query.Status;
        }

        public Status FinalizeSlicedFindPath(ref long[] path, ref int pathCount, int maxPath)
        {
            pathCount = 0;

            if ((_query.Status & Status.Failure) != 0)
            {
                _query = new QueryData();
                return Status.Failure;
            }

            int n = 0;

            if (_query.StartRef == _query.EndRef)
            {
                path[n++] = _query.StartRef;
            }
            else
            {
                if(_query.LastBestNode == null)
                    throw new ApplicationException("Query has no last best node");

                if(_query.LastBestNode.Id != _query.EndRef)
                    _query.Status |= Status.PartialResult;
                Node prev = null;
                Node node = _query.LastBestNode;
                do
                {
                    Node next = NodePool.GetNodeAtIdx(node.PIdx);
                    node.PIdx = NodePool.GetNodeIdx(prev);
                    prev = node;
                    node = next;
                } while (node != null);

                node = prev;
                do
                {
                    path[n++] = node.Id;
                    if (n >= maxPath)
                    {
                        _query.Status |= Status.BufferTooSmall;
                        break;
                    }
                    node = NodePool.GetNodeAtIdx(node.PIdx);
                } while (node != null);
            }

            Status details = _query.Status & Status.DetailMask;

            _query = new QueryData();
            pathCount = n;

            return Status.Success | details;
        }

        public Status FinalizeSlicedFindPathPartial(long[] existing, int existingSize, ref long[] path, ref int pathCount, int maxPath)
        {
            pathCount = 0;
            if(existingSize == 0)
                return Status.Failure;

            if ((_query.Status & Status.Failure) != 0)
            {
                _query = new QueryData();
                return Status.Failure;
            }

            int n = 0;

            if (_query.StartRef == _query.EndRef)
            {
                path[n++] = _query.StartRef;
            }
            else
            {
                Node prev = null;
                Node node = null;
                for (int i = existingSize-1; i >= 0; i--)
                {
                    node = NodePool.FindNode(existing[i]);
                    if (node != null)
                        break;
                }

                if (node == null)
                {
                    _query.Status |= Status.PartialResult;
                    if(_query.LastBestNode == null)
                        throw new ApplicationException("Query Last Best Node is not initialized");
                    node = _query.LastBestNode;
                }

                do
                {
                    Node next = NodePool.GetNodeAtIdx(node.PIdx);
                    node.PIdx = NodePool.GetNodeIdx(prev);
                    prev = node;
                    node = next;
                } while (node != null);

                node = prev;
                do
                {
                    path[n++] = node.Id;
                    if (n >= maxPath)
                    {
                        _query.Status |= Status.BufferTooSmall;
                        break;
                    }
                    node = NodePool.GetNodeAtIdx(node.PIdx);
                } while (node != null);
            }

            Status details = _query.Status & Status.DetailMask;

            _query = new QueryData();

            pathCount = n;

            return Status.Success | details;
        }

        public Status FindPolysAroundCircle(long startRef, float[] centerPos, float radius, QueryFilter filter,
                                            ref long[] resultRef, ref long[] resultParent, float[] resultCost,
                                            ref int resultCount, int maxResult)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");
            if (NodePool == null)
                throw new ApplicationException("NodePool is not initialized");
            if (_openList == null)
                throw new ApplicationException("OpenList is not initialized");

            resultCount = 0;

            if(startRef <= 0 || !NavMesh.IsValidPolyRef(startRef))
                return Status.Failure | Status.InvalidParam;

            NodePool.Clear();
            _openList.Clear();

            Node startNode = NodePool.GetNode(startRef);
            Array.Copy(centerPos, startNode.Pos, 3);
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = Node.NodeOpen;
            _openList.Push(startNode);

            Status status = Status.Success;

            int n = 0;
            if (n < maxResult)
            {
                resultRef[n] = startNode.Id;
                resultParent[n] = 0;
                resultCost[n] = 0;
                n++;
            }
            else
            {
                status |= Status.BufferTooSmall;
            }

            float radiusSqr = radius*radius;
            while (!_openList.Empty())
            {
                Node bestNode = _openList.Pop();
                bestNode.Flags &= ~Node.NodeOpen;
                bestNode.Flags |= Node.NodeClosed;

                long bestRef = bestNode.Id;
                MeshTile bestTile = null;
                Poly bestPoly = null;
                NavMesh.GetTileAndPolyByRefUnsafe(bestRef, out bestTile, out bestPoly);

                long parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;
                if (bestNode.PIdx != 0)
                    parentRef = NodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                if(parentRef > 0)
                    NavMesh.GetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);

                for (long i = bestPoly.FirstLink; i != NavMesh.NullLink; i = bestTile.Links[i].Next)
                {
                    Link link = bestTile.Links[i];
                    long neighborRef = link.Ref;

                    if (neighborRef <= 0 || neighborRef == parentRef)
                        continue;

                    MeshTile neighborTile = null;
                    Poly neighborPoly = null;
                    NavMesh.GetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    if (!filter.PassFilter(neighborRef, neighborTile, neighborPoly))
                        continue;

                    float[] va = new float[3], vb = new float[3];
                    if (GetPortalPoints(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile, ref va, ref vb) != Status.Success)
                        continue;

                    float tseg;
                    float distSqr = Helper.DistancePtSegSqr2D(centerPos[0], centerPos[1], centerPos[2], va[0], va[1], va[2], vb[0], vb[1], vb[2], out tseg);
                    if (distSqr > radiusSqr)
                        continue;

                    Node neighborNode = NodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                    {
                        status |= Status.OutOfNodes;
                        continue;
                    }

                    if ((neighborNode.Flags & Node.NodeClosed) != 0)
                        continue;

                    if (neighborNode.Flags == 0)
                    {
                        float[] temp = new float[3];
                        Helper.VLerp(ref temp, va[0], va[1], va[2], vb[0], vb[1], vb[2], 0.5f);
                        Array.Copy(temp, neighborNode.Pos, 3);
                    }

                    float total = bestNode.Total + Helper.VDist(bestNode.Pos[0], bestNode.Pos[1], bestNode.Pos[2], neighborNode.Pos[0], neighborNode.Pos[1], neighborNode.Pos[2]);

                    if ((neighborNode.Flags & Node.NodeOpen) != 0 && total >= neighborNode.Total)
                        continue;

                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = (neighborNode.Flags & ~Node.NodeClosed);
                    neighborNode.PIdx = NodePool.GetNodeIdx(bestNode);
                    neighborNode.Total = total;

                    if ((neighborNode.Flags & Node.NodeOpen) != 0)
                    {
                        _openList.Modify(neighborNode);
                    }
                    else
                    {
                        if (n < maxResult)
                        {
                            resultRef[n] = neighborNode.Id;
                            resultParent[n] = NodePool.GetNodeAtIdx(neighborNode.PIdx).Id;
                            resultCost[n] = neighborNode.Total;
                            n++;
                        }
                        else
                        {
                            status |= Status.BufferTooSmall;
                        }
                        neighborNode.Flags = Node.NodeOpen;
                        _openList.Push(neighborNode);
                    }
                }
            }
            resultCount = n;

            return status;
        }

        public Status FindPolysAroundShape(long startRef, float[] verts, int nverts, QueryFilter filter,
                                           ref long[] resultRef, ref long[] resultParent, ref float[] resultCost,
                                           ref int resultCount, int maxResult)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");
            if (NodePool == null)
                throw new ApplicationException("NodePool is not initialized");
            if (_openList == null)
                throw new ApplicationException("OpenList is not initialized");

            resultCount = 0;

            if(startRef <= 0 || !NavMesh.IsValidPolyRef(startRef))
                return Status.Failure | Status.InvalidParam;

            NodePool.Clear();
            _openList.Clear();

            float[] centerPos = {0, 0, 0};
            for (int i = 0; i < nverts; i++)
            {
                centerPos = Helper.VAdd(centerPos[0], centerPos[1], centerPos[2], verts[i*3 + 0], verts[i*3 + 1], verts[i*3 + 2]);
            }

            centerPos = Helper.VScale(centerPos[0], centerPos[1], centerPos[2], 1.0f/nverts);

            Node startNode = NodePool.GetNode(startRef);
            Array.Copy(centerPos, startNode.Pos, 3);
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = Node.NodeOpen;
            _openList.Push(startNode);

            Status status = Status.Success;

            int n = 0;
            if (n < maxResult)
            {
                resultRef[n] = startNode.Id;
                resultParent[n] = 0;
                resultCost[n] = 0;
                n++;
            }
            else
            {
                status |= Status.BufferTooSmall;
            }

            while (!_openList.Empty())
            {
                Node bestNode = _openList.Pop();
                bestNode.Flags &= Node.NodeOpen;
                bestNode.Flags |= Node.NodeClosed;

                long bestRef = bestNode.Id;
                MeshTile bestTile = null;
                Poly bestPoly = null;

                NavMesh.GetTileAndPolyByRefUnsafe(bestRef, out bestTile, out bestPoly);

                long parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;

                if (bestNode.PIdx > 0)
                    parentRef = NodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                if(parentRef > 0)
                    NavMesh.GetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);

                for (long i = bestPoly.FirstLink; i != NavMesh.NullLink; i = bestTile.Links[i].Next)
                {
                    Link link = bestTile.Links[i];
                    long neighborRef = link.Ref;

                    if (neighborRef <= 0 || neighborRef == parentRef)
                        continue;

                    MeshTile neighborTile = null;
                    Poly neighborPoly = null;
                    NavMesh.GetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    float[] va = new float[3], vb = new float[3];
                    if (GetPortalPoints(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile, ref va, ref vb) != Status.Success)
                        continue;

                    float tmin, tmax;
                    int segMin, segMax;
                    if (!Helper.IntersectSegmentPoly2D(va, vb, verts, nverts, out tmin, out tmax, out segMin, out segMax))
                        continue;
                    if (tmin > 1.0f || tmax < 0.0f)
                        continue;

                    Node neighborNode = NodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                    {
                        status |= Status.OutOfNodes;
                        continue;
                    }

                    if ((neighborNode.Flags & Node.NodeClosed) != 0)
                        continue;

                    if (neighborNode.Flags == 0)
                    {
                        float[] temp = new float[3];
                        Helper.VLerp(ref temp, va[0], va[1], va[2], vb[0], vb[1], vb[2], 0.5f);
                        Array.Copy(temp, neighborNode.Pos, 3);
                    }

                    float total = bestNode.Total + Helper.VDist(bestNode.Pos[0], bestNode.Pos[1], bestNode.Pos[2], neighborNode.Pos[0], neighborNode.Pos[1], neighborNode.Pos[2]);

                    if((neighborNode.Flags & Node.NodeOpen) != 0 && total >= neighborNode.Total)
                        continue;

                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = (neighborNode.Flags & ~Node.NodeClosed);
                    neighborNode.PIdx = NodePool.GetNodeIdx(bestNode);
                    neighborNode.Total = total;

                    if ((neighborNode.Flags & Node.NodeOpen) != 0)
                    {
                        _openList.Modify(neighborNode);
                    }
                    else
                    {
                        if (n < maxResult)
                        {
                            resultRef[n] = neighborNode.Id;
                            resultParent[n] = NodePool.GetNodeAtIdx(neighborNode.PIdx).Id;
                            resultCost[n] = neighborNode.Total;
                            n++;
                        }
                        else
                        {
                            status |= Status.BufferTooSmall;
                        }
                        neighborNode.Flags = Node.NodeOpen;
                        _openList.Push(neighborNode);
                    }
                }
            }

            resultCount = n;

            return status;
        }

        public Status FindNearestPoly(float[] center, float[] extents, QueryFilter filter, ref long nearestRef,
                                      ref float[] nearestPt)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");

            nearestRef = 0;
            long[] polys = new long[128];

            int polyCount = 0;
            if((QueryPolygons(center, extents, filter, ref polys, ref polyCount, 128) & Status.Failure) != 0)
                return Status.Failure | Status.InvalidParam;

            long nearest = 0;
            float nearestDistanceSqr = float.MaxValue;
            for (int i = 0; i < polyCount; i++)
            {
                long refId = polys[i];
                float[] closestPtPoly = new float[3];
                ClosestPointOnPoly(refId, center, ref closestPtPoly);
                float d = Helper.VDistSqr(center[0], center[1], center[2], closestPtPoly[0], closestPtPoly[1],
                                          closestPtPoly[2]);
                if (d < nearestDistanceSqr)
                {
                    if (nearestPt != null)
                    {
                        Array.Copy(closestPtPoly, nearestPt, 3);
                    }
                    nearestDistanceSqr = d;
                    nearest = refId;
                }
            }

            nearestRef = nearest;

            return Status.Success;
        }

        public Status QueryPolygons(float[] center, float[] extents, QueryFilter filter, ref long[] polys,
                                    ref int polyCount, int maxPolys)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");

            float[] bmin = Helper.VSub(center[0], center[1], center[2], extents[0], extents[1], extents[2]);
            float[] bmax = Helper.VAdd(center[0], center[1], center[2], extents[0], extents[1], extents[2]);

            int minx, miny, maxx, maxy;
            NavMesh.CalcTileLoc(bmin[0], bmin[1], bmin[2], out minx, out miny);
            NavMesh.CalcTileLoc(bmax[0], bmax[1], bmax[2], out maxx, out maxy);

            int MaxNeis = 32;
            MeshTile[] neis = new MeshTile[MaxNeis];

            int n = 0;
            for (int y = miny; y <= maxy; y++)
            {
                for (int x = minx; x <= maxx; x++)
                {
                    int nneis = NavMesh.GetTilesAt(x, y, ref neis, MaxNeis);
                    for (int j = 0; j < nneis; j++)
                    {
                        long[] tempPolys = new long[maxPolys-n];
                        int tempn = QueryPolygonsInTile(neis[j], bmin, bmax, filter, ref tempPolys, maxPolys - n);
                        for (int i = 0; i < tempn; i++)
                        {
                            polys[n + i] = tempPolys[i];
                        }
                        n += tempn;
                        if (n >= maxPolys)
                        {
                            polyCount = n;
                            return Status.Success | Status.BufferTooSmall;
                        }
                    }
                }
            }
            polyCount = n;

            return Status.Success;
        }

        public Status FindLocalNeighbourhood(long startRef, float[] centerPos, float radius, QueryFilter filter,
                                             ref long[] resultRef, ref long[] resultParent, ref int resultCount,
                                             int maxResult)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");
            if (_tinyNodePool == null)
                throw new ApplicationException("tinyNodePool is not initialized");
            if(filter == null)
                throw new ArgumentException("QueryFilter cannot be null");
            resultCount = 0;

            if(startRef <= 0 || !NavMesh.IsValidPolyRef(startRef))
                return Status.Failure | Status.InvalidParam;

            int MaxStack = 48;
            Node[] stack = new Node[MaxStack];
            int nstack = 0;

            _tinyNodePool.Clear();

            Node startNode = _tinyNodePool.GetNode(startRef);
            startNode.PIdx = 0;
            startNode.Id = startRef;
            startNode.Flags = Node.NodeClosed;
            stack[nstack++] = startNode;

            float radiusSqr = radius*radius;

            float[] pa = new float[NavMeshBuilder.VertsPerPoly * 3];
            float[] pb = new float[NavMeshBuilder.VertsPerPoly * 3];

            Status status = Status.Success;

            int n = 0;
            if (n < maxResult)
            {
                resultRef[n] = startNode.Id;
                if(resultParent != null)
                    resultParent[n] = 0;
                n++;
            }
            else
            {
                status |= Status.BufferTooSmall;
            }

            while (nstack > 0)
            {
                Node curNode = stack[0];
                for (int i = 0; i < nstack-1; i++)
                {
                    stack[i] = stack[i + 1];
                }
                nstack--;

                long curRef = curNode.Id;

                MeshTile curTile = null;
                Poly curPoly = null;
                NavMesh.GetTileAndPolyByRefUnsafe(curRef, out curTile, out curPoly);

                for (long i = curPoly.FirstLink; i != NavMesh.NullLink; i = curTile.Links[i].Next)
                {
                    Link link = curTile.Links[i];

                    long neighborRef = link.Ref;

                    if (neighborRef <= 0)
                        continue;

                    Node neighborNode = _tinyNodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                        continue;
                    if ((neighborNode.Flags & Node.NodeClosed) != 0)
                        continue;

                    MeshTile neighborTile = null;
                    Poly neighborPoly = null;
                    NavMesh.GetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);
                    if (neighborPoly.Type == NavMeshBuilder.PolyTypeOffMeshConnection)
                        continue;

                    if (!filter.PassFilter(neighborRef, neighborTile, neighborPoly))
                        continue;

                    float[] va = new float[3], vb = new float[3];
                    if(GetPortalPoints(curRef, curPoly, curTile, neighborRef, neighborPoly, neighborTile, ref va, ref vb)!= Status.Success)
                        continue;

                    float tseg;
                    float distSqr = Helper.DistancePtSegSqr2D(centerPos[0], centerPos[1], centerPos[2], va[0], va[1], va[2], vb[0], vb[1], vb[2], out tseg);
                    if (distSqr > radiusSqr)
                        continue;

                    neighborNode.Flags |= Node.NodeClosed;
                    neighborNode.PIdx = _tinyNodePool.GetNodeIdx(curNode);

                    int npa = neighborPoly.VertCount;
                    for (int k = 0; k < npa; k++)
                    {
                        Array.Copy(neighborTile.Verts, neighborPoly.Verts[k]*3, pa, k*3, 3);
                    }

                    bool overlap = false;
                    for (int j = 0; j < n; j++)
                    {
                        long pastRef = resultRef[j];

                        bool connected = false;
                        for (long k = curPoly.FirstLink; k != NavMesh.NullLink; k=curTile.Links[k].Next)
                        {
                            if (curTile.Links[k].Ref == pastRef)
                            {
                                connected = true;
                                break;
                            }
                        }
                        if (connected)
                            continue;

                        MeshTile pastTile = null;
                        Poly pastPoly = null;
                        NavMesh.GetTileAndPolyByRefUnsafe(pastRef, out pastTile, out pastPoly);
                        if(pastTile == null || pastPoly == null)
                            throw new Exception("past is null");
                        int npb = pastPoly.VertCount;
                        for (int k = 0; k < npb; k++)
                        {
                            Array.Copy(pastTile.Verts, pastPoly.Verts[k]*3, pb, k*3, 3);
                        }

                        if (Helper.OverlapPolyPoly2D(pa, npa, pb, npb))
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (overlap)
                        continue;
                    if (n < maxResult)
                    {
                        resultRef[n] = neighborRef;
                        if(resultParent != null)
                            resultParent[n] = curRef;
                        n++;
                    }
                    else
                    {
                        status |= Status.BufferTooSmall;
                    }

                    if (nstack < MaxStack)
                    {
                        stack[nstack++] = neighborNode;
                    }
                }
            }

            resultCount = n;
            return status;
        }

        public Status MoveAlongSurface(long startRef, float[] startPos, float[] endPos, QueryFilter filter,
                                       ref float[] resultPos, ref long[] visited, ref int visitedCount,
                                       int maxVisitedSize)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");
            if (_tinyNodePool == null)
                throw new ApplicationException("tinyNodePool is not initialized");

            visitedCount = 0;

            if(startRef <= 0)
                return Status.Failure | Status.InvalidParam;
            if(!NavMesh.IsValidPolyRef(startRef))
                return Status.Failure | Status.InvalidParam;

            Status status = Status.Success;

            int MaxStack = 48;
            Node[] stack = new Node[MaxStack];
            int nstack = 0;

            _tinyNodePool.Clear();

            Node startNode = _tinyNodePool.GetNode(startRef);
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = Node.NodeClosed;
            stack[nstack++] = startNode;

            float[] bestPos = new float[3];
            float bestDist = float.MaxValue;
            Node bestNode = null;
            Array.Copy(startPos, bestPos, 3);

            float[] searchPos = new float[3];
            float searchRadSqr;
            Helper.VLerp(ref searchPos, startPos[0], startPos[1], startPos[2], endPos[0], endPos[1], endPos[2], 0.5f);
            searchRadSqr = Helper.VDist(startPos[0], startPos[1], startPos[2], endPos[0], endPos[1], endPos[2])/2.0f +
                           0.001f;
            searchRadSqr *= searchRadSqr;

            float[] verts = new float[NavMeshBuilder.VertsPerPoly*3];

            while (nstack > 0)
            {
                Node curNode = stack[0];
                for (int i = 0; i < nstack-1; i++)
                {
                    stack[i] = stack[i + 1];
                }
                nstack--;

                long curRef = curNode.Id;
                MeshTile curTile = null;
                Poly curPoly = null;
                NavMesh.GetTileAndPolyByRefUnsafe(curRef, out curTile, out curPoly);

                int nverts = curPoly.VertCount;
                for (int i = 0; i < nverts; i++)
                {
                    Array.Copy(curTile.Verts, curPoly.Verts[i]*3, verts, i*3, 3);
                }

                if (Helper.PointInPolygon(endPos[0], endPos[1], endPos[2], verts, nverts))
                {
                    bestNode = curNode;
                    Array.Copy(endPos, bestPos, 3);
                    break;
                }

                for (int i = 0, j = curPoly.VertCount-1; i < curPoly.VertCount; j=i++)
                {
                    int MaxNeis = 8;
                    int nneis = 0;
                    long[] neis = new long[MaxNeis];

                    if ((curPoly.Neis[j] & NavMeshBuilder.ExtLink) != 0)
                    {
                        for (long k = curPoly.FirstLink; k != NavMesh.NullLink; k= curTile.Links[k].Next)
                        {
                            Link link = curTile.Links[k];
                            if (link.Edge == j)
                            {
                                if (link.Ref != 0)
                                {
                                    MeshTile neiTile = null;
                                    Poly neiPoly = null;
                                    NavMesh.GetTileAndPolyByRefUnsafe(link.Ref, out neiTile, out neiPoly);
                                    if (filter.PassFilter(link.Ref, neiTile, neiPoly))
                                    {
                                        if (nneis < MaxNeis)
                                            neis[nneis++] = link.Ref;
                                    }
                                }
                            }
                        }
                    }
                    else if (curPoly.Neis[j] > 0)
                    {
                        long idx = curPoly.Neis[j] - 1;
                        long refId = NavMesh.GetPolyRefBase(curTile) | idx;
                        if (filter.PassFilter(refId, curTile, curTile.Polys[idx]))
                        {
                            neis[nneis++] = refId;
                        }
                    }

                    if (nneis == 0)
                    {
                        int vj = j*3;
                        int vi = i*3;
                        float tseg;
                        float distSqr = Helper.DistancePtSegSqr2D(endPos[0], endPos[1], endPos[2], verts[vj + 0],
                                                                  verts[vj + 1], verts[vj + 2], verts[vi + 0],
                                                                  verts[vi + 1], verts[vi + 2], out tseg);
                        if (distSqr < bestDist)
                        {
                            Helper.VLerp(ref bestPos, verts[vj + 0], verts[vj + 1], verts[vj + 2], verts[vi + 0], verts[vi + 1], verts[vi + 2], tseg);
                            bestDist = distSqr;
                            bestNode = curNode;
                        }
                    }
                    else
                    {
                        for (int k = 0; k < nneis; k++)
                        {
                            Node neighborNode = _tinyNodePool.GetNode(neis[k]);
                            if (neighborNode == null)
                                continue;
                            if ((neighborNode.Flags & Node.NodeClosed) != 0)
                                continue;

                            int vj = j*3;
                            int vi = i*3;
                            float tseg;
                            float distSqr = Helper.DistancePtSegSqr2D(searchPos[0], searchPos[1], searchPos[2],
                                                                      verts[vj + 0], verts[vj + 1], verts[vj + 2],
                                                                      verts[vi + 0], verts[vi + 1], verts[vi + 2],
                                                                      out tseg);
                            if (distSqr > searchRadSqr)
                                continue;

                            if (nstack < MaxStack)
                            {
                                neighborNode.PIdx = _tinyNodePool.GetNodeIdx(curNode);
                                neighborNode.Flags |= Node.NodeClosed;
                                stack[nstack++] = neighborNode;
                            }
                        }
                    }
                }
            }

            int n = 0;
            if (bestNode != null)
            {
                Node prev = null;
                Node node = bestNode;
                do
                {
                    Node next = _tinyNodePool.GetNodeAtIdx(node.PIdx);
                    node.PIdx = _tinyNodePool.GetNodeIdx(prev);
                    prev = node;
                    node = next;
                } while (node != null);

                node = prev;
                do
                {
                    visited[n++] = node.Id;
                    if (n >= maxVisitedSize)
                    {
                        status |= Status.BufferTooSmall;
                    }
                    node = _tinyNodePool.GetNodeAtIdx(node.PIdx);
                } while (node != null);
            }
            Array.Copy(bestPos, resultPos, 3);
            visitedCount = n;
            return status;
        }

        public Status Raycast(long startRef, float[] startPos, float[] endPos, QueryFilter filter, ref float t,
                              ref float[] hitNormal, ref long[] path, ref int pathCount, int maxPath)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");
            t = 0;
            pathCount = 0;
            if(startRef <= 0 || !NavMesh.IsValidPolyRef(startRef))
                return Status.Failure | Status.InvalidParam;

            long curRef = startRef;
            float[] verts = new float[3*NavMeshBuilder.VertsPerPoly];
            int n = 0;

            hitNormal[0] = 0;
            hitNormal[1] = 0;
            hitNormal[2] = 0;

            Status status = Status.Success;

            while (curRef > 0)
            {
                MeshTile tile = null;
                Poly poly = null;
                NavMesh.GetTileAndPolyByRefUnsafe(curRef, out tile, out poly);

                int nv = 0;
                for (int i = 0; i < poly.VertCount; i++)
                {
                    Array.Copy(tile.Verts, poly.Verts[i]*3, verts, nv*3, 3);
                    nv++;
                }

                float tmin, tmax;
                int segMin, segMax;
                if (!Helper.IntersectSegmentPoly2D(startPos, endPos, verts, nv, out tmin, out tmax, out segMin,
                                                  out segMax))
                {
                    pathCount = n;
                    return status;
                }

                if (tmax > t)
                    t = tmax;

                if (n < maxPath)
                {
                    path[n++] = curRef;
                }
                else
                {
                    status |= Status.BufferTooSmall;
                }

                if (segMax == -1)
                {
                    t = float.MaxValue;
                    pathCount = n;
                    return status;
                }

                long nextRef = 0;

                for (long i = poly.FirstLink; i != NavMesh.NullLink; i = tile.Links[i].Next)
                {
                    Link link = tile.Links[i];

                    if (link.Edge != segMax)
                        continue;

                    MeshTile nextTile = null;
                    Poly nextPoly = null;

                    NavMesh.GetTileAndPolyByRefUnsafe(link.Ref, out nextTile, out nextPoly);

                    if (nextPoly.Type == NavMeshBuilder.PolyTypeOffMeshConnection)
                        continue;

                    if (!filter.PassFilter(link.Ref, nextTile, nextPoly))
                        continue;

                    if (link.Side == 0xff)
                    {
                        nextRef = link.Ref;
                        break;
                    }

                    if (link.BMin == 0 && link.BMax == 255)
                    {
                        nextRef = link.Ref;
                        break;
                    }

                    int left = poly.Verts[link.Edge]*3;
                    int right = poly.Verts[(link.Edge + 1)%poly.VertCount]*3;

                    if (link.Side == 0 || link.Side == 4)
                    {
                        float s = 1.0f/255.0f;
                        float lmin = tile.Verts[left + 2] + (tile.Verts[right + 2] - tile.Verts[left + 2])*(link.BMin*s);
                        float lmax = tile.Verts[left + 2] + (tile.Verts[right + 2] - tile.Verts[left + 2]) * (link.BMax * s);
                        if (lmin > lmax)
                        {
                            float temp = lmin;
                            lmin = lmax;
                            lmax = temp;
                        }

                        float z = startPos[2] + (endPos[2] - startPos[2])*tmax;
                        if (z >= lmin && z <= lmax)
                        {
                            nextRef = link.Ref;
                            break;
                        }
                    }
                    else if(link.Side == 2 || link.Side == 6)
                    {
                        float s = 1.0f / 255.0f;
                        float lmin = tile.Verts[left + 0] + (tile.Verts[right + 0] - tile.Verts[left + 0]) * (link.BMin * s);
                        float lmax = tile.Verts[left + 0] + (tile.Verts[right + 0] - tile.Verts[left + 0]) * (link.BMax * s);
                        if (lmin > lmax)
                        {
                            float temp = lmin;
                            lmin = lmax;
                            lmax = temp;
                        }

                        float z = startPos[0] + (endPos[0] - startPos[0]) * tmax;
                        if (z >= lmin && z <= lmax)
                        {
                            nextRef = link.Ref;
                            break;
                        }
                    }
                }

                if (nextRef == 0)
                {
                    int a = segMax;
                    int b = segMax + 1 < nv ? segMax + 1 : 0;
                    float dx = verts[b*3 + 0] - verts[a*3 + 0];
                    float dz = verts[b*3 + 2] - verts[a*3 + 2];
                    hitNormal[0] = dz;
                    hitNormal[1] = 0;
                    hitNormal[2] = -dx;
                    Helper.VNormalize(ref hitNormal);

                    pathCount = n;
                    return status;
                }
                curRef = nextRef;
            }
            pathCount = n;
            return status;
        }

        public Status FindDistanceToWall(long startRef, float[] centerPos, float maxRadius, QueryFilter filter,
                                         ref float hitDist, ref float[] hitPos, ref float[] hitNormal)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");
            if (NodePool == null)
                throw new ApplicationException("NodePool is not initialized");
            if (_openList == null)
                throw new ApplicationException("OpenList is not initialized");

            if(startRef <= 0 || !NavMesh.IsValidPolyRef(startRef))
                return Status.Failure|Status.InvalidParam;

            NodePool.Clear();
            _openList.Clear();

            Node startNode = NodePool.GetNode(startRef);
            Array.Copy(centerPos, startNode.Pos, 3);
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = Node.NodeOpen;
            _openList.Push(startNode);

            float radiusSqr = maxRadius*maxRadius;

            Status status = Status.Success;

            while (!_openList.Empty())
            {
                Node bestNode = _openList.Pop();
                bestNode.Flags &= ~Node.NodeOpen;
                bestNode.Flags |= Node.NodeClosed;

                long bestRef = bestNode.Id;
                MeshTile bestTile = null;
                Poly bestPoly = null;

                NavMesh.GetTileAndPolyByRefUnsafe(bestRef, out bestTile, out bestPoly);

                long parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;

                if (bestNode.PIdx > 0)
                    parentRef = NodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                if(parentRef>0)
                    NavMesh.GetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);

                for (int i = 0, j = bestPoly.VertCount-1; i < bestPoly.VertCount; j=i++)
                {
                    if ((bestPoly.Neis[j] & NavMeshBuilder.ExtLink) != 0)
                    {
                        bool solid = true;
                        for (long k = bestPoly.FirstLink; k < NavMesh.NullLink; k=bestTile.Links[k].Next)
                        {
                            Link link = bestTile.Links[k];
                            if (link.Edge == j)
                            {
                                if (link.Ref != 0)
                                {
                                    MeshTile neiTile = null;
                                    Poly neiPoly = null;
                                    NavMesh.GetTileAndPolyByRefUnsafe(link.Ref, out neiTile, out neiPoly);
                                    if (filter.PassFilter(link.Ref, neiTile, neiPoly))
                                        solid = false;
                                }
                                break;
                            }
                        }
                        if (!solid) continue;
                    }
                    else if (bestPoly.Neis[j] > 0)
                    {
                        long idx = bestPoly.Neis[j] - 1;
                        long refId = NavMesh.GetPolyRefBase(bestTile) | idx;
                        if (filter.PassFilter(refId, bestTile, bestTile.Polys[idx]))
                            continue;
                    }

                    int vj = bestPoly.Verts[j]*3;
                    int vi = bestPoly.Verts[i]*3;
                    float tseg;
                    float distSqr = Helper.DistancePtSegSqr2D(centerPos[0], centerPos[1], centerPos[2],
                                                              bestTile.Verts[vj + 0], bestTile.Verts[vj + 1],
                                                              bestTile.Verts[vj + 2], bestTile.Verts[vi + 0],
                                                              bestTile.Verts[vi + 1], bestTile.Verts[vi + 2], out tseg);

                    if (distSqr > radiusSqr)
                        continue;

                    radiusSqr = distSqr;

                    hitPos[0] = bestTile.Verts[vj + 0] + (bestTile.Verts[vi + 0] - bestTile.Verts[vj + 0]) * tseg;
                    hitPos[0] = bestTile.Verts[vj + 1] + (bestTile.Verts[vi + 1] - bestTile.Verts[vj + 1]) * tseg;
                    hitPos[0] = bestTile.Verts[vj + 2] + (bestTile.Verts[vi + 2] - bestTile.Verts[vj + 2]) * tseg;
                }

                for (long i = bestPoly.FirstLink; i != NavMesh.NullLink; i++)
                {
                    Link link = bestTile.Links[i];
                    long neighborRef = link.Ref;

                    if (neighborRef <= 0 || neighborRef == parentRef)
                        continue;

                    MeshTile neighborTile = null;
                    Poly neighborPoly = null;
                    NavMesh.GetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    if (neighborPoly.Type == NavMeshBuilder.PolyTypeOffMeshConnection)
                        continue;

                    int va = bestPoly.Verts[link.Edge]*3;
                    int vb = bestPoly.Verts[(link.Edge + 1)%bestPoly.VertCount]*3;
                    float tseg;
                    float distSqr = Helper.DistancePtSegSqr2D(centerPos[0], centerPos[1], centerPos[2],
                                                              bestTile.Verts[va + 0], bestTile.Verts[va + 1],
                                                              bestTile.Verts[va + 2], bestTile.Verts[vb + 0],
                                                              bestTile.Verts[vb + 1], bestTile.Verts[vb + 2], out tseg);
                    if (distSqr > radiusSqr)
                        continue;

                    if (!filter.PassFilter(neighborRef, neighborTile, neighborPoly))
                        continue;

                    Node neighborNode = NodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                    {
                        status |= Status.OutOfNodes;
                        continue;
                    }

                    if ((neighborNode.Flags & Node.NodeClosed) != 0)
                        continue;

                    if (neighborNode.Flags == 0)
                    {
                        float[] temp = new float[3];
                        GetEdgeMidPoint(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile, ref temp);
                        Array.Copy(temp, neighborNode.Pos, 3);
                    }

                    float total = bestNode.Total + Helper.VDist(bestNode.Pos[0], bestNode.Pos[1], bestNode.Pos[2], neighborNode.Pos[0], neighborNode.Pos[1], neighborNode.Pos[2]);

                    if ((neighborNode.Flags & Node.NodeOpen) != 0 && total >= neighborNode.Total)
                        continue;

                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = (neighborNode.Flags & ~Node.NodeClosed);
                    neighborNode.PIdx = NodePool.GetNodeIdx(bestNode);
                    neighborNode.Total = total;

                    if ((neighborNode.Flags & Node.NodeOpen) != 0)
                    {
                        _openList.Modify(neighborNode);
                    }
                    else
                    {
                        neighborNode.Flags |= Node.NodeOpen;
                        _openList.Push(neighborNode);
                    }
                }
            }

            hitNormal = Helper.VSub(centerPos[0], centerPos[1], centerPos[2], hitPos[0], hitPos[1], hitPos[2]);
            Helper.VNormalize(ref hitNormal);

            hitDist = (float)Math.Sqrt(radiusSqr);

            return status;
        }

        public Status GetPolyWallSegments(long refId, QueryFilter filter, ref float[] segmentVerts, ref long[] segmentRefs,
                                          ref int segmentCount, int maxSegments)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");

            segmentCount = 0;

            MeshTile tile = null;
            Poly poly = null;
            if((NavMesh.GetTileAndPolyByRef(refId, ref tile, ref poly) & Status.Failure) != 0)
                return Status.Failure | Status.InvalidParam;

            int n = 0;
            int MaxInterval = 16;
            SegInterval[] ints = new SegInterval[MaxInterval];
            for (int i = 0; i < MaxInterval; i++)
            {
                ints[i] = new SegInterval();
            }
            int nints;

            bool storePortals = segmentRefs != null;

            Status status = Status.Success;

            for (int i = 0, j = poly.VertCount-1; i < poly.VertCount; j=i++)
            {
                nints = 0;
                if ((poly.Neis[j] & NavMeshBuilder.ExtLink) != 0)
                {
                    for (long k = poly.FirstLink; k != NavMesh.NullLink; k=tile.Links[k].Next)
                    {
                        Link link = tile.Links[k];
                        if (link.Edge == j)
                        {
                            if (link.Ref != 0)
                            {
                                MeshTile neiTile = null;
                                Poly neiPoly = null;
                                NavMesh.GetTileAndPolyByRefUnsafe(link.Ref, out neiTile, out neiPoly);
                                if (filter.PassFilter(link.Ref, neiTile, neiPoly))
                                {
                                    InsertInterval(ref ints, ref nints, MaxInterval, link.BMin, link.BMax, link.Ref);
                                }
                            }
                        }
                    }
                }
                else
                {
                    long neiRef = 0;
                    if (poly.Neis[j] > 0)
                    {
                        long idx = poly.Neis[j] - 1;
                        neiRef = NavMesh.GetPolyRefBase(tile) | idx;
                        if (!filter.PassFilter(neiRef, tile, tile.Polys[idx]))
                            neiRef = 0;
                    }

                    if (neiRef != 0 && !storePortals)
                        continue;

                    if (n < maxSegments)
                    {
                        int vj1 = poly.Verts[j]*3;
                        int vi1 = poly.Verts[i]*3;
                        Array.Copy(tile.Verts, vj1, segmentVerts, n*6, 3);
                        Array.Copy(tile.Verts, vi1, segmentVerts, n*6+3, 3);
                        if (segmentRefs != null)
                            segmentRefs[n] = neiRef;
                        n++;
                    }
                    else
                    {
                        status |= Status.BufferTooSmall;
                    }
                    continue;
                }

                InsertInterval(ref ints, ref nints, MaxInterval, -1, 0, 0);
                InsertInterval(ref ints, ref nints, MaxInterval, 255, 256, 0);

                int vj = poly.Verts[j]*3;
                int vi = poly.Verts[i]*3;
                for (int k = 1; k < nints; k++)
                {
                    if (storePortals && ints[k].RefId > 0)
                    {
                        float tmin = ints[k].TMin/255.0f;
                        float tmax = ints[k].TMax/255.0f;
                        if (n < maxSegments)
                        {
                            float[] temp = new float[3];
                            Helper.VLerp(ref temp, tile.Verts[vj + 0], tile.Verts[vj + 1], tile.Verts[vj + 2], tile.Verts[vi + 0], tile.Verts[vi + 1], tile.Verts[vi + 2], tmin);
                            Array.Copy(temp, 0, segmentVerts, n*6, 3);
                            Helper.VLerp(ref temp, tile.Verts[vj + 0], tile.Verts[vj + 1], tile.Verts[vj + 2], tile.Verts[vi + 0], tile.Verts[vi + 1], tile.Verts[vi + 2], tmax);
                            Array.Copy(temp, 0, segmentVerts, n * 6+3, 3);
                            if (segmentRefs != null)
                                segmentRefs[n] = ints[k].RefId;
                            n++;
                        }
                        else
                        {
                            status |= Status.BufferTooSmall;
                        }
                    }
                    int imin = ints[k - 1].TMax;
                    int imax = ints[k].TMin;
                    if (imin != imax)
                    {
                        float tmin = imin / 255.0f;
                        float tmax = imax / 255.0f;
                        if (n < maxSegments)
                        {
                            float[] temp = new float[3];
                            Helper.VLerp(ref temp, tile.Verts[vj + 0], tile.Verts[vj + 1], tile.Verts[vj + 2], tile.Verts[vi + 0], tile.Verts[vi + 1], tile.Verts[vi + 2], tmin);
                            Array.Copy(temp, 0, segmentVerts, n * 6, 3);
                            Helper.VLerp(ref temp, tile.Verts[vj + 0], tile.Verts[vj + 1], tile.Verts[vj + 2], tile.Verts[vi + 0], tile.Verts[vi + 1], tile.Verts[vi + 2], tmax);
                            Array.Copy(temp, 0, segmentVerts, n * 6 + 3, 3);
                            if (segmentRefs != null)
                                segmentRefs[n] = 0;
                            n++;
                        }
                        else
                        {
                            status |= Status.BufferTooSmall;
                        }

                    }
                }
            }
            segmentCount = n;
            return status;
        }

        public static void InsertInterval(ref SegInterval[] ints, ref int nints, int maxInts, short tmin, short tmax, long refId)
        {
            if (nints + 1 > maxInts) return;

            int idx = 0;
            while (idx < nints)
            {
                if (tmax <= ints[idx].TMin)
                    break;
                idx++;
            }

            if (nints - idx > 0)
            {
                Array.Copy(ints, idx, ints, idx+1, nints-idx);
            }
            ints[idx].RefId = refId;
            ints[idx].TMin = tmin;
            ints[idx].TMax = tmax;
            nints++;
        }

        public Status FindRandomPoint(QueryFilter filter, RandFunc func, ref long randomRef, ref float[] randomPt)
        {
            if(NavMesh == null)
                return Status.Failure;

            MeshTile tile = null;
            float tsum = 0.0f;
            for (int i = 0; i < NavMesh.GetMaxTiles(); i++)
            {
                MeshTile temp = NavMesh.GetTile(i);
                if (temp == null || temp.Header == null)
                    continue;

                float area = 1.0f;
                tsum += area;
                float u = func();
                if (u*tsum <= area)
                    tile = temp;
            }
            if(tile == null)
                return Status.Failure;

            Poly poly = null;
            long polyRef = 0;
            long baseRef = (int)NavMesh.GetPolyRefBase(tile);

            float areaSum = 0.0f;
            for (int i = 0; i < tile.Header.PolyCount; i++)
            {
                Poly p = tile.Polys[i];

                if (p.Type != NavMeshBuilder.PolyTypeGround)
                    continue;

                long refId = baseRef | i;
                if(!filter.PassFilter(refId, tile, p))
                    continue;

                float polyArea = 0.0f;
                for (int j = 2; j < p.VertCount; j++)
                {
                    float[] va = new float[3], vb = new float[3], vc = new float[3];
                    Array.Copy(tile.Verts, p.Verts[0] * 3, va, 0, 3);
                    Array.Copy(tile.Verts, p.Verts[j-1] * 3, vb, 0, 3);
                    Array.Copy(tile.Verts, p.Verts[j] * 3, vc, 0, 3);
                    polyArea += Helper.TriArea2D(va, vb, vc);
                }
                areaSum += polyArea;
                float u = func();
                if (u*areaSum <= polyArea)
                {
                    poly = p;
                    polyRef = refId;
                }
            }
            if(poly == null)
                return Status.Failure;

            int v = poly.Verts[0]*3;
            float[] verts = new float[3*NavMeshBuilder.VertsPerPoly];
            float[] areas = new float[NavMeshBuilder.VertsPerPoly];
            Array.Copy(tile.Verts, v, verts, 0, 3);
            for (int j = 1; j < poly.VertCount; j++)
            {
                v = poly.Verts[j]*3;
                Array.Copy(tile.Verts, v, verts, j*3, 3);
            }

            float s = func();
            float t = func();

            float[] pt = new float[3];
            Helper.RandomPointInConvexPoly(verts, poly.VertCount, ref areas, s, t, ref pt);

            float h = 0.0f;
            Status status = GetPolyHeight(polyRef, pt, ref h);
            if ((status & Status.Failure) != 0)
                return status;
            pt[1] = h;

            Array.Copy(pt, 0, randomPt, 0, 3);
            randomRef = polyRef;

            return Status.Success;
        }

        public Status FindRandomPointAroundCircle(long startRef, float[] centerPos, float radius, QueryFilter filter,
                                                  RandFunc frand, ref long randomRef, ref float[] randomPt)
        {
            if(NavMesh == null || NodePool == null || _openList == null)
                return Status.Failure;

            if(startRef <= 0 || !NavMesh.IsValidPolyRef(startRef))
                return Status.Failure | Status.InvalidParam;

            MeshTile startTile = null;
            Poly startPoly = null;
            NavMesh.GetTileAndPolyByRefUnsafe(startRef, out startTile, out startPoly);
            if(!filter.PassFilter(startRef, startTile, startPoly))
                return Status.Failure | Status.InvalidParam;

            NodePool.Clear();
            _openList.Clear();

            Node startNode = NodePool.GetNode(startRef);
            Array.Copy(centerPos, 0, startNode.Pos, 0, 3);
            startNode.PIdx = 0;
            startNode.Cost = 0;
            startNode.Total = 0;
            startNode.Id = startRef;
            startNode.Flags = Node.NodeOpen;
            _openList.Push(startNode);

            Status status = Status.Success;

            float radiusSqr = (radius*radius);
            float areaSum = 0.0f;

            MeshTile randomTile = null;
            Poly randomPoly = null;
            long randomPolyRef = 0;

            while (!_openList.Empty())
            {
                Node bestNode = _openList.Pop();
                bestNode.Flags &= Node.NodeOpen;
                bestNode.Flags |= Node.NodeClosed;

                long bestRef = bestNode.Id;
                MeshTile bestTile = null;
                Poly bestPoly = null;
                NavMesh.GetTileAndPolyByRefUnsafe(bestRef, out bestTile, out bestPoly);

                if (bestPoly.Type == NavMeshBuilder.PolyTypeGround)
                {
                    float polyArea = 0.0f;
                    for (int j = 2; j < bestPoly.VertCount; j++)
                    {
                        float[] va = new float[3], vb = new float[3], vc = new float[3];
                        Array.Copy(bestTile.Verts, bestPoly.Verts[0] * 3, va, 0, 3);
                        Array.Copy(bestTile.Verts, bestPoly.Verts[j - 1] * 3, vb, 0, 3);
                        Array.Copy(bestTile.Verts, bestPoly.Verts[j] * 3, vc, 0, 3);
                        polyArea += Helper.TriArea2D(va, vb, vc);
                    }
                    areaSum += polyArea;
                    float u = frand();
                    if (u*areaSum <= polyArea)
                    {
                        randomTile = bestTile;
                        randomPoly = bestPoly;
                        randomPolyRef = bestRef;
                    }
                }

                long parentRef = 0;
                MeshTile parentTile = null;
                Poly parentPoly = null;

                if (bestNode.PIdx > 0)
                {
                    parentRef = NodePool.GetNodeAtIdx(bestNode.PIdx).Id;
                }
                if (parentRef > 0)
                {
                    NavMesh.GetTileAndPolyByRefUnsafe(parentRef, out parentTile, out parentPoly);
                }

                for (long i = bestPoly.FirstLink; i != NavMesh.NullLink; i = bestTile.Links[i].Next)
                {
                    Link link = bestTile.Links[i];
                    long neighborRef = link.Ref;
                    if (neighborRef <= 0 || neighborRef == parentRef)
                        continue;

                    MeshTile neighborTile = null;
                    Poly neighborPoly = null;
                    NavMesh.GetTileAndPolyByRefUnsafe(neighborRef, out neighborTile, out neighborPoly);

                    if (!filter.PassFilter(neighborRef, neighborTile, neighborPoly))
                        continue;

                    float[] va = new float[3], vb = new float[3];
                    if ((GetPortalPoints(bestRef, bestPoly, bestTile, neighborRef, neighborPoly, neighborTile, ref va, ref vb) & Status.Failure) != 0)
                        continue;

                    float tseg;
                    float distSqr = Helper.DistancePtSegSqr2D(centerPos[0], centerPos[1], centerPos[2], va[0], va[1], va[2], vb[0], vb[1], vb[2], out tseg);
                    if (distSqr > radiusSqr)
                        continue;

                    Node neighborNode = NodePool.GetNode(neighborRef);
                    if (neighborNode == null)
                    {
                        status |= Status.OutOfNodes;
                        continue;
                    }

                    if ((neighborNode.Flags & Node.NodeClosed) != 0)
                        continue;

                    if (neighborNode.Flags == 0)
                    {
                        float[] pos = new float[3];
                        Helper.VLerp(ref pos, va[0], va[1], va[2], vb[0], vb[1], vb[2], 0.5f);
                        Array.Copy(pos, 0, neighborNode.Pos, 0, 3);
                    }

                    float total = bestNode.Total + Helper.VDist(bestNode.Pos[0], bestNode.Pos[1], bestNode.Pos[2], neighborNode.Pos[0], neighborNode.Pos[1], neighborNode.Pos[2]);

                    if ((neighborNode.Flags & Node.NodeOpen) != 0 && total >= neighborNode.Total)
                        continue;

                    neighborNode.Id = neighborRef;
                    neighborNode.Flags = (neighborNode.Flags & ~Node.NodeClosed);
                    neighborNode.PIdx = NodePool.GetNodeIdx(bestNode);
                    neighborNode.Total = total;

                    if ((neighborNode.Flags & Node.NodeOpen) != 0)
                    {
                        _openList.Modify(neighborNode);
                    }
                    else
                    {
                        neighborNode.Flags = Node.NodeOpen;
                        _openList.Push(neighborNode);
                    }
                }
            }

            if(randomPoly == null)
                return Status.Failure;

            int v = randomPoly.Verts[0]*3;
            float[] verts = new float[3*NavMeshBuilder.VertsPerPoly];
            float[] areas = new float[NavMeshBuilder.VertsPerPoly];
            Array.Copy(randomTile.Verts, v, verts, 0, 3);
            for (int j = 1; j < randomPoly.VertCount; j++)
            {
                v = randomPoly.Verts[j]*3;
                Array.Copy(randomTile.Verts, v, verts, j*3, 3);
            }

            float s = frand();
            float t = frand();

            float[] pt = new float[3];
            Helper.RandomPointInConvexPoly(verts, randomPoly.VertCount, ref areas, s, t, ref pt);

            float h = 0.0f;
            Status stat = GetPolyHeight(randomPolyRef, pt, ref h);
            if ((status & Status.Failure) != 0)
                return stat;

            pt[1] = h;

            Array.Copy(pt, 0, randomPt, 0, 3);
            randomRef = randomPolyRef;

            return Status.Success;
        }

        public Status ClosestPointOnPoly(long refId, float[] pos, ref float[] closest)
        {
            if(NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");

            MeshTile tile = null;
            Poly poly = null;
            if((NavMesh.GetTileAndPolyByRef(refId, ref tile, ref poly) & Status.Failure) != 0)
                return Status.Failure | Status.InvalidParam;
            if(tile == null)
                return Status.Failure | Status.InvalidParam;
            ClosestPointOnPolyInTile(tile, poly, pos, ref closest);

            return Status.Success;
        }

        public Status ClosestPointOnPolyBoundary(long refId, float[] pos, ref float[] closest)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");
            MeshTile tile = null;
            Poly poly = null;
            if ((NavMesh.GetTileAndPolyByRef(refId, ref tile, ref poly) & Status.Failure) != 0)
                return Status.Failure | Status.InvalidParam;

            float[] verts = new float[3*NavMeshBuilder.VertsPerPoly];
            float[] edged = new float[NavMeshBuilder.VertsPerPoly];
            float[] edget = new float[NavMeshBuilder.VertsPerPoly];
            int nv = 0;
            for (int i = 0; i < poly.VertCount; i++)
            {
                Array.Copy(tile.Verts, poly.Verts[i]*3, verts, nv*3, 3);
                nv++;
            }

            bool inside = Helper.DistancePtPolyEdgesSqr(pos[0], pos[1], pos[2], verts, nv, ref edged, ref edget);
            if (inside)
            {
                Array.Copy(pos, closest, 3);
            }
            else
            {
                float dmin = float.MaxValue;
                int imin = -1;
                for (int i = 0; i < nv; i++)
                {
                    if (edged[i] < dmin)
                    {
                        dmin = edged[i];
                        imin = i;
                    }
                }
                int va = imin*3;
                int vb = ((imin + 1)%nv)*3;
                Helper.VLerp(ref closest, verts[va + 0], verts[va + 1], verts[va + 2], verts[vb + 0], verts[vb + 1], verts[vb + 2], edget[imin]);
            }
            return Status.Success;
        }

        public Status GetPolyHeight(long refId, float[] pos, ref float height)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");

            MeshTile tile = null;
            Poly poly = null;
            if((NavMesh.GetTileAndPolyByRef(refId, ref tile, ref poly) & Status.Failure) != 0)
                return Status.Failure | Status.InvalidParam;

            if (poly.Type == NavMeshBuilder.PolyTypeOffMeshConnection)
            {
                int v0 = poly.Verts[0]*3;
                int v1 = poly.Verts[1]*3;
                float d0 = Helper.VDist(pos[0], pos[1], pos[2], tile.Verts[v0 + 0], tile.Verts[v0 + 1], tile.Verts[v0 + 2]);
                float d1 = Helper.VDist(pos[0], pos[1], pos[2], tile.Verts[v1 + 0], tile.Verts[v1 + 1], tile.Verts[v1 + 2]);
                float u = d0/(d0 + d1);
                height = tile.Verts[v0 + 1] + (tile.Verts[v1 + 1] - tile.Verts[v0 + 1])*u;

                return Status.Success;
            }
            else
            {
                long ip = 0;
                for (int i = 0; i < tile.Polys.Length; i++)
                {
                    if (tile.Polys[i] == poly)
                    {
                        ip = i;
                        break;
                    }
                }
                PolyDetail pd = tile.DetailMeshes[ip];
                for (int j = 0; j < pd.TriCount; j++)
                {
                    int t = ((int)pd.TriBase + j)*4;
                    float[] v = new float[9];
                    for (int k = 0; k < 3; k++)
                    {
                        if (tile.DetailTris[t + k] < poly.VertCount)
                        {
                            Array.Copy(tile.Verts, poly.Verts[tile.DetailTris[t + k]] * 3, v, k*3, 3);
                        }
                        else
                        {
                            Array.Copy(tile.DetailVerts, (pd.VertBase + (tile.DetailTris[t + k]-poly.VertCount))*3, v, k * 3, 3);                            
                        }
                    }
                    float h = 0.0f;
                    if (Helper.ClosestHeightPointTriangle(pos[0], pos[1], pos[2], v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7], v[8], ref h))
                    {
                        height = h;
                        return Status.Success;
                    }
                }
            }

            return Status.Failure | Status.InvalidParam;
        }

        public bool IsValidPolyRef(long refId, QueryFilter filter)
        {
            MeshTile tile = null;
            Poly poly = null;

            Status status = NavMesh.GetTileAndPolyByRef(refId, ref tile, ref poly);
            if ((status & Status.Failure) != 0)
                return false;

            if (!filter.PassFilter(refId, tile, poly))
                return false;
            return true;
        }

        public bool IsInClosedList(long refId)
        {
            if (NodePool == null) return false;
            Node node = NodePool.FindNode(refId);
            return node != null && (node.Flags & Node.NodeClosed) != 0;
        }

        public bool IsInOpenList(long refId)
        {
            if (NodePool == null) return false;
            Node node = NodePool.FindNode(refId);
            return node != null && (node.Flags & Node.NodeOpen) != 0;
        }

        private int QueryPolygonsInTile(MeshTile tile, float[] qmin, float[] qmax, QueryFilter filter, ref long[] polys,
                                        int maxPolys)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");

            if (tile.BVTree != null)
            {
                int node = 0;
                int end = tile.Header.BVNodeCount;
                float[] tbmin = tile.Header.BMin;
                float[] tbmax = tile.Header.BMax;
                float qfac = tile.Header.BVQuantFactor;

                int[] bmin = new int[3], bmax = new int[3];

                float minx = Helper.Clamp(qmin[0], tbmin[0], tbmax[0]) - tbmin[0];
                float miny = Helper.Clamp(qmin[1], tbmin[1], tbmax[1]) - tbmin[1];
                float minz = Helper.Clamp(qmin[2], tbmin[2], tbmax[2]) - tbmin[2];
                float maxx = Helper.Clamp(qmax[0], tbmin[0], tbmax[0]) - tbmin[0];
                float maxy = Helper.Clamp(qmax[1], tbmin[1], tbmax[1]) - tbmin[1];
                float maxz = Helper.Clamp(qmax[2], tbmin[2], tbmax[2]) - tbmin[2];

                bmin[0] = (int)(qfac * minx) & 0xfffe;
                bmin[1] = (int)(qfac * miny) & 0xfffe;
                bmin[2] = (int)(qfac * minz) & 0xfffe;
                bmax[0] = (int)(qfac * maxx + 1) | 1;
                bmax[1] = (int)(qfac * maxy + 1) | 1;
                bmax[2] = (int)(qfac * maxz + 1) | 1;

                long baseRef = NavMesh.GetPolyRefBase(tile);
                int n = 0;
                while (node < end)
                {
                    bool overlap = Helper.OverlapQuantBounds(bmin, bmax, tile.BVTree[node].BMin, tile.BVTree[node].BMax);
                    bool isLeafNode = tile.BVTree[node].I >= 0;

                    if (isLeafNode && overlap)
                    {
                        long refId = baseRef | tile.BVTree[node].I;
                        if (filter.PassFilter(refId, tile, tile.Polys[tile.BVTree[node].I]))
                        {
                            if (n < maxPolys)
                                polys[n++] = refId;
                        }
                    }

                    if (overlap || isLeafNode)
                        node++;
                    else
                    {
                        int escapeIndex = -tile.BVTree[node].I;
                        node += escapeIndex;
                    }
                }
                return n;
            }
            else
            {
                float[] bmin = new float[3], bmax = new float[3];
                int n = 0;
                long baseRef = NavMesh.GetPolyRefBase(tile);
                for (int i = 0; i < tile.Header.PolyCount; i++)
                {
                    Poly p = tile.Polys[i];
                    if (p.Type == NavMeshBuilder.PolyTypeOffMeshConnection)
                        continue;

                    long refId = baseRef | i;
                    if (!filter.PassFilter(refId, tile, p))
                        continue;

                    int v = p.Verts[0]*3;
                    Array.Copy(tile.Verts, v, bmin, 0, 3);
                    Array.Copy(tile.Verts, v, bmax, 0, 3);
                    for (int j = 1; j < p.VertCount; j++)
                    {
                        v = p.Verts[j]*3;
                        Helper.VMin(ref bmin, tile.Verts[v + 0], tile.Verts[v + 1], tile.Verts[v + 2]);
                        Helper.VMax(ref bmax, tile.Verts[v + 0], tile.Verts[v + 1], tile.Verts[v + 2]);
                    }
                    if (Helper.OverlapBounds(qmin[0], qmin[1], qmin[2], qmax[0], qmax[1], qmax[2], bmin[0], bmin[1],
                                             bmin[2], bmax[0], bmax[1], bmax[2]))
                    {
                        if (n < maxPolys)
                            polys[n++] = refId;
                    }
                }
                return n;
            }
        }

        private long FindNearestPolyInTile(MeshTile tile, float[] center, float[] extents, QueryFilter filter,
                                          ref float[] nearestPt)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");

            float[] bmin = Helper.VSub(center[0], center[1], center[2], extents[0], extents[1], extents[2]);
            float[] bmax = Helper.VAdd(center[0], center[1], center[2], extents[0], extents[1], extents[2]);

            long[] polys = new long[128];
            int polyCount = QueryPolygonsInTile(tile, bmin, bmax, filter, ref polys, 128);

            long nearest = 0;
            float nearestDistanceSqr = float.MaxValue;
            for (int i = 0; i < polyCount; i++)
            {
                long refId = polys[i];
                Poly poly = tile.Polys[NavMesh.DecodePolyIdPoly(refId)];
                float[] closestPtPoly = new float[3];
                ClosestPointOnPolyInTile(tile, poly, center, ref closestPtPoly);

                float d = Helper.VDistSqr(center[0], center[1], center[2], closestPtPoly[0], closestPtPoly[1], closestPtPoly[2]);
                if (d < nearestDistanceSqr)
                {
                    Array.Copy(closestPtPoly, nearestPt, 3);
                    nearestDistanceSqr = d;
                    nearest = refId;
                }
            }
            return nearest;
        }

        private void ClosestPointOnPolyInTile(MeshTile tile, Poly poly, float[] pos, ref float[] closest)
        {
            if (poly.Type == NavMeshBuilder.PolyTypeOffMeshConnection)
            {
                int v0 = poly.Verts[0]*3;
                int v1 = poly.Verts[1]*3;
                float d0 = Helper.VDist(pos[0], pos[1], pos[2], tile.Verts[v0 + 0], tile.Verts[v0 + 1],
                                        tile.Verts[v0 + 2]);
                float d1 = Helper.VDist(pos[0], pos[1], pos[2], tile.Verts[v1 + 0], tile.Verts[v1 + 1],
                                        tile.Verts[v1 + 2]);

                float u = d0/(d0 + d1);
                Helper.VLerp(ref closest, tile.Verts[v0 + 0], tile.Verts[v0 + 1], tile.Verts[v0 + 2], tile.Verts[v1 + 0], tile.Verts[v1 + 1],tile.Verts[v1 + 2], u);
                return;
            }

            long ip = 0;
            for (int i = 0; i < tile.Polys.Length; i++)
            {
                if (tile.Polys[i] == poly)
                {
                    ip = i;
                    break;

                }
            }

            PolyDetail pd = tile.DetailMeshes[ip];

            float[] verts = new float[3*NavMeshBuilder.VertsPerPoly];
            float[] edged = new float[NavMeshBuilder.VertsPerPoly];
            float[] edget = new float[NavMeshBuilder.VertsPerPoly];
            int nv = poly.VertCount;
            for (int i = 0; i < nv; i++)
            {
                Array.Copy(tile.Verts, poly.Verts[i]*3, verts, i*3, 3);
            }

            Array.Copy(pos, closest, 3);
            if (!Helper.DistancePtPolyEdgesSqr(pos[0], pos[1], pos[2], verts, nv, ref edged, ref edget))
            {
                float dmin = float.MaxValue;
                int imin = -1;
                for (int i = 0; i < nv; i++)
                {
                    if (edged[i] < dmin)
                    {
                        dmin = edged[i];
                        imin = i;
                    }
                }
                int va = imin*3;
                int vb = ((imin + 1)%nv)*3;
                Helper.VLerp(ref closest, verts[va + 0], verts[va + 1], verts[va + 2], verts[vb + 0], verts[vb + 1], verts[vb + 2], edget[imin]);
            }

            for (int j = 0; j < pd.TriCount; j++)
            {
                int t = ((int)pd.TriBase + j)*4;
                float[] v = new float[9];
                for (int k = 0; k < 3; k++)
                {
                    if(tile.DetailTris[t+k] < poly.VertCount)
                        Array.Copy(tile.Verts, poly.Verts[tile.DetailTris[t+k]]*3, v, k*3, 3);
                    else
                    {
                        Array.Copy(tile.DetailVerts, (pd.VertBase + (tile.DetailTris[t + k]-poly.VertCount))*3, v, k * 3, 3);
                    }
                }
                float h = 0.0f;
                if (Helper.ClosestHeightPointTriangle(pos[0], pos[1], pos[2], v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7], v[8], ref h))
                {
                    closest[1] = h;
                    break;
                }
            }
        }

        private Status GetPortalPoints(long from, long to, ref float[] left, ref float[] right, ref short fromType,
                                       ref short toType)
        {
            if (NavMesh == null)
                throw new ApplicationException("NavMesh is not initialized");

            MeshTile fromTile = null;
            Poly fromPoly = null;
            if((NavMesh.GetTileAndPolyByRef(from, ref fromTile, ref fromPoly) & Status.Failure) != 0)
                return Status.Failure | Status.InvalidParam;
            fromType = fromPoly.Type;

            MeshTile toTile = null;
            Poly toPoly = null;
            if((NavMesh.GetTileAndPolyByRef(to, ref toTile, ref toPoly) & Status.Failure) != 0)
                return Status.Failure | Status.InvalidParam;
            toType = toPoly.Type;

            return GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, ref left, ref right);
        }

        private Status GetPortalPoints(long from, Poly fromPoly, MeshTile fromTile, long to, Poly toPoly, MeshTile toTile,
                                       ref float[] left, ref float[] right)
        {
            Link link = null;
            for (long i = fromPoly.FirstLink; i != NavMesh.NullLink; i = fromTile.Links[i].Next)
            {
                if (fromTile.Links[i].Ref == to)
                {
                    link = fromTile.Links[i];
                    break;
                }
            }
            if (link == null)
            {
                return Status.Failure | Status.InvalidParam;
            }

            if (fromPoly.Type == NavMeshBuilder.PolyTypeOffMeshConnection)
            {
                for (long i = fromPoly.FirstLink; i != NavMesh.NullLink; i = fromTile.Links[i].Next)
                {
                    if (fromTile.Links[i].Ref == to)
                    {
                        int v = fromTile.Links[i].Edge;
                        Array.Copy(fromTile.Verts, fromPoly.Verts[v]*3, left, 0, 3);
                        Array.Copy(fromTile.Verts, fromPoly.Verts[v] * 3, right, 0, 3);
                        return Status.Success;
                    }
                }
                return Status.Failure | Status.InvalidParam;
            }

            if (toPoly.Type == NavMeshBuilder.PolyTypeOffMeshConnection)
            {
                for (long i = toPoly.FirstLink; i != NavMesh.NullLink; i = toTile.Links[i].Next)
                {
                    if (toTile.Links[i].Ref == from)
                    {
                        int v = toTile.Links[i].Edge;
                        Array.Copy(toTile.Verts, toPoly.Verts[v] * 3, left, 0, 3);
                        Array.Copy(toTile.Verts, toPoly.Verts[v] * 3, right, 0, 3);
                        return Status.Success;
                    }
                }
                return Status.Failure | Status.InvalidParam;
            }
            int v0 = fromPoly.Verts[link.Edge];
            int v1 = fromPoly.Verts[(link.Edge + 1)%fromPoly.VertCount];
            Array.Copy(fromTile.Verts, v0*3, left, 0, 3);
            Array.Copy(fromTile.Verts, v1*3, right, 0, 3);

            if (link.Side != 0xff)
            {
                if (link.BMin != 0 || link.BMax != 255)
                {
                    float s = 1.0f/255.0f;
                    float tmin = link.BMin*s;
                    float tmax = link.BMax*s;
                    Helper.VLerp(ref left, fromTile.Verts[v0 * 3 + 0], fromTile.Verts[v0 * 3 + 1], fromTile.Verts[v0 * 3 + 2], fromTile.Verts[v1 + 3 + 0], fromTile.Verts[v1 + 3 + 1], fromTile.Verts[v1 + 3 + 2], tmin);
                    Helper.VLerp(ref right, fromTile.Verts[v0 * 3 + 0], fromTile.Verts[v0 * 3 + 1], fromTile.Verts[v0 * 3 + 2], fromTile.Verts[v1 + 3 + 0], fromTile.Verts[v1 + 3 + 1], fromTile.Verts[v1 + 3 + 2], tmax);
                }
            }
            return Status.Success;
        }

        private Status GetEdgeMidPoint(long from, long to, ref float[] mid)
        {
            float[] left = new float[3], right = new float[3];
            short fromType = 0, toType = 0;
            if ((GetPortalPoints(from, to, ref left, ref right, ref fromType, ref toType) & Status.Failure) != 0)
            {
                return Status.Failure |  Status.InvalidParam;
            }
            mid[0] = (left[0] + right[0]) * 0.5f;
            mid[1] = (left[1] + right[1]) * 0.5f;
            mid[2] = (left[2] + right[2]) * 0.5f;
            return Status.Success;
        }

        private Status GetEdgeMidPoint(long from, Poly fromPoly, MeshTile fromTile, long to, Poly toPoly, MeshTile toTile,
                                      ref float[] mid)
        {
            float[] left = new float[3], right = new float[3];
            short fromType = 0, toType = 0;
            if ((GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, ref left, ref right) & Status.Failure) != 0)
            {
                return Status.Failure | Status.InvalidParam;
            }
            mid[0] = (left[0] + right[0]) * 0.5f;
            mid[1] = (left[1] + right[1]) * 0.5f;
            mid[2] = (left[2] + right[2]) * 0.5f;
            return Status.Success;
        }

        private Status AppendVertex(float[] pos, short flags, long refId, ref float[] straightPath,
                                    ref short[] straightPathFlags, ref long[] straightPathRefs, ref int straightPathCount,
                                    int maxStraightPath)
        {
            if (straightPathCount > 0 &&
                Helper.VEqual(straightPath[(straightPathCount - 1)*3 + 0], straightPath[(straightPathCount - 1)*3 + 1],
                              straightPath[(straightPathCount - 1)*3 + 2], pos[0], pos[1], pos[2]))
            {
                straightPathFlags[straightPathCount - 1] = flags;
                straightPathRefs[straightPathCount - 1] = refId;
            }
            else
            {
                Array.Copy(pos, 0, straightPath, straightPathCount*3, 3);
                straightPathFlags[straightPathCount] = flags;
                straightPathRefs[straightPathCount] = refId;
                straightPathCount++;

                if (flags == StraightPathEnd || straightPathCount >= maxStraightPath)
                {
                    return Status.Success | ((straightPathCount >= maxStraightPath) ? Status.BufferTooSmall : 0);
                }
            }
            return Status.InProgress;
        }

        private Status AppendPortals(int startIdx, int endIdx, float[] endPos, long[] path, ref float[] straightPath,
                                     ref short[] straightPathFlags, ref long[] straightPathRefs, ref int straightPathCount,
                                     int maxStraightPath, int options)
        {
            int startPos = (straightPathCount - 1)*3;
            Status stat = 0;
            for (int i = startIdx; i < endIdx; i++)
            {
                long from = path[i];
                MeshTile fromTile = null;
                Poly fromPoly = null;
                if((NavMesh.GetTileAndPolyByRef(from, ref fromTile, ref fromPoly) & Status.Failure) != 0)
                    return Status.Failure | Status.InvalidParam;

                long to = path[i + 1];
                MeshTile toTile = null;
                Poly toPoly = null;
                if((NavMesh.GetTileAndPolyByRef(to, ref toTile, ref toPoly) & Status.Failure) != 0)
                    return Status.Failure | Status.InvalidParam;
                
                float[] left = new float[3], right = new float[3];
                if ((GetPortalPoints(from, fromPoly, fromTile, to, toPoly, toTile, ref left, ref right) & Status.Failure) != 0)
                    break;

                if ((options & StraightPathAreaCrossings) != 0)
                {
                    if (fromPoly.Area == toPoly.Area)
                        continue;
                }

                float s = 0.0f, t = 0.0f;
                if (Helper.IntersectSegSeg2D(straightPath[startPos + 0], straightPath[startPos + 1],
                                             straightPath[startPos + 2], endPos[0], endPos[1], endPos[2], left, right,
                                             ref s, ref t))
                {
                    float[] pt = new float[3];
                    Helper.VLerp(ref pt, left[0], left[1], left[2], right[0], right[1], right[2], t);

                    stat = AppendVertex(pt, 0, path[i + 1], ref straightPath, ref straightPathFlags,
                                        ref straightPathRefs, ref straightPathCount, maxStraightPath);
                    if (stat != Status.InProgress)
                        return stat;
                }
            }
            return Status.InProgress;
        }
	}
}
