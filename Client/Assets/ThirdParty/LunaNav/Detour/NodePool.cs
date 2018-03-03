using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LunaNav
{
    [Serializable]
	public class NodePool
	{
	    public int MaxNodes { get; private set; }
        public int HashSize { get; private set; }
	    private Node[] _nodes;
	    private int[] _first;
	    private int[] _next;
	    private int _nodeCount;

        public NodePool(int maxNodes, int hashSize)
        {
            if(hashSize != Helper.NextPow2(hashSize))
                throw new ArgumentException("Hash size must be a power of 2");
            if(maxNodes <= 0)
                throw new ArgumentException("Max nodes must be greater than 0");

            MaxNodes = maxNodes;
            HashSize = hashSize;
            _nodes = new Node[maxNodes];
            for (int i = 0; i < maxNodes; i++)
            {
                _nodes[i] = new Node();
            }
            _next = new int[maxNodes];
            _first = new int[hashSize];

            for (int i = 0; i < hashSize; i++)
            {
                _first[i] = Node.NullIdx;
            }
            for (int i = 0; i < maxNodes; i++)
            {
                _next[i] = Node.NullIdx;
            }
        }

        public void Clear()
        {
            _nodeCount = 0;
            for (int i = 0; i < HashSize; i++)
            {
                _first[i] = Node.NullIdx;
            }
        }

	    public Node GetNode(long id)
	    {
	        long bucket = Helper.HashRef(id) & (HashSize - 1);
	        int i = _first[bucket];
	        Node node = null;
	        while (i != Node.NullIdx)
	        {
	            if (_nodes[i].Id == id)
	                return _nodes[i];
	            i = _next[i];
	        }

	        if (_nodeCount >= MaxNodes)
	            return null;

	        i = _nodeCount;
	        _nodeCount++;

	        node = _nodes[i];
	        node.PIdx = 0;
	        node.Cost = 0;
	        node.Total = 0;
	        node.Id = id;
	        node.Flags = 0;

	        _next[i] = _first[bucket];
	        _first[bucket] = i;

	        return node;
	    }

	    public Node FindNode(long id)
	    {
	        long bucket = Helper.HashRef(id) & (HashSize - 1);
	        int i = _first[bucket];
	        while (i != Node.NullIdx)
	        {
	            if (_nodes[i].Id == id)
	                return _nodes[i];
	            i = _next[i];
	        }
	        return null;
	    }

	    public long GetNodeIdx(Node node)
        {
            if (node == null) return 0;
            for (int i = 0; i < _nodes.Length; i++)
            {
                if (node == _nodes[i])
                    return i+1;
            }
            return 0;
        }

        public Node GetNodeAtIdx(long idx)
        {
            if (idx <= 0) return null;

            return _nodes[idx - 1];
        }

        public int GetFirst(int bucket)
        {
            return _first[bucket];
        }
        public int GetNext(int i)
        {
            return _next[i];
        }
	}
}
