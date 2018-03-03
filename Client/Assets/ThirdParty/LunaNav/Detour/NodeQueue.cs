using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaNav
{
    [Serializable]
	public class NodeQueue
	{

        public NodeQueue(int n)
        {
            if(n <= 0)
                throw new ArgumentException("Capacity must be greater than 0");
            Capacity = n;
            _heap = new Node[n+1];
            _size = 0;
        }

        public void Clear()
        {
            _size = 0;
        }

	    public Node Top
	    {
            get { return _heap[0]; }
	    }

        public Node Pop()
        {
            Node result = _heap[0];
            _size--;
            TrickleDown(0, _heap[_size]);
            return result;
        }

        public void Push(Node node)
        {
            _size++;
            BubbleUp(_size - 1, node);
        }

        public void Modify(Node node)
        {
            for (int i = 0; i < _size; i++)
            {
                if (_heap[i] == node)
                {
                    BubbleUp(i, node);
                    return;
                }
            }
        }

        public bool Empty()
        {
            return _size == 0;
        }

        public int Capacity { get; private set; }

        private void BubbleUp(int i, Node node)
        {
            int parent = (i - 1)/2;
            while ((i > 0) && (_heap[parent].Total > node.Total))
            {
                _heap[i] = _heap[parent];
                i = parent;
                parent = (i - 1)/2;
            }
            _heap[i] = node;
        }

        public void TrickleDown(int i, Node node)
        {
            int child = (i*2) + 1;
            while (child < _size)
            {
                if (((child + 1) < _size) && (_heap[child].Total > _heap[child + 1].Total))
                {
                    child++;
                }
                _heap[i] = _heap[child];
                i = child;
                child = (i*2) + 1;
            }
            BubbleUp(i, node);
        }

	    private Node[] _heap;
	    private int _size;
	}
}
